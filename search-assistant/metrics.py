import math

import heatmap as hm


def coverage_curve(order, grid):
    steps, coverage, distance = [], [], []
    cum, dist_accum, prev = 0.0, 0.0, None
    seen = set()
    for i, cell in enumerate(order, start=1):
        if cell not in seen:
            cum += grid.get(cell, 0.0)
            seen.add(cell)
        if prev is not None:
            dist_accum += math.hypot(cell[0] - prev[0], cell[1] - prev[1]) * hm.CELL_SIZE_M
        prev = cell
        steps.append(i)
        coverage.append(cum)
        distance.append(dist_accum)
    return steps, coverage, distance


def coverage_at(steps, coverage, distance, target_step):
    idx = min(target_step, len(steps)) - 1
    return coverage[idx], distance[idx]


def expected_discovery_steps(order, points, cell_size=hm.CELL_SIZE_M, budget=None):
    if budget is None:
        budget = len(order)
    step_index = {}
    for i, cell in enumerate(order, start=1):
        if i > budget:
            break
        if cell not in step_index:
            step_index[cell] = i
    discovery_steps = []
    for x, y in points:
        cell = (int(x // cell_size), int(y // cell_size))
        discovery_steps.append(step_index.get(cell, budget))
    mean_steps = sum(discovery_steps) / len(discovery_steps)
    return mean_steps, discovery_steps
