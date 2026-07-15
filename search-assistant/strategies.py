import math

import heatmap as hm


def full_bounds(grid, margin=2):
    min_x, max_x, min_y, max_y = hm.grid_bounds(grid)
    return min_x - margin, max_x + margin, min_y - margin, max_y + margin


def spiral_cells():
    yield (0, 0)
    x, y = 0, 0
    dx, dy = 1, 0
    leg = 1
    while True:
        for _ in range(2):
            for _ in range(leg):
                x += dx
                y += dy
                yield (x, y)
            dx, dy = -dy, dx
        leg += 1


def naive_order(bounds):
    min_x, max_x, min_y, max_y = bounds
    total_cells = (max_x - min_x + 1) * (max_y - min_y + 1)
    order = []
    for cell in spiral_cells():
        x, y = cell
        if min_x <= x <= max_x and min_y <= y <= max_y:
            order.append(cell)
            if len(order) >= total_cells:
                break
    return order


def greedy_order(grid, start=(0, 0)):
    return sorted(
        grid.keys(),
        key=lambda c: (-grid[c], math.hypot(c[0] - start[0], c[1] - start[1])),
    )


def _entropy(probs):
    return -sum(p * math.log(p) for p in probs if p > 0)


def info_gain_order(grid, start=(0, 0)):
    remaining = dict(grid)
    order = []
    current = start
    while remaining:
        total = sum(remaining.values())
        h_before = _entropy(p / total for p in remaining.values())

        best_cell, best_score = None, -1.0
        for c, p in remaining.items():
            rest_total = total - p
            if rest_total > 1e-12:
                rest_probs = [pp / rest_total for cc, pp in remaining.items() if cc != c]
                h_after = _entropy(rest_probs)
            else:
                h_after = 0.0
            expected_h_after = (1.0 - p / total) * h_after
            gain = h_before - expected_h_after

            dist = math.hypot(c[0] - current[0], c[1] - current[1]) * hm.CELL_SIZE_M
            dist = max(dist, hm.CELL_SIZE_M)
            score = gain / dist

            if score > best_score:
                best_score, best_cell = score, c

        order.append(best_cell)
        current = best_cell
        del remaining[best_cell]
    return order


def walk_cells(a, b):
    x0, y0 = a
    x1, y1 = b
    dx, dy = x1 - x0, y1 - y0
    steps = max(abs(dx), abs(dy))
    sx = (dx > 0) - (dx < 0)
    sy = (dy > 0) - (dy < 0)
    x, y = x0, y0
    path = []
    for _ in range(steps):
        if x != x1:
            x += sx
        if y != y1:
            y += sy
        path.append((x, y))
    return path


def expand_to_single_step_path(waypoints):
    if not waypoints:
        return []
    path = [waypoints[0]]
    for a, b in zip(waypoints, waypoints[1:]):
        path.extend(walk_cells(a, b))
    return path
