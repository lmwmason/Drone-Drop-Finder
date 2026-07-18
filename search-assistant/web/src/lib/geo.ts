import type { GeoPoint } from "../types";

const METERS_PER_DEGREE_LAT = 111320;
const DEG2RAD = Math.PI / 180;
const RAD2DEG = 180 / Math.PI;

/**
 * Rotates a body-frame offset (+X forward, +Y right, at the moment of
 * failure) into a real-world lat/lon, using the drone's heading at that
 * moment. Rotation formula matches library/README.md section 5:
 *   north_m = x*cos(heading) - y*sin(heading)
 *   east_m  = x*sin(heading) + y*cos(heading)
 */
export function bodyFrameToLatLon(
  origin: GeoPoint,
  xMeters: number,
  yMeters: number,
  headingDeg: number,
): GeoPoint {
  const headingRad = headingDeg * DEG2RAD;
  const northM = xMeters * Math.cos(headingRad) - yMeters * Math.sin(headingRad);
  const eastM = xMeters * Math.sin(headingRad) + yMeters * Math.cos(headingRad);

  const lat = origin.lat + northM / METERS_PER_DEGREE_LAT;
  const metersPerDegreeLon = METERS_PER_DEGREE_LAT * Math.cos(origin.lat * DEG2RAD);
  const lon = origin.lon + eastM / metersPerDegreeLon;

  return { lat, lon };
}

/** Inverse of the north/east part of bodyFrameToLatLon — local flat-earth offset, fine at search-radius scale (tens of meters). */
export function latLonToMeters(origin: GeoPoint, point: GeoPoint): { northM: number; eastM: number } {
  const northM = (point.lat - origin.lat) * METERS_PER_DEGREE_LAT;
  const metersPerDegreeLon = METERS_PER_DEGREE_LAT * Math.cos(origin.lat * DEG2RAD);
  const eastM = (point.lon - origin.lon) * metersPerDegreeLon;
  return { northM, eastM };
}

export function distanceMeters(a: GeoPoint, b: GeoPoint): number {
  const { northM, eastM } = latLonToMeters(a, b);
  return Math.hypot(northM, eastM);
}

/** True-north bearing (0-360) from a to b. */
export function bearingDeg(a: GeoPoint, b: GeoPoint): number {
  const { northM, eastM } = latLonToMeters(a, b);
  return (Math.atan2(eastM, northM) * RAD2DEG + 360) % 360;
}
