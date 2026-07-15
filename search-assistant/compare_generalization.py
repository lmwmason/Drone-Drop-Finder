import csv
import random
import sys

import heatmap as hm
import metrics
import strategies

SWEEP_CSV = "data/sweep_20260715_152938.csv"
SAMPLE_SIZE = 40
SEED = 42
MIN_CELLS = 10
OUT_CSV = "results/generalization.csv"


def run(csv_path, sample_size, seed):
    grouped = hm.group_sweep_points_by_combo(csv_path)
    combos = [c for c in grouped if len(grouped[c]) > 0]

    rng = random.Random(seed)
    candidates = [c for c in combos if len(hm.build_probability_grid(grouped[c])) >= MIN_CELLS]
    sample = rng.sample(candidates, min(sample_size, len(candidates)))

    rows = []
    for combo in sample:
        points = grouped[combo]
        grid = hm.build_probability_grid(points)
        bounds = strategies.full_bounds(grid)

        orders = {
            "naive": strategies.naive_order(bounds),
            "greedy": strategies.expand_to_single_step_path(strategies.greedy_order(grid)),
            "info_gain": strategies.expand_to_single_step_path(strategies.info_gain_order(grid)),
        }

        row = {"altitude_m": combo[0], "h_speed_mps": combo[1], "v_speed_mps": combo[2], "cells": len(grid)}
        for name, order in orders.items():
            steps, coverage, distance = metrics.coverage_curve(order, grid)
            mean_disc_steps, _ = metrics.expected_discovery_steps(order, points)
            row[f"{name}_full_distance_m"] = distance[-1]
            row[f"{name}_discovery_steps"] = mean_disc_steps
        rows.append(row)

    return rows


def summarize(rows):
    def avg_ratio(key, baseline):
        ratios = [r[f"info_gain_{key}"] / r[f"{baseline}_{key}"] for r in rows if r[f"{baseline}_{key}"] > 0]
        return sum(ratios) / len(ratios)

    print(f"combos sampled: {len(rows)}")
    print(f"info_gain distance vs naive:  {avg_ratio('full_distance_m', 'naive'):.2f}x")
    print(f"info_gain distance vs greedy: {avg_ratio('full_distance_m', 'greedy'):.2f}x")
    print(f"info_gain discovery_steps vs naive:  {avg_ratio('discovery_steps', 'naive'):.2f}x")
    print(f"info_gain discovery_steps vs greedy: {avg_ratio('discovery_steps', 'greedy'):.2f}x")

    worse_distance = [r for r in rows if r["info_gain_full_distance_m"] > r["naive_full_distance_m"]]
    print(f"combos where info_gain traveled farther than naive: {len(worse_distance)}/{len(rows)}")


def write_csv(rows, out_path):
    fieldnames = list(rows[0].keys())
    with open(out_path, "w", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)


if __name__ == "__main__":
    csv_path = sys.argv[1] if len(sys.argv) > 1 else SWEEP_CSV
    rows = run(csv_path, SAMPLE_SIZE, SEED)
    summarize(rows)
    write_csv(rows, OUT_CSV)
    print(f"\nwrote {OUT_CSV}")
