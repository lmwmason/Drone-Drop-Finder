import csv

import matplotlib.pyplot as plt

import heatmap as hm
import metrics
import q_learning as ql
import strategies
from compare_step1 import COMBO, SWEEP_CSV
from compare_step2 import EPISODES, MAX_STEPS, SEED

COLOR_NAIVE = "#2a78d6"
COLOR_GREEDY = "#008300"
COLOR_INFO_GAIN = "#e87ba4"
COLOR_Q_LEARNING = "#eda100"
COLOR_GRID = "#dddddd"
COLOR_TEXT = "#52514e"

REWARDS_CSV = "results/q_learning_rewards.csv"
OUT_CONVERGENCE = "results/step2_reward_convergence.png"
OUT_BUDGET_COMPARE = "results/step2_matched_budget_coverage.png"


def plot_convergence():
    rows = list(csv.DictReader(open(REWARDS_CSV)))
    episodes = [int(r["episode"]) for r in rows]
    rewards = [float(r["total_reward"]) for r in rows]

    window = 20
    smoothed = [
        sum(rewards[max(0, i - window):i + 1]) / len(rewards[max(0, i - window):i + 1])
        for i in range(len(rewards))
    ]

    fig, ax = plt.subplots(figsize=(7, 4.5))
    ax.plot(episodes, rewards, color=COLOR_Q_LEARNING, linewidth=1, alpha=0.35, label="episode reward")
    ax.plot(episodes, smoothed, color=COLOR_Q_LEARNING, linewidth=2, label="20-episode moving avg")

    ax.set_xlabel("episode")
    ax.set_ylabel("total reward")
    ax.set_title(f"Q-learning convergence — alt={COMBO[0]}m h_spd={COMBO[1]}m/s v_spd={COMBO[2]}m/s")
    ax.grid(color=COLOR_GRID, linewidth=0.8)
    ax.spines[["top", "right"]].set_visible(False)
    ax.legend(frameon=False)
    fig.tight_layout()
    fig.savefig(OUT_CONVERGENCE, dpi=150)
    plt.close(fig)


def plot_matched_budget_coverage():
    points = hm.load_sweep_points(SWEEP_CSV, *COMBO)
    grid = hm.build_probability_grid(points)
    bounds = strategies.full_bounds(grid)

    weights, _ = ql.train(grid, bounds, episodes=EPISODES, max_steps=MAX_STEPS, seed=SEED)
    q_order = ql.rollout(weights, grid, bounds, max_steps=MAX_STEPS)

    orders = {
        "naive": (strategies.naive_order(bounds), COLOR_NAIVE),
        "greedy": (strategies.expand_to_single_step_path(strategies.greedy_order(grid)), COLOR_GREEDY),
        "info_gain": (strategies.expand_to_single_step_path(strategies.info_gain_order(grid)), COLOR_INFO_GAIN),
        "q_learning": (q_order, COLOR_Q_LEARNING),
    }

    names, coverages, colors = [], [], []
    for name, (order, color) in orders.items():
        cap = min(MAX_STEPS, len(order))
        _, coverage, _ = metrics.coverage_curve(order[:cap], grid)
        names.append(name)
        coverages.append(coverage[-1] * 100)
        colors.append(color)

    fig, ax = plt.subplots(figsize=(6, 4.5))
    x = range(len(names))
    ax.bar(x, coverages, color=colors, width=0.55)
    for xi, c in zip(x, coverages):
        ax.annotate(f"{c:.0f}%", xy=(xi, c), xytext=(0, 6), textcoords="offset points",
                    ha="center", color=COLOR_TEXT, fontsize=9)

    ax.set_xticks(list(x))
    ax.set_xticklabels(names)
    ax.set_ylabel("probability mass covered (%)")
    ax.set_ylim(0, 110)
    ax.set_title(f"coverage at matched {MAX_STEPS}-step budget\n(single-cell-step movement, all strategies)",
                 fontsize=13)
    ax.grid(color=COLOR_GRID, linewidth=0.8, axis="y")
    ax.spines[["top", "right"]].set_visible(False)
    fig.tight_layout()
    fig.savefig(OUT_BUDGET_COMPARE, dpi=150)
    plt.close(fig)


if __name__ == "__main__":
    plot_convergence()
    plot_matched_budget_coverage()
    print(f"wrote {OUT_CONVERGENCE}")
    print(f"wrote {OUT_BUDGET_COMPARE}")
