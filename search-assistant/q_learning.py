import random

DIRS = [(0, 1), (1, 1), (1, 0), (1, -1), (0, -1), (-1, -1), (-1, 0), (-1, 1)]

REWARD_SCALE = 100.0
STEP_COST = 1.0
NEIGHBOR_RADIUS = 2

ALPHA = 0.01
GAMMA = 0.9
EPSILON_START = 1.0
EPSILON_END = 0.05
EPSILON_DECAY_FRACTION = 0.6

N_FEATURES = 4


def in_bounds(cell, bounds):
    min_x, max_x, min_y, max_y = bounds
    return min_x <= cell[0] <= max_x and min_y <= cell[1] <= max_y


def _neighbor_sum_unvisited(grid, visited, cell, r=NEIGHBOR_RADIUS):
    total = 0.0
    for dx in range(-r, r + 1):
        for dy in range(-r, r + 1):
            if dx == 0 and dy == 0:
                continue
            n = (cell[0] + dx, cell[1] + dy)
            if n not in visited:
                total += grid.get(n, 0.0)
    return total


def _neighbor_visited_fraction(visited, cell, r=NEIGHBOR_RADIUS):
    total = visited_count = 0
    for dx in range(-r, r + 1):
        for dy in range(-r, r + 1):
            if dx == 0 and dy == 0:
                continue
            total += 1
            if (cell[0] + dx, cell[1] + dy) in visited:
                visited_count += 1
    return visited_count / total


def features(grid, visited, cell):
    own_prob = 0.0 if cell in visited else grid.get(cell, 0.0)
    neighbor_prob = _neighbor_sum_unvisited(grid, visited, cell)
    neighbor_saturation = _neighbor_visited_fraction(visited, cell)
    return (own_prob, neighbor_prob, neighbor_saturation, 1.0)


def qval(weights, phi):
    return sum(w * p for w, p in zip(weights, phi))


def best_action(weights, grid, visited, pos, valid_actions, prefer_unvisited=True):
    cand_features = {
        a: features(grid, visited, (pos[0] + DIRS[a][0], pos[1] + DIRS[a][1]))
        for a in valid_actions
    }
    pool = valid_actions
    if prefer_unvisited:
        unvisited = [
            a for a in valid_actions
            if (pos[0] + DIRS[a][0], pos[1] + DIRS[a][1]) not in visited
        ]
        if unvisited:
            pool = unvisited
    best = max(pool, key=lambda a: qval(weights, cand_features[a]))
    return best, cand_features


def _valid_actions(pos, bounds):
    return [a for a in range(8) if in_bounds((pos[0] + DIRS[a][0], pos[1] + DIRS[a][1]), bounds)]


def train(grid, bounds, start=(0, 0), episodes=500, max_steps=200, seed=None):
    rng = random.Random(seed)
    weights = [0.0] * N_FEATURES
    episode_rewards = []

    for ep in range(episodes):
        epsilon = max(
            EPSILON_END,
            EPSILON_START - (EPSILON_START - EPSILON_END) * ep / (episodes * EPSILON_DECAY_FRACTION),
        )
        pos = start
        visited = {pos}
        total_reward = 0.0

        for _ in range(max_steps):
            valid = _valid_actions(pos, bounds)
            if rng.random() < epsilon:
                a = rng.choice(valid)
                cand_features = {
                    i: features(grid, visited, (pos[0] + DIRS[i][0], pos[1] + DIRS[i][1]))
                    for i in valid
                }
            else:
                a, cand_features = best_action(weights, grid, visited, pos, valid)

            new_pos = (pos[0] + DIRS[a][0], pos[1] + DIRS[a][1])
            phi = cand_features[a]
            reward = phi[0] * REWARD_SCALE - STEP_COST
            visited.add(new_pos)

            new_valid = _valid_actions(new_pos, bounds)
            if new_valid:
                _, new_cand_features = best_action(weights, grid, visited, new_pos, new_valid)
                best_next_q = max(qval(weights, f) for f in new_cand_features.values())
            else:
                best_next_q = 0.0

            td_error = (reward + GAMMA * best_next_q) - qval(weights, phi)
            weights = [w + ALPHA * td_error * p for w, p in zip(weights, phi)]

            pos = new_pos
            total_reward += reward

        episode_rewards.append(total_reward)

    return weights, episode_rewards


def rollout(weights, grid, bounds, start=(0, 0), max_steps=200):
    pos = start
    visited = {pos}
    order = [pos]

    for _ in range(max_steps):
        valid = _valid_actions(pos, bounds)
        if not valid:
            break
        a, _ = best_action(weights, grid, visited, pos, valid)
        pos = (pos[0] + DIRS[a][0], pos[1] + DIRS[a][1])
        visited.add(pos)
        order.append(pos)

    return order
