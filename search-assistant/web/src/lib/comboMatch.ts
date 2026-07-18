import type { Combo, ManifestEntry } from "../types";

export interface ComboMatch {
  entry: ManifestEntry;
  normalizedDistance: number; // 0 = exact match, larger = further off-grid
}

// Distance above this is treated as "no reasonable match" by the caller —
// tuned so a value roughly in the middle of one axis's range alone doesn't
// trip it, but being off on all three axes does.
export const NEAREST_COMBO_WARN_THRESHOLD = 0.35;

function axisRange(values: number[]): number {
  const range = Math.max(...values) - Math.min(...values);
  return range > 0 ? range : 1; // avoid divide-by-zero if every combo shares one axis value
}

// Normalizes each axis by its span across the manifest before computing
// Euclidean distance, so e.g. a 20 m/s difference in h_speed doesn't
// dominate a 2 m/s difference in v_speed just because its raw range is bigger.
export function nearestCombo(
  manifest: ManifestEntry[],
  target: Combo,
): ComboMatch | null {
  if (manifest.length === 0) return null;

  const altRange = axisRange(manifest.map((m) => m.altitude_m));
  const hRange = axisRange(manifest.map((m) => m.h_speed_mps));
  const vRange = axisRange(manifest.map((m) => m.v_speed_mps));

  let best: ManifestEntry | null = null;
  let bestDist = Number.POSITIVE_INFINITY;

  for (const entry of manifest) {
    const dAlt = (entry.altitude_m - target.altitude_m) / altRange;
    const dH = (entry.h_speed_mps - target.h_speed_mps) / hRange;
    const dV = (entry.v_speed_mps - target.v_speed_mps) / vRange;
    const dist = Math.hypot(dAlt, dH, dV);
    if (dist < bestDist) {
      bestDist = dist;
      best = entry;
    }
  }

  if (!best) return null;
  return { entry: best, normalizedDistance: bestDist };
}
