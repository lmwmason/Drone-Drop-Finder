import { useState } from "react";
import type { UrlParams } from "../types";

interface SetupFormProps {
  initial: UrlParams;
  onSubmit: (params: UrlParams) => void;
}

type FieldName = "lat" | "lon" | "altitudeM" | "hSpeedMps" | "vSpeedMps" | "headingDeg";

const FIELDS: { name: FieldName; label: string; placeholder: string }[] = [
  { name: "lat", label: "Latitude (from OSD GPS line)", placeholder: "-33.86785" },
  { name: "lon", label: "Longitude (from OSD GPS line)", placeholder: "151.20732" },
  { name: "altitudeM", label: "Altitude, m (OSD \"A\" value)", placeholder: "113" },
  { name: "hSpeedMps", label: "Horizontal speed, m/s (OSD \"H\" / 10)", placeholder: "11.4" },
  { name: "vSpeedMps", label: "Vertical speed, m/s (OSD \"V\" / 10)", placeholder: "-5.0" },
  { name: "headingDeg", label: "Heading, deg (OSD \"C\" value)", placeholder: "182" },
];

export function SetupForm({ initial, onSubmit }: SetupFormProps) {
  const [values, setValues] = useState<Record<FieldName, string>>({
    lat: initial.lat !== null ? String(initial.lat) : "",
    lon: initial.lon !== null ? String(initial.lon) : "",
    altitudeM: initial.altitudeM !== null ? String(initial.altitudeM) : "",
    hSpeedMps: initial.hSpeedMps !== null ? String(initial.hSpeedMps) : "",
    vSpeedMps: initial.vSpeedMps !== null ? String(initial.vSpeedMps) : "",
    headingDeg: initial.headingDeg !== null ? String(initial.headingDeg) : "",
  });
  const [error, setError] = useState<string | null>(null);

  function update(name: FieldName, value: string) {
    setValues((v) => ({ ...v, [name]: value }));
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const altitudeM = Number.parseFloat(values.altitudeM);
    const hSpeedMps = Number.parseFloat(values.hSpeedMps);
    const vSpeedMps = Number.parseFloat(values.vSpeedMps);
    if (!Number.isFinite(altitudeM) || !Number.isFinite(hSpeedMps) || !Number.isFinite(vSpeedMps)) {
      setError("Altitude, H-speed and V-speed are required to find a matching search grid.");
      return;
    }

    const lat = Number.parseFloat(values.lat);
    const lon = Number.parseFloat(values.lon);
    const headingDeg = Number.parseFloat(values.headingDeg);

    setError(null);
    onSubmit({
      altitudeM,
      hSpeedMps,
      vSpeedMps,
      lat: Number.isFinite(lat) ? lat : null,
      lon: Number.isFinite(lon) ? lon : null,
      headingDeg: Number.isFinite(headingDeg) ? headingDeg : null,
    });
  }

  return (
    <form className="panel setup-form" onSubmit={handleSubmit}>
      <div className="k">Enter values from the FC's OSD</div>
      {FIELDS.map((f) => (
        <label key={f.name}>
          {f.label}
          <input
            type="text"
            inputMode="decimal"
            placeholder={f.placeholder}
            value={values[f.name]}
            onChange={(e) => update(f.name, e.target.value)}
          />
        </label>
      ))}
      {error && <div className="error">{error}</div>}
      <button className="advance" type="submit">
        Find search area
      </button>
    </form>
  );
}
