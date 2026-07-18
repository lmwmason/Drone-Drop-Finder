import { compassWord } from "../lib/compass";
import { useDeviceOrientation } from "../hooks/useDeviceOrientation";
import type { SearchState } from "../hooks/useSearchState";
import { CompassDial } from "./CompassDial";
import { Gauges } from "./Gauges";

interface GuidancePanelProps {
  search: SearchState;
  /** True-north heading offset to apply to the grid-frame bearing (null = unknown, grid-frame bearing shown as-is with a warning). */
  headingDeg: number | null;
}

export function GuidancePanel({ search, headingDeg }: GuidancePanelProps) {
  const orientation = useDeviceOrientation();
  const { stepIndex, pathLength, coveragePct, isComplete, next, gridFrameBearingDeg, distanceM, advance, reset } =
    search;

  const trueBearingDeg =
    gridFrameBearingDeg === null ? null : ((gridFrameBearingDeg + (headingDeg ?? 0)) % 360 + 360) % 360;

  return (
    <section className="panel guidance">
      <div className="k">Next action</div>

      {headingDeg === null && (
        <div className="heading-warning">Heading unknown — bearings are grid-relative, not true-north.</div>
      )}

      <CompassDial
        deviceHeadingDeg={orientation.headingDeg}
        targetBearingDeg={isComplete ? null : trueBearingDeg}
        status={orientation.status}
        live={orientation.live}
        supported={orientation.supported}
        enabled={orientation.enabled}
        onEnable={orientation.requestEnable}
      />

      {isComplete ? (
        <>
          <div className="guidance-text">Search complete — full area covered</div>
          <div className="guidance-sub">{stepIndex} steps total</div>
        </>
      ) : (
        <>
          <div className="guidance-text">
            Head <span className="dir">{compassWord(trueBearingDeg ?? 0)}</span> for {Math.round(distanceM ?? 0)}m
          </div>
          <div className="guidance-sub">
            bearing {(trueBearingDeg ?? 0).toFixed(0)}°  ·  grid ({next?.[0]}, {next?.[1]})
          </div>
        </>
      )}

      <Gauges
        bearingDeg={isComplete ? null : trueBearingDeg}
        distanceM={isComplete ? null : distanceM}
        coveragePct={coveragePct}
        stepIndex={stepIndex}
        stepTotal={pathLength - 1}
      />

      <div className="progress-row">
        <div className="progress-track">
          <div className="progress-fill" style={{ width: `${coveragePct.toFixed(1)}%` }} />
        </div>
        <div className="progress-label">
          <span>{coveragePct.toFixed(0)}% covered</span>
          <span className="status-chip">
            <span className="dot" />
            {isComplete ? "done" : "searching"}
          </span>
        </div>
      </div>

      <button className="advance" disabled={isComplete} onClick={advance}>
        Advance to next point
      </button>
      <button className="reset" onClick={reset}>
        Restart
      </button>
    </section>
  );
}
