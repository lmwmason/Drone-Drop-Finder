import type { UrlParams } from "../types";

function parseFloatInRange(
  raw: string | null,
  min: number,
  max: number,
): number | null {
  if (raw === null || raw === "") return null;
  const v = Number.parseFloat(raw);
  if (!Number.isFinite(v) || v < min || v > max) return null;
  return v;
}

export function parseUrlParams(search: string): UrlParams {
  const q = new URLSearchParams(search);
  return {
    lat: parseFloatInRange(q.get("lat"), -90, 90),
    lon: parseFloatInRange(q.get("lon"), -180, 180),
    altitudeM: parseFloatInRange(q.get("alt"), 0, 2000),
    hSpeedMps: parseFloatInRange(q.get("hspd"), 0, 200),
    vSpeedMps: parseFloatInRange(q.get("vspd"), -200, 200),
    headingDeg: parseFloatInRange(q.get("hdg"), 0, 360),
  };
}

export function hasCombo(p: UrlParams): boolean {
  return p.altitudeM !== null && p.hSpeedMps !== null && p.vSpeedMps !== null;
}

export function hasGps(p: UrlParams): boolean {
  return p.lat !== null && p.lon !== null;
}

export function toUrlParams(p: UrlParams): URLSearchParams {
  const q = new URLSearchParams();
  if (p.lat !== null) q.set("lat", String(p.lat));
  if (p.lon !== null) q.set("lon", String(p.lon));
  if (p.altitudeM !== null) q.set("alt", String(p.altitudeM));
  if (p.hSpeedMps !== null) q.set("hspd", String(p.hSpeedMps));
  if (p.vSpeedMps !== null) q.set("vspd", String(p.vSpeedMps));
  if (p.headingDeg !== null) q.set("hdg", String(p.headingDeg));
  return q;
}
