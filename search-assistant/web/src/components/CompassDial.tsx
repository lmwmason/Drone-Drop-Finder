import { turnInstruction } from "../lib/compass";

interface CompassDialProps {
  deviceHeadingDeg: number | null;
  targetBearingDeg: number | null;
  status: string;
  live: boolean;
  supported: boolean;
  enabled: boolean;
  onEnable: () => void;
}

const TICKS: { label: string; angle: number }[] = [
  { label: "N", angle: 0 },
  { label: "E", angle: 90 },
  { label: "S", angle: 180 },
  { label: "W", angle: 270 },
];

function tickLine(label: string, angle: number) {
  const r = 54;
  const rad = (angle * Math.PI) / 180;
  const x1 = 60 + Math.sin(rad) * (r - 8);
  const y1 = 60 - Math.cos(rad) * (r - 8);
  const x2 = 60 + Math.sin(rad) * r;
  const y2 = 60 - Math.cos(rad) * r;
  const lx = 60 + Math.sin(rad) * (r - 18);
  const ly = 60 - Math.cos(rad) * (r - 18);
  return (
    <g key={label}>
      <line x1={x1} y1={y1} x2={x2} y2={y2} stroke="var(--text-dim)" strokeWidth={1.5} />
      <text
        x={lx}
        y={ly}
        textAnchor="middle"
        dominantBaseline="middle"
        fontFamily="var(--mono)"
        fontSize={10}
        fontWeight={700}
        fill={label === "N" ? "var(--accent)" : "var(--text-faint)"}
      >
        {label}
      </text>
    </g>
  );
}

function targetArrow(targetBearingDeg: number | null) {
  if (targetBearingDeg === null) return null;
  const rad = (targetBearingDeg * Math.PI) / 180;
  const r = 40;
  const tipX = 60 + Math.sin(rad) * r;
  const tipY = 60 - Math.cos(rad) * r;
  return (
    <g>
      <line x1={60} y1={60} x2={tipX} y2={tipY} stroke="var(--accent)" strokeWidth={2.5} strokeLinecap="round" />
      <circle cx={tipX} cy={tipY} r={5} fill="var(--accent)" />
    </g>
  );
}

export function CompassDial({
  deviceHeadingDeg,
  targetBearingDeg,
  status,
  live,
  supported,
  enabled,
  onEnable,
}: CompassDialProps) {
  const heading = deviceHeadingDeg ?? 0;

  return (
    <div className="compass-row">
      <div className="compass-dial-wrap">
        <svg viewBox="0 0 120 120" width={120} height={120}>
          <circle cx={60} cy={60} r={54} fill="none" stroke="var(--line)" strokeWidth={1.5} />
          <circle cx={60} cy={60} r={2} fill="var(--text-faint)" />
          <g style={{ transition: "transform 160ms ease-out" }} transform={`rotate(${-heading}, 60, 60)`}>
            {TICKS.map((t) => tickLine(t.label, t.angle))}
            {targetArrow(targetBearingDeg)}
          </g>
        </svg>
        <div className="compass-forward" />
      </div>
      <div className="compass-info">
        <div className="compass-turn">{turnInstruction(deviceHeadingDeg, targetBearingDeg)}</div>
        <div className={`compass-status${live ? " live" : ""}`}>{status}</div>
        {!enabled && (
          <button className="reset" id="btn-compass" type="button" disabled={!supported} onClick={onEnable}>
            Enable compass
          </button>
        )}
      </div>
    </div>
  );
}
