import { useMemo } from "react";
import { Circle, CircleMarker, MapContainer, Polyline, TileLayer } from "react-leaflet";
import type { ComboBlob, GeoPoint } from "../types";
import { bodyFrameToLatLon } from "../lib/geo";

const HEAT_LO: [number, number, number] = [18, 59, 58];
const HEAT_HI: [number, number, number] = [127, 224, 214];

function lerpColor(c0: [number, number, number], c1: [number, number, number], t: number): string {
  const r = Math.round(c0[0] + (c1[0] - c0[0]) * t);
  const g = Math.round(c0[1] + (c1[1] - c0[1]) * t);
  const b = Math.round(c0[2] + (c1[2] - c0[2]) * t);
  return `rgb(${r},${g},${b})`;
}

interface SearchMapProps {
  scenario: ComboBlob;
  origin: GeoPoint;
  headingDeg: number | null;
  visited: Set<string>;
  stepIndex: number;
  liveGps: GeoPoint | null;
  liveAccuracyM: number | null;
}

export function SearchMap({
  scenario,
  origin,
  headingDeg,
  visited,
  stepIndex,
  liveGps,
  liveAccuracyM,
}: SearchMapProps) {
  const heading = headingDeg ?? 0;
  const { cell_size_m: cellSizeM, grid, info_gain_path: path } = scenario;

  const cellPoints = useMemo(() => {
    let maxProb = 0;
    for (const p of Object.values(grid)) if (p > maxProb) maxProb = p;

    return Object.entries(grid).map(([key, p]) => {
      const [cx, cy] = key.split(",").map(Number);
      const xMeters = (cx + 0.5) * cellSizeM;
      const yMeters = (cy + 0.5) * cellSizeM;
      const latLon = bodyFrameToLatLon(origin, xMeters, yMeters, heading);
      const t = maxProb > 0 ? Math.pow(p / maxProb, 0.55) : 0;
      return { key, latLon, color: lerpColor(HEAT_LO, HEAT_HI, t) };
    });
  }, [grid, cellSizeM, origin, heading]);

  const pathLatLon = useMemo(
    () =>
      path.map(([px, py]) => {
        const xMeters = (px + 0.5) * cellSizeM;
        const yMeters = (py + 0.5) * cellSizeM;
        return bodyFrameToLatLon(origin, xMeters, yMeters, heading);
      }),
    [path, cellSizeM, origin, heading],
  );

  // Computed once (stable as long as the scenario/origin/heading don't change)
  // and passed to MapContainer's `bounds` prop so Leaflet settles on the final
  // view before the tile layer requests anything — calling map.fitBounds()
  // *after* mount instead aborts the initial tile batch and, for reasons
  // that smell like a Leaflet/GridLayer tile-key reuse bug, the replacement
  // tiles for the new view then never load either.
  const fitBounds = useMemo<[number, number][]>(
    () => cellPoints.map((c) => [c.latLon.lat, c.latLon.lon]),
    [cellPoints],
  );

  const current = pathLatLon[stepIndex];
  const next = stepIndex + 1 < pathLatLon.length ? pathLatLon[stepIndex + 1] : null;

  return (
    <MapContainer
      bounds={fitBounds.length > 0 ? fitBounds : undefined}
      boundsOptions={{ padding: [24, 24] }}
      center={fitBounds.length === 0 ? [origin.lat, origin.lon] : undefined}
      zoom={fitBounds.length === 0 ? 17 : undefined}
      className="leaflet-map"
      scrollWheelZoom
    >
      <TileLayer
        attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
      />

      {cellPoints.map(({ key, latLon, color }) => (
        <CircleMarker
          key={key}
          center={[latLon.lat, latLon.lon]}
          radius={8}
          pathOptions={{
            color: "transparent",
            fillColor: visited.has(key) ? "#3c9a63" : color,
            fillOpacity: visited.has(key) ? 0.75 : 0.65,
          }}
        />
      ))}

      <Polyline
        positions={pathLatLon.slice(0, stepIndex + 1).map((p) => [p.lat, p.lon])}
        pathOptions={{ color: "#ff7a1f", weight: 2.5, opacity: 0.9 }}
      />

      {current && (
        <CircleMarker
          center={[current.lat, current.lon]}
          radius={6}
          pathOptions={{ color: "#241304", weight: 1.5, fillColor: "#ff7a1f", fillOpacity: 1 }}
        />
      )}
      {next && (
        <CircleMarker
          center={[next.lat, next.lon]}
          radius={9}
          pathOptions={{ color: "#ff7a1f", weight: 2, fillOpacity: 0 }}
        />
      )}

      {liveGps && (
        <>
          <CircleMarker
            center={[liveGps.lat, liveGps.lon]}
            radius={7}
            pathOptions={{ color: "#0a2a3d", weight: 1.5, fillColor: "#4a9fd8", fillOpacity: 1 }}
          />
          {liveAccuracyM !== null && (
            <Circle
              center={[liveGps.lat, liveGps.lon]}
              radius={liveAccuracyM}
              pathOptions={{ color: "#4a9fd8", weight: 1, fillColor: "#4a9fd8", fillOpacity: 0.12 }}
            />
          )}
        </>
      )}
    </MapContainer>
  );
}
