import json
import sys

import heatmap as hm
import strategies
from compare_step1 import COMBO, SWEEP_CSV

OUT_JSON = "web/scenario.json"


def run(csv_path, altitude_m, h_speed_mps, v_speed_mps, out_path):
    points = hm.load_sweep_points(csv_path, altitude_m, h_speed_mps, v_speed_mps)
    grid = hm.build_probability_grid(points)
    bounds = strategies.full_bounds(grid)

    info_gain_waypoints = strategies.info_gain_order(grid)
    info_gain_path = strategies.expand_to_single_step_path(info_gain_waypoints)

    data = {
        "combo": {"altitude_m": altitude_m, "h_speed_mps": h_speed_mps, "v_speed_mps": v_speed_mps},
        "cell_size_m": hm.CELL_SIZE_M,
        "bounds": bounds,
        "start": [0, 0],
        "grid": {f"{x},{y}": round(p, 6) for (x, y), p in grid.items()},
        "info_gain_path": [[x, y] for x, y in info_gain_path],
        "sample_count": len(points),
    }

    with open(out_path, "w") as f:
        json.dump(data, f)

    return data


if __name__ == "__main__":
    csv_path = sys.argv[1] if len(sys.argv) > 1 else SWEEP_CSV
    data = run(csv_path, *COMBO, OUT_JSON)
    print(f"wrote {OUT_JSON}")
    print(f"cells: {len(data['grid'])}  path length: {len(data['info_gain_path'])}")
