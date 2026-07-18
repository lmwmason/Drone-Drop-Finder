import json
import os
import sys

import heatmap as hm
import strategies
from compare_step1 import SWEEP_CSV

OUT_DIR = "dist"
COMBOS_SUBDIR = "combos"


def build_scenario(altitude_m, h_speed_mps, v_speed_mps, points):
    grid = hm.build_probability_grid(points)
    bounds = strategies.full_bounds(grid)

    info_gain_waypoints = strategies.info_gain_order(grid)
    info_gain_path = strategies.expand_to_single_step_path(info_gain_waypoints)

    return {
        "combo": {"altitude_m": altitude_m, "h_speed_mps": h_speed_mps, "v_speed_mps": v_speed_mps},
        "cell_size_m": hm.CELL_SIZE_M,
        "bounds": bounds,
        "start": [0, 0],
        "grid": {f"{x},{y}": round(p, 6) for (x, y), p in grid.items()},
        "info_gain_path": [[x, y] for x, y in info_gain_path],
        "sample_count": len(points),
    }


def run(csv_path, out_dir):
    combos_dir = os.path.join(out_dir, COMBOS_SUBDIR)
    os.makedirs(combos_dir, exist_ok=True)

    grouped = hm.group_sweep_points_by_combo(csv_path)

    manifest = []
    for index, (combo, points) in enumerate(sorted(grouped.items())):
        altitude_m, h_speed_mps, v_speed_mps = combo
        scenario = build_scenario(altitude_m, h_speed_mps, v_speed_mps, points)

        # Index-based filenames avoid float-formatting mismatches between
        # Python (writer) and the JS upload/fetch side (reader) — the
        # manifest carries the real combo values, the filename is just a key.
        filename = f"{index:03d}.json"
        with open(os.path.join(combos_dir, filename), "w") as f:
            json.dump(scenario, f)

        manifest.append({
            "altitude_m": altitude_m,
            "h_speed_mps": h_speed_mps,
            "v_speed_mps": v_speed_mps,
            "sample_count": len(points),
            "cell_count": len(scenario["grid"]),
            "file": f"{COMBOS_SUBDIR}/{filename}",
        })

    with open(os.path.join(out_dir, "manifest.json"), "w") as f:
        json.dump(manifest, f)

    return manifest


if __name__ == "__main__":
    csv_path = sys.argv[1] if len(sys.argv) > 1 else SWEEP_CSV
    out_dir = sys.argv[2] if len(sys.argv) > 2 else OUT_DIR
    manifest = run(csv_path, out_dir)
    cell_counts = [m["cell_count"] for m in manifest]
    print(f"wrote {len(manifest)} combo files to {out_dir}/{COMBOS_SUBDIR}/")
    print(f"cell counts: min={min(cell_counts)} max={max(cell_counts)} avg={sum(cell_counts)/len(cell_counts):.1f}")
    print(f"manifest: {out_dir}/manifest.json")
