#include "drone_drop.h"

#include <stdio.h>
#include <string.h>
#include <math.h>
#include <float.h>
#include <stdlib.h>

/* ------------------------------------------------------------------ */
/*  Compile-time constants                                              */
/* ------------------------------------------------------------------ */

#define LINE_BUF   256
#define FLOAT_EPS  0.001f
#define P95_Z      1.6449f

/* CSV column positions */
#define COL_ALT      0
#define COL_HSPEED   1
#define COL_VSPEED   2
#define COL_CRASH_X  6
#define COL_CRASH_Y  7
#define COL_CRASH_D  8

/* ------------------------------------------------------------------ */
/*  Internal types                                                      */
/* ------------------------------------------------------------------ */

typedef struct {
    float alt;
    float h_speed;
    float v_speed;
    float mean_x;
    float mean_y;
    float std_x;
    float std_y;
    float mean_dist;
    float p95_dist;
} LutEntry;

typedef struct {
    double   sum_x;
    double   sum_y;
    double   sum_x2;
    double   sum_y2;
    double   sum_d;
    double   sum_d2;
    uint32_t n;
} Accumulator;

/* ------------------------------------------------------------------ */
/*  Static storage — no heap allocation                                 */
/* ------------------------------------------------------------------ */

static LutEntry s_lut[DRONE_DROP_MAX_COMBOS];
static int      s_n_combos;

static float    s_alt_lv[DRONE_DROP_MAX_ALT_LEVELS];
static int      s_n_alt;
static float    s_hsp_lv[DRONE_DROP_MAX_HSPEED_LEVELS];
static int      s_n_hsp;
static float    s_vsp_lv[DRONE_DROP_MAX_VSPEED_LEVELS];
static int      s_n_vsp;

static int s_idx[DRONE_DROP_MAX_ALT_LEVELS]
                [DRONE_DROP_MAX_HSPEED_LEVELS]
                [DRONE_DROP_MAX_VSPEED_LEVELS];

static int s_ready;

/* ------------------------------------------------------------------ */
/*  Utility                                                             */
/* ------------------------------------------------------------------ */

static int fapprox(float a, float b)
{
    float d = a - b;
    return (d < 0.0f ? -d : d) < FLOAT_EPS;
}

static void sorted_insert(float *arr, int *n, int cap, float v)
{
    int i;
    for (i = 0; i < *n; i++)
        if (fapprox(arr[i], v)) return;
    if (*n >= cap) return;
    i = *n;
    while (i > 0 && arr[i - 1] > v) { arr[i] = arr[i - 1]; i--; }
    arr[i] = v;
    (*n)++;
}

static int level_idx(const float *arr, int n, float v)
{
    int i;
    for (i = 0; i < n; i++)
        if (fapprox(arr[i], v)) return i;
    return -1;
}

static void accumulate(Accumulator *a, float x, float y, float d)
{
    a->sum_x  += (double)x;
    a->sum_y  += (double)y;
    a->sum_x2 += (double)x * x;
    a->sum_y2 += (double)y * y;
    a->sum_d  += (double)d;
    a->sum_d2 += (double)d * d;
    a->n++;
}

static void finalize(const Accumulator *a, LutEntry *e)
{
    double n, mx, my, vx, vy, md, vd;
    float  std_d;

    if (a->n == 0) return;
    n  = (double)a->n;
    mx = a->sum_x / n;
    my = a->sum_y / n;
    vx = a->sum_x2 / n - mx * mx;
    vy = a->sum_y2 / n - my * my;
    md = a->sum_d  / n;
    vd = a->sum_d2 / n - md * md;

    e->mean_x    = (float)mx;
    e->mean_y    = (float)my;
    e->std_x     = (float)sqrt(vx < 0.0 ? 0.0 : vx);
    e->std_y     = (float)sqrt(vy < 0.0 ? 0.0 : vy);
    e->mean_dist = (float)md;
    std_d        = (float)sqrt(vd < 0.0 ? 0.0 : vd);
    e->p95_dist  = e->mean_dist + P95_Z * std_d;
}

static int parse_line(const char *line, float cols[10])
{
    int   c = 0;
    const char *p = line;
    char  *end;

    while (*p && c < 10) {
        while (*p == ' ' || *p == '\t') p++;
        if (*p == '\0' || *p == '\n' || *p == '\r') break;
        cols[c++] = (float)strtod(p, &end);
        if (end == p) return 0;
        p = end;
        if (*p == ',') p++;
    }
    return (c >= 9) ? 1 : 0;
}

/* ------------------------------------------------------------------ */
/*  Interpolation                                                       */
/* ------------------------------------------------------------------ */

static void bracket(const float *lv, int n, float v,
                    int *i0, int *i1, float *t)
{
    int i;
    if (n <= 1 || v <= lv[0])  { *i0 = *i1 = 0;     *t = 0.0f; return; }
    if (v >= lv[n - 1])        { *i0 = *i1 = n - 1; *t = 0.0f; return; }
    for (i = 0; i < n - 1; i++) {
        if (v <= lv[i + 1]) {
            *i0 = i;
            *i1 = i + 1;
            *t  = (v - lv[i]) / (lv[i + 1] - lv[i]);
            return;
        }
    }
    *i0 = *i1 = n - 1; *t = 0.0f;
}

static float lerp(float a, float b, float t) { return a + t * (b - a); }

/*
 * Trilinear interpolation across 8 LUT corners.
 * Corner order: [alt0/alt1][hsp0/hsp1][vsp0/vsp1]
 * Index encoding: bit2=alt, bit1=hsp, bit0=vsp
 *   c[0] = (a0,h0,v0)  c[1] = (a0,h0,v1)
 *   c[2] = (a0,h1,v0)  c[3] = (a0,h1,v1)
 *   c[4] = (a1,h0,v0)  c[5] = (a1,h0,v1)
 *   c[6] = (a1,h1,v0)  c[7] = (a1,h1,v1)
 */
#define TRI_FIELD(c, ta, th, tv, f) \
    lerp( \
        lerp(lerp((c)[0]->f, (c)[4]->f, ta), lerp((c)[2]->f, (c)[6]->f, ta), th), \
        lerp(lerp((c)[1]->f, (c)[5]->f, ta), lerp((c)[3]->f, (c)[7]->f, ta), th), \
        tv)

/* ------------------------------------------------------------------ */
/*  Public API                                                          */
/* ------------------------------------------------------------------ */

DroneDrop_Status drone_drop_init(const char *csv_path)
{
    FILE       *fp;
    char        line[LINE_BUF];
    float       cols[10];
    Accumulator acc;
    float       cur_alt, cur_h, cur_v;
    int         cur_combo, ai, hi, vi, i;

    s_ready    = 0;
    s_n_combos = 0;
    s_n_alt = s_n_hsp = s_n_vsp = 0;
    memset(s_idx, -1, sizeof(s_idx));

    fp = fopen(csv_path, "r");
    if (!fp) return DRONE_DROP_ERR_FILE;

    if (!fgets(line, sizeof(line), fp)) {
        fclose(fp);
        return DRONE_DROP_ERR_FORMAT;
    }

    cur_alt = FLT_MAX; cur_h = FLT_MAX; cur_v = FLT_MAX;
    cur_combo = -1;
    memset(&acc, 0, sizeof(acc));

    while (fgets(line, sizeof(line), fp)) {
        float alt, h, v, x, y, d;

        if (!parse_line(line, cols)) continue;

        alt = cols[COL_ALT];
        h   = cols[COL_HSPEED];
        v   = cols[COL_VSPEED];
        x   = cols[COL_CRASH_X];
        y   = cols[COL_CRASH_Y];
        d   = cols[COL_CRASH_D];

        if (!fapprox(alt, cur_alt) || !fapprox(h, cur_h) || !fapprox(v, cur_v)) {
            if (cur_combo >= 0)
                finalize(&acc, &s_lut[cur_combo]);

            if (s_n_combos >= DRONE_DROP_MAX_COMBOS) {
                fclose(fp);
                return DRONE_DROP_ERR_OVERFLOW;
            }

            cur_combo              = s_n_combos++;
            s_lut[cur_combo].alt     = alt;
            s_lut[cur_combo].h_speed = h;
            s_lut[cur_combo].v_speed = v;

            sorted_insert(s_alt_lv, &s_n_alt, DRONE_DROP_MAX_ALT_LEVELS,    alt);
            sorted_insert(s_hsp_lv, &s_n_hsp, DRONE_DROP_MAX_HSPEED_LEVELS, h);
            sorted_insert(s_vsp_lv, &s_n_vsp, DRONE_DROP_MAX_VSPEED_LEVELS, v);

            cur_alt = alt; cur_h = h; cur_v = v;
            memset(&acc, 0, sizeof(acc));
        }

        accumulate(&acc, x, y, d);
    }

    if (cur_combo >= 0)
        finalize(&acc, &s_lut[cur_combo]);

    fclose(fp);

    if (s_n_combos == 0) return DRONE_DROP_ERR_NO_DATA;

    for (i = 0; i < s_n_combos; i++) {
        ai = level_idx(s_alt_lv, s_n_alt, s_lut[i].alt);
        hi = level_idx(s_hsp_lv, s_n_hsp, s_lut[i].h_speed);
        vi = level_idx(s_vsp_lv, s_n_vsp, s_lut[i].v_speed);
        if (ai >= 0 && hi >= 0 && vi >= 0)
            s_idx[ai][hi][vi] = i;
    }

    s_ready = 1;
    return DRONE_DROP_OK;
}

DroneDrop_Status drone_drop_predict(float altitude_m,
                                     float h_speed_mps,
                                     float v_speed_mps,
                                     DroneDrop_Prediction *out)
{
    int   a0, a1, h0, h1, v0, v1, k, idx;
    float ta, th, tv;
    const LutEntry *c[8];
    int corners[8][3];

    if (!s_ready || !out) return DRONE_DROP_ERR_NOT_INIT;

    bracket(s_alt_lv, s_n_alt, altitude_m,  &a0, &a1, &ta);
    bracket(s_hsp_lv, s_n_hsp, h_speed_mps, &h0, &h1, &th);
    bracket(s_vsp_lv, s_n_vsp, v_speed_mps, &v0, &v1, &tv);

    corners[0][0]=a0; corners[0][1]=h0; corners[0][2]=v0;
    corners[1][0]=a0; corners[1][1]=h0; corners[1][2]=v1;
    corners[2][0]=a0; corners[2][1]=h1; corners[2][2]=v0;
    corners[3][0]=a0; corners[3][1]=h1; corners[3][2]=v1;
    corners[4][0]=a1; corners[4][1]=h0; corners[4][2]=v0;
    corners[5][0]=a1; corners[5][1]=h0; corners[5][2]=v1;
    corners[6][0]=a1; corners[6][1]=h1; corners[6][2]=v0;
    corners[7][0]=a1; corners[7][1]=h1; corners[7][2]=v1;

    for (k = 0; k < 8; k++) {
        idx  = s_idx[corners[k][0]][corners[k][1]][corners[k][2]];
        c[k] = (idx >= 0) ? &s_lut[idx] : &s_lut[0];
    }

    out->mean_x_m        = TRI_FIELD(c, ta, th, tv, mean_x);
    out->mean_y_m        = TRI_FIELD(c, ta, th, tv, mean_y);
    out->std_x_m         = TRI_FIELD(c, ta, th, tv, std_x);
    out->std_y_m         = TRI_FIELD(c, ta, th, tv, std_y);
    out->mean_distance_m = TRI_FIELD(c, ta, th, tv, mean_dist);
    out->p95_distance_m  = TRI_FIELD(c, ta, th, tv, p95_dist);

    return DRONE_DROP_OK;
}

void drone_drop_free(void)
{
    s_ready    = 0;
    s_n_combos = 0;
    s_n_alt = s_n_hsp = s_n_vsp = 0;
    memset(s_idx, -1, sizeof(s_idx));
}

int drone_drop_lut_size(void)
{
    return s_ready ? s_n_combos : 0;
}
