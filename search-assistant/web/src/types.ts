export interface Combo {
  altitude_m: number;
  h_speed_mps: number;
  v_speed_mps: number;
}

// One precomputed scenario — matches export_all_scenarios.py's build_scenario() output.
export interface ComboBlob {
  combo: Combo;
  cell_size_m: number;
  bounds: [number, number, number, number]; // minX, maxX, minY, maxY
  start: [number, number];
  grid: Record<string, number>; // "x,y" -> probability
  info_gain_path: [number, number][];
  sample_count: number;
}

// One row of combos-manifest.json — matches export_all_scenarios.py's manifest entries,
// plus a `url` the JSON blob can be fetched from (local /public path in dev, a Vercel
// Blob public URL once uploaded — see scripts/upload-combos.mjs).
export interface ManifestEntry extends Combo {
  sample_count: number;
  cell_count: number;
  url: string;
}

export interface UrlParams {
  lat: number | null;
  lon: number | null;
  altitudeM: number | null;
  hSpeedMps: number | null;
  vSpeedMps: number | null;
  headingDeg: number | null;
}

export interface GeoPoint {
  lat: number;
  lon: number;
}
