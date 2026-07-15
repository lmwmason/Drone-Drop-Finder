import sys

import heatmap as hm
import metrics
import strategies

SWEEP_CSV = "data/sweep_20260715_152938.csv"
COMBO = (113.33, 11.429, -5.0)
CHECKPOINT_FRACTIONS = (0.1, 0.25, 0.5, 0.75, 1.0)


def run(csv_path, altitude_m, h_speed_mps, v_speed_mps):
    points = hm.load_sweep_points(csv_path, altitude_m, h_speed_mps, v_speed_mps)
    grid = hm.build_probability_grid(points)
    bounds = strategies.full_bounds(grid)

    orders = {
        "naive": strategies.naive_order(bounds),
        "greedy": strategies.expand_to_single_step_path(strategies.greedy_order(grid)),
        "info_gain": strategies.expand_to_single_step_path(strategies.info_gain_order(grid)),
    }

    print(f"combo: altitude={altitude_m} h_speed={h_speed_mps} v_speed={v_speed_mps}")
    print(f"trials={len(points)} cells={len(grid)} bounds={bounds}")
    print()

    for name, order in orders.items():
        steps, coverage, distance = metrics.coverage_curve(order, grid)
        mean_disc_steps, _ = metrics.expected_discovery_steps(order, points)

        print(f"--- {name} ---")
        print(f"  expected discovery steps: {mean_disc_steps:.2f}")
        for frac in CHECKPOINT_FRACTIONS:
            target_step = max(1, round(frac * len(steps)))
            cov, dist = metrics.coverage_at(steps, coverage, distance, target_step)
            print(f"  step {target_step:4d} ({frac*100:5.1f}%): "
                  f"coverage={cov*100:5.1f}%  distance={dist:7.1f}m")
        print()


if __name__ == "__main__":
    csv_path = sys.argv[1] if len(sys.argv) > 1 else SWEEP_CSV
    run(csv_path, *COMBO)
