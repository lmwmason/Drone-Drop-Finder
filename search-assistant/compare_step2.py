import csv
import sys

import heatmap as hm
import metrics
import q_learning as ql
import strategies
from compare_step1 import COMBO, SWEEP_CSV

EPISODES = 500
MAX_STEPS = 200
SEED = 0
REWARDS_CSV = "results/q_learning_rewards.csv"


def run(csv_path, altitude_m, h_speed_mps, v_speed_mps, episodes, max_steps, seed):
    points = hm.load_sweep_points(csv_path, altitude_m, h_speed_mps, v_speed_mps)
    grid = hm.build_probability_grid(points)
    bounds = strategies.full_bounds(grid)

    weights, episode_rewards = ql.train(grid, bounds, episodes=episodes, max_steps=max_steps, seed=seed)
    q_order = ql.rollout(weights, grid, bounds, max_steps=max_steps)

    orders = {
        "naive": strategies.naive_order(bounds),
        "greedy": strategies.expand_to_single_step_path(strategies.greedy_order(grid)),
        "info_gain": strategies.expand_to_single_step_path(strategies.info_gain_order(grid)),
        "q_learning": q_order,
    }

    print(f"combo: altitude={altitude_m} h_speed={h_speed_mps} v_speed={v_speed_mps}")
    print(f"trials={len(points)} cells={len(grid)} bounds={bounds}")
    print(f"q_learning weights (own_prob, neighbor_prob, neighbor_saturation, bias): "
          f"{[round(w, 3) for w in weights]}")
    print(f"reward first-10 avg: {sum(episode_rewards[:10])/10:.1f}  "
          f"last-10 avg: {sum(episode_rewards[-10:])/10:.1f}")
    print()

    print(f"=== full run (each strategy runs to its own completion) ===")
    for name, order in orders.items():
        steps, coverage, distance = metrics.coverage_curve(order, grid)
        mean_disc_steps, _ = metrics.expected_discovery_steps(order, points)
        print(f"--- {name} ---")
        print(f"  steps to run: {len(order)}")
        print(f"  coverage reached: {coverage[-1]*100:.1f}%")
        print(f"  distance traveled: {distance[-1]:.1f}m")
        print(f"  expected discovery steps: {mean_disc_steps:.2f}")
    print()

    print(f"=== matched budget ({max_steps} single-cell steps, all strategies) ===")
    for name, order in orders.items():
        cap = min(max_steps, len(order))
        steps, coverage, distance = metrics.coverage_curve(order[:cap], grid)
        mean_disc_steps, _ = metrics.expected_discovery_steps(order, points, budget=max_steps)
        print(f"--- {name} ---")
        print(f"  coverage @ budget: {coverage[-1]*100:.1f}%")
        print(f"  distance @ budget: {distance[-1]:.1f}m")
        print(f"  expected discovery steps (capped): {mean_disc_steps:.2f}")
    print()

    return episode_rewards


def write_rewards_csv(episode_rewards, out_path):
    with open(out_path, "w", newline="") as f:
        writer = csv.writer(f)
        writer.writerow(["episode", "total_reward"])
        for i, r in enumerate(episode_rewards):
            writer.writerow([i, r])


if __name__ == "__main__":
    csv_path = sys.argv[1] if len(sys.argv) > 1 else SWEEP_CSV
    rewards = run(csv_path, *COMBO, EPISODES, MAX_STEPS, SEED)
    write_rewards_csv(rewards, REWARDS_CSV)
    print(f"wrote {REWARDS_CSV}")
