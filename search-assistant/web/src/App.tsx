import { useMemo, useState } from "react";
import manifestData from "./data/combos-manifest.json";
import { Header } from "./components/Header";
import { GridCanvas } from "./components/GridCanvas";
import { GuidancePanel } from "./components/GuidancePanel";
import { SearchMap } from "./components/SearchMap";
import { SetupForm } from "./components/SetupForm";
import { useComboData } from "./hooks/useComboData";
import { useGeolocation } from "./hooks/useGeolocation";
import { useSearchState } from "./hooks/useSearchState";
import { nearestCombo, NEAREST_COMBO_WARN_THRESHOLD } from "./lib/comboMatch";
import { hasCombo, parseUrlParams } from "./lib/urlParams";
import type { ComboBlob, ManifestEntry, UrlParams } from "./types";

const manifest = manifestData as ManifestEntry[];

function SearchScreen({ params }: { params: UrlParams }) {
  const match = useMemo(() => {
    if (params.altitudeM === null || params.hSpeedMps === null || params.vSpeedMps === null) return null;
    return nearestCombo(manifest, {
      altitude_m: params.altitudeM,
      h_speed_mps: params.hSpeedMps,
      v_speed_mps: params.vSpeedMps,
    });
  }, [params.altitudeM, params.hSpeedMps, params.vSpeedMps]);

  const { data, loading, error } = useComboData(match?.entry.url ?? null);

  if (!match) return <div className="panel guidance">No combo values provided.</div>;
  if (loading || !data) return <div className="panel guidance">Loading search grid…</div>;
  if (error) return <div className="panel guidance">Failed to load search grid: {error}</div>;

  return (
    <>
      <Header combo={data.combo} sampleCount={data.sample_count} />
      {match.normalizedDistance > NEAREST_COMBO_WARN_THRESHOLD && (
        <div className="combo-warning">
          No close combo match for the entered altitude/speed — showing the nearest available grid (ALT{" "}
          {data.combo.altitude_m}m, H {data.combo.h_speed_mps}m/s, V {data.combo.v_speed_mps}m/s). Double-check the
          values if this looks wrong.
        </div>
      )}
      <main>
        <SearchInner scenario={data} params={params} />
      </main>
      <footer>
        POLICY: info-gain (entropy reduction ÷ travel distance) · 40-scenario validation: 3.3× shorter path, 3.5×
        fewer steps to discovery vs. naive spiral search
      </footer>
    </>
  );
}

function SearchInner({ scenario, params }: { scenario: ComboBlob; params: UrlParams }) {
  const search = useSearchState(scenario);
  const { lat, lon } = params;
  const [trackingEnabled, setTrackingEnabled] = useState(false);

  // Stable object identity across re-renders — an inline `{ lat, lon }`
  // literal here would create a new object every render, which SearchMap's
  // memoized geo-conversion depends on (and which fed a since-removed
  // post-mount fitBounds() call that used to abort in-flight tile loads).
  const origin = useMemo(() => (lat !== null && lon !== null ? { lat, lon } : null), [lat, lon]);

  const geo = useGeolocation(trackingEnabled && origin !== null);

  return (
    <>
      <section className="panel map-panel">
        <div className="map-head">
          <span className="map-label">Landing probability field</span>
          <div className="legend">
            <span>
              <span className="swatch sw-heat" />
              probability
            </span>
            <span>
              <span className="swatch sw-covered" />
              searched
            </span>
            <span>
              <span className="swatch sw-next" />
              next point
            </span>
            {origin && (
              <span>
                <span className="swatch sw-you" />
                you
              </span>
            )}
          </div>
          {origin && (
            <button className="reset" type="button" onClick={() => setTrackingEnabled((v) => !v)}>
              {trackingEnabled ? "Tracking on" : "Track my location"}
            </button>
          )}
        </div>
        {origin ? (
          <SearchMap
            scenario={scenario}
            origin={origin}
            headingDeg={params.headingDeg}
            visited={search.visited}
            stepIndex={search.stepIndex}
            liveGps={geo.position}
            liveAccuracyM={geo.accuracyM}
          />
        ) : (
          <GridCanvas scenario={scenario} visited={search.visited} stepIndex={search.stepIndex} />
        )}
      </section>

      <div className="stack">
        <GuidancePanel search={search} headingDeg={params.headingDeg} />
      </div>
    </>
  );
}

export default function App() {
  const [params, setParams] = useState<UrlParams>(() => parseUrlParams(window.location.search));

  if (!hasCombo(params)) {
    return (
      <div className="app">
        <SetupForm initial={params} onSubmit={setParams} />
      </div>
    );
  }

  return (
    <div className="app">
      <SearchScreen params={params} />
    </div>
  );
}
