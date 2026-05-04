#include "drone_drop.h"
#include <stdio.h>

int main(void)
{
    DroneDrop_Status      status;
    DroneDrop_Prediction  pred;

    status = drone_drop_init("sweep_data.csv");
    if (status != DRONE_DROP_OK) {
        printf("Init failed: %d\n", status);
        return 1;
    }

    printf("LUT loaded: %d combos\n", drone_drop_lut_size());

    float altitude_m   = 80.0f;
    float h_speed_mps  = 12.0f;
    float v_speed_mps  = -2.0f;

    status = drone_drop_predict(altitude_m, h_speed_mps, v_speed_mps, &pred);
    if (status != DRONE_DROP_OK) {
        printf("Predict failed: %d\n", status);
        return 1;
    }

    printf("--- Crash prediction ---\n");
    printf("  Input : alt=%.1f m  h_spd=%.1f m/s  v_spd=%.1f m/s\n",
           altitude_m, h_speed_mps, v_speed_mps);
    printf("  Mean offset (drone frame): fwd=%.2f m  lat=%.2f m\n",
           pred.mean_x_m, pred.mean_y_m);
    printf("  Std dev : fwd=%.2f m  lat=%.2f m\n",
           pred.std_x_m, pred.std_y_m);
    printf("  Mean distance : %.2f m\n", pred.mean_distance_m);
    printf("  P95  radius   : %.2f m  (safety exclusion zone)\n",
           pred.p95_distance_m);

    drone_drop_free();
    return 0;
}
