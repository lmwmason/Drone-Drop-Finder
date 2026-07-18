import { useMemo, useState } from "react";
import type { ComboBlob } from "../types";

function key(x: number, y: number): string {
  return `${x},${y}`;
}

export interface SearchState {
  stepIndex: number;
  pathLength: number;
  visited: Set<string>;
  coveragePct: number;
  isComplete: boolean;
  current: [number, number];
  next: [number, number] | null;
  /** Bearing in the grid's own body frame (0 = grid +Y) — callers with a real heading should rotate this to true north. */
  gridFrameBearingDeg: number | null;
  distanceM: number | null;
  advance: () => void;
  reset: () => void;
}

export function useSearchState(scenario: ComboBlob): SearchState {
  const { info_gain_path: path, grid, cell_size_m: cellSizeM } = scenario;
  const [stepIndex, setStepIndex] = useState(0);
  const [visited, setVisited] = useState<Set<string>>(() => new Set([key(path[0][0], path[0][1])]));

  const totalProb = useMemo(() => Object.values(grid).reduce((a, b) => a + b, 0), [grid]);

  const coveragePct = useMemo(() => {
    if (totalProb <= 0) return 0;
    let covered = 0;
    for (const k of visited) covered += grid[k] ?? 0;
    return (covered / totalProb) * 100;
  }, [visited, grid, totalProb]);

  const current = path[stepIndex];
  const isComplete = stepIndex + 1 >= path.length;
  const next = isComplete ? null : path[stepIndex + 1];

  let gridFrameBearingDeg: number | null = null;
  let distanceM: number | null = null;
  if (next) {
    const dx = (next[0] - current[0]) * cellSizeM;
    const dy = (next[1] - current[1]) * cellSizeM;
    distanceM = Math.hypot(dx, dy);
    gridFrameBearingDeg = ((Math.atan2(dx, dy) * 180) / Math.PI + 360) % 360;
  }

  function advance() {
    setStepIndex((i) => {
      if (i + 1 >= path.length) return i;
      const newIndex = i + 1;
      setVisited((prev) => new Set(prev).add(key(path[newIndex][0], path[newIndex][1])));
      return newIndex;
    });
  }

  function reset() {
    setStepIndex(0);
    setVisited(new Set([key(path[0][0], path[0][1])]));
  }

  return {
    stepIndex,
    pathLength: path.length,
    visited,
    coveragePct,
    isComplete,
    current,
    next,
    gridFrameBearingDeg,
    distanceM,
    advance,
    reset,
  };
}
