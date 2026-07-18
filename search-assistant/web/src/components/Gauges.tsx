interface GaugesProps {
  bearingDeg: number | null;
  distanceM: number | null;
  coveragePct: number;
  stepIndex: number;
  stepTotal: number;
}

export function Gauges({ bearingDeg, distanceM, coveragePct, stepIndex, stepTotal }: GaugesProps) {
  return (
    <div className="instruments">
      <div className="gauge">
        <div className="k">Bearing</div>
        <div className="v">
          {bearingDeg === null ? "—" : <>{bearingDeg.toFixed(0)}<span className="unit">°</span></>}
        </div>
      </div>
      <div className="gauge">
        <div className="k">Distance</div>
        <div className="v">
          {distanceM === null ? "—" : <>{Math.round(distanceM)}<span className="unit">m</span></>}
        </div>
      </div>
      <div className="gauge">
        <div className="k">Coverage</div>
        <div className="v">
          {coveragePct.toFixed(0)}<span className="unit">%</span>
        </div>
      </div>
      <div className="gauge">
        <div className="k">Step</div>
        <div className="v">
          {stepIndex} <span className="unit">/ {stepTotal}</span>
        </div>
      </div>
    </div>
  );
}
