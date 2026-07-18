import { useEffect, useRef } from "react";
import type { ComboBlob } from "../types";

const PX = 18;
const HEAT_LO: [number, number, number] = [18, 59, 58];
const HEAT_HI: [number, number, number] = [127, 224, 214];

function lerpColor(c0: [number, number, number], c1: [number, number, number], t: number): string {
  const r = Math.round(c0[0] + (c1[0] - c0[0]) * t);
  const g = Math.round(c0[1] + (c1[1] - c0[1]) * t);
  const b = Math.round(c0[2] + (c1[2] - c0[2]) * t);
  return `rgb(${r},${g},${b})`;
}

interface GridCanvasProps {
  scenario: ComboBlob;
  visited: Set<string>;
  stepIndex: number;
}

export function GridCanvas({ scenario, visited, stepIndex }: GridCanvasProps) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const { bounds, grid, info_gain_path: path } = scenario;
  const [minX, maxX, minY, maxY] = bounds;

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    const cols = maxX - minX + 1;
    const rows = maxY - minY + 1;
    const dpr = Math.min(window.devicePixelRatio || 1, 2);
    canvas.width = cols * PX * dpr;
    canvas.height = rows * PX * dpr;
    canvas.style.aspectRatio = `${cols} / ${rows}`;
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);

    const cellToPx = (x: number, y: number) => ({ px: (x - minX) * PX, py: (maxY - y) * PX });

    let maxProb = 0;
    for (const p of Object.values(grid)) if (p > maxProb) maxProb = p;

    ctx.clearRect(0, 0, cols * PX, rows * PX);

    for (let x = minX; x <= maxX; x++) {
      for (let y = minY; y <= maxY; y++) {
        const { px, py } = cellToPx(x, y);
        const p = grid[`${x},${y}`] ?? 0;
        if (p > 0) {
          const t = Math.pow(p / maxProb, 0.55);
          ctx.fillStyle = lerpColor(HEAT_LO, HEAT_HI, t);
        } else {
          ctx.fillStyle = "rgba(255,255,255,0.02)";
        }
        ctx.fillRect(px, py, PX - 1, PX - 1);
      }
    }

    ctx.fillStyle = "rgba(60, 154, 99, 0.55)";
    for (const k of visited) {
      const [x, y] = k.split(",").map(Number);
      const { px, py } = cellToPx(x, y);
      ctx.fillRect(px, py, PX - 1, PX - 1);
    }

    ctx.strokeStyle = "rgba(255,122,31,0.9)";
    ctx.lineWidth = 1.5;
    ctx.beginPath();
    for (let i = 0; i <= stepIndex; i++) {
      const { px, py } = cellToPx(path[i][0], path[i][1]);
      const cx = px + PX / 2;
      const cy = py + PX / 2;
      if (i === 0) ctx.moveTo(cx, cy);
      else ctx.lineTo(cx, cy);
    }
    ctx.stroke();

    const cur = path[stepIndex];
    const curPx = cellToPx(cur[0], cur[1]);
    ctx.fillStyle = "#ff7a1f";
    ctx.beginPath();
    ctx.arc(curPx.px + PX / 2, curPx.py + PX / 2, 5, 0, Math.PI * 2);
    ctx.fill();
    ctx.strokeStyle = "#241304";
    ctx.lineWidth = 1.5;
    ctx.stroke();

    if (stepIndex + 1 < path.length) {
      const next = path[stepIndex + 1];
      const nextPx = cellToPx(next[0], next[1]);
      ctx.strokeStyle = "#ff7a1f";
      ctx.lineWidth = 2;
      ctx.beginPath();
      ctx.arc(nextPx.px + PX / 2, nextPx.py + PX / 2, 7, 0, Math.PI * 2);
      ctx.stroke();
    }
  }, [scenario, visited, stepIndex, minX, maxX, minY, maxY, grid, path]);

  return (
    <div id="map-wrap">
      <canvas ref={canvasRef} />
    </div>
  );
}
