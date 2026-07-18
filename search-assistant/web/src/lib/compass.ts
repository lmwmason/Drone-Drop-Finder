const COMPASS_WORDS = ["N", "NE", "E", "SE", "S", "SW", "W", "NW"];

export function compassWord(bearingDeg: number): string {
  const idx = Math.round(bearingDeg / 45) % 8;
  return COMPASS_WORDS[idx];
}

/** "Straight ahead" / "Turn right N°" / "Turn left N°" relative to the phone's current heading. */
export function turnInstruction(
  deviceHeadingDeg: number | null,
  targetBearingDeg: number | null,
): string {
  if (deviceHeadingDeg === null || targetBearingDeg === null) return "—";

  const rel = (((targetBearingDeg - deviceHeadingDeg) % 360) + 360) % 360;
  if (rel <= 12 || rel >= 348) return "Straight ahead";
  if (rel < 180) return `Turn right ${Math.round(rel)}°`;
  return `Turn left ${Math.round(360 - rel)}°`;
}
