import csv

CELL_SIZE_M = 4.0
FLOAT_EPS = 0.001


def load_crash_points(csv_path):
    points = []
    with open(csv_path, newline="") as f:
        reader = csv.DictReader(f)
        for row in reader:
            points.append((float(row["x_meters"]), float(row["y_meters"])))
    return points


def load_sweep_points(csv_path, altitude_m, h_speed_mps, v_speed_mps):
    points = []
    with open(csv_path, newline="") as f:
        reader = csv.DictReader(f)
        for row in reader:
            if (abs(float(row["altitude_m"]) - altitude_m) < FLOAT_EPS and
                    abs(float(row["h_speed_mps"]) - h_speed_mps) < FLOAT_EPS and
                    abs(float(row["v_speed_mps"]) - v_speed_mps) < FLOAT_EPS):
                points.append((float(row["crash_x_m"]), float(row["crash_y_m"])))
    return points


def sweep_combos(csv_path):
    combos = set()
    with open(csv_path, newline="") as f:
        reader = csv.DictReader(f)
        for row in reader:
            combos.add((float(row["altitude_m"]), float(row["h_speed_mps"]), float(row["v_speed_mps"])))
    return sorted(combos)


def group_sweep_points_by_combo(csv_path):
    grouped = {}
    with open(csv_path, newline="") as f:
        reader = csv.DictReader(f)
        for row in reader:
            combo = (float(row["altitude_m"]), float(row["h_speed_mps"]), float(row["v_speed_mps"]))
            grouped.setdefault(combo, []).append((float(row["crash_x_m"]), float(row["crash_y_m"])))
    return grouped


def bin_to_grid(points, cell_size_m=CELL_SIZE_M):
    grid = {}
    for x, y in points:
        cell = (int(x // cell_size_m), int(y // cell_size_m))
        grid[cell] = grid.get(cell, 0) + 1
    return grid


def normalize_grid(grid):
    total = sum(grid.values())
    return {cell: count / total for cell, count in grid.items()}


def build_probability_grid(points, cell_size_m=CELL_SIZE_M):
    counts = bin_to_grid(points, cell_size_m)
    return normalize_grid(counts)


def grid_bounds(grid):
    xs = [c[0] for c in grid]
    ys = [c[1] for c in grid]
    return min(xs), max(xs), min(ys), max(ys)
