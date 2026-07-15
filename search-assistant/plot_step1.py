import csv

import matplotlib.pyplot as plt

import heatmap as hm
import metrics
import strategies
from compare_step1 import COMBO, SWEEP_CSV

COLOR_NAIVE = "#2a78d6"
COLOR_GREEDY = "#008300"
COLOR_INFO_GAIN = "#e87ba4"
COLOR_GRID = "#dddddd"
COLOR_TEXT = "#52514e"

GENERALIZATION_CSV = "results/generalization.csv"
OUT_COVERAGE = "results/step1_coverage_vs_distance.png"
OUT_SUMMARY = "results/step1_distance_summary.png"
OUT_DISCOVERY = "results/step1_discovery_steps_summary.png"


def plot_coverage_curve():
    points = hm.load_sweep_points(SWEEP_CSV, *COMBO)
    grid = hm.build_probability_grid(points)
    bounds = strategies.full_bounds(grid)

    series = [
        ("naive", strategies.naive_order(bounds), COLOR_NAIVE),
        ("greedy", strategies.expand_to_single_step_path(strategies.greedy_order(grid)), COLOR_GREEDY),
        ("info_gain", strategies.expand_to_single_step_path(strategies.info_gain_order(grid)), COLOR_INFO_GAIN),
    ]

    fig, ax = plt.subplots(figsize=(7, 4.5))
    for name, order, color in series:
        _, coverage, distance = metrics.coverage_curve(order, grid)
        ax.plot(distance, [c * 100 for c in coverage], color=color, linewidth=2, label=name)

    final_x = max(metrics.coverage_curve(order, grid)[2][-1] for _, order, _ in series)
    for name, order, color in series:
        _, coverage, distance = metrics.coverage_curve(order, grid)
        ax.annotate(f"{name}: {distance[-1]:.0f} m", xy=(distance[-1], coverage[-1] * 100),
                    xytext=(4, 0), textcoords="offset points", color=COLOR_TEXT, fontsize=9)

    ax.set_xlabel("distance traveled (m)")
    ax.set_ylabel("probability mass covered (%)")
    ax.set_title(f"coverage vs. distance — alt={COMBO[0]}m h_spd={COMBO[1]}m/s v_spd={COMBO[2]}m/s")
    ax.set_xlim(0, final_x * 1.15)
    ax.grid(color=COLOR_GRID, linewidth=0.8)
    ax.spines[["top", "right"]].set_visible(False)
    ax.legend(frameon=False)
    fig.tight_layout()
    fig.savefig(OUT_COVERAGE, dpi=150)
    plt.close(fig)


def plot_generalization_summary():
    rows = list(csv.DictReader(open(GENERALIZATION_CSV)))
    strategies_ = ["naive", "greedy", "info_gain"]
    colors = {"naive": COLOR_NAIVE, "greedy": COLOR_GREEDY, "info_gain": COLOR_INFO_GAIN}

    means, mins, maxs = [], [], []
    for s in strategies_:
        vals = [float(r[f"{s}_full_distance_m"]) for r in rows]
        means.append(sum(vals) / len(vals))
        mins.append(min(vals))
        maxs.append(max(vals))

    fig, ax = plt.subplots(figsize=(6, 4.5))
    x = range(len(strategies_))
    bars = ax.bar(x, means, color=[colors[s] for s in strategies_], width=0.55)
    ax.errorbar(x, means,
                yerr=[[m - lo for m, lo in zip(means, mins)], [hi - m for m, hi in zip(means, maxs)]],
                fmt="none", ecolor=COLOR_TEXT, capsize=4, linewidth=1)

    for xi, m in zip(x, means):
        ax.annotate(f"{m:.0f} m", xy=(xi, m), xytext=(0, 6), textcoords="offset points",
                    ha="center", color=COLOR_TEXT, fontsize=9)

    ax.set_xticks(list(x))
    ax.set_xticklabels(strategies_)
    ax.set_ylabel("distance to 100% coverage (m)")
    ax.set_title(f"distance to full coverage across {len(rows)} flight-state combos\n(bars: mean, whiskers: min–max)")
    ax.grid(color=COLOR_GRID, linewidth=0.8, axis="y")
    ax.spines[["top", "right"]].set_visible(False)
    fig.tight_layout()
    fig.savefig(OUT_SUMMARY, dpi=150)
    plt.close(fig)


def plot_discovery_steps_summary():
    rows = list(csv.DictReader(open(GENERALIZATION_CSV)))
    strategies_ = ["naive", "greedy", "info_gain"]
    colors = {"naive": COLOR_NAIVE, "greedy": COLOR_GREEDY, "info_gain": COLOR_INFO_GAIN}

    means = []
    for s in strategies_:
        vals = [float(r[f"{s}_discovery_steps"]) for r in rows]
        means.append(sum(vals) / len(vals))

    fig, ax = plt.subplots(figsize=(6, 4.5))
    x = range(len(strategies_))
    ax.bar(x, means, color=[colors[s] for s in strategies_], width=0.55)
    for xi, m in zip(x, means):
        ax.annotate(f"{m:.0f}", xy=(xi, m), xytext=(0, 6), textcoords="offset points",
                    ha="center", color=COLOR_TEXT, fontsize=9)

    ax.set_xticks(list(x))
    ax.set_xticklabels(strategies_)
    ax.set_ylabel("expected discovery steps")
    ax.set_title(f"expected steps to find target across {len(rows)} flight-state combos\n"
                 f"(single-cell-step movement for all strategies)", fontsize=12)
    ax.grid(color=COLOR_GRID, linewidth=0.8, axis="y")
    ax.spines[["top", "right"]].set_visible(False)
    fig.tight_layout()
    fig.savefig(OUT_DISCOVERY, dpi=150)
    plt.close(fig)


if __name__ == "__main__":
    plot_coverage_curve()
    plot_generalization_summary()
    plot_discovery_steps_summary()
    print(f"wrote {OUT_COVERAGE}")
    print(f"wrote {OUT_SUMMARY}")
    print(f"wrote {OUT_DISCOVERY}")
