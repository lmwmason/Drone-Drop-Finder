import { useEffect, useState } from "react";
import type { GeoPoint } from "../types";

interface GeolocationState {
  position: GeoPoint | null;
  accuracyM: number | null;
  status: "idle" | "watching" | "error" | "unsupported";
  error: string | null;
}

/**
 * Wraps navigator.geolocation.watchPosition for a "you are here" map overlay
 * only — this never drives search/visited state. Consumer GPS accuracy
 * (3-10 m, worse under canopy) is the same order of magnitude as the 4 m
 * grid cell, so auto-advancing coverage from raw GPS would risk corrupting
 * the info-gain UX's coverage percentage. The Advance button stays the only
 * way to mark a cell searched.
 */
export function useGeolocation(enabled: boolean): GeolocationState {
  const [state, setState] = useState<GeolocationState>({
    position: null,
    accuracyM: null,
    status: "idle",
    error: null,
  });

  useEffect(() => {
    if (!enabled) return;

    if (!("geolocation" in navigator)) {
      setState((s) => ({ ...s, status: "unsupported", error: "Geolocation not supported on this device" }));
      return;
    }

    setState((s) => ({ ...s, status: "watching" }));

    const watchId = navigator.geolocation.watchPosition(
      (pos) => {
        setState({
          position: { lat: pos.coords.latitude, lon: pos.coords.longitude },
          accuracyM: pos.coords.accuracy,
          status: "watching",
          error: null,
        });
      },
      (err) => {
        setState((s) => ({ ...s, status: "error", error: err.message }));
      },
      { enableHighAccuracy: true, maximumAge: 5000, timeout: 15000 },
    );

    return () => navigator.geolocation.clearWatch(watchId);
  }, [enabled]);

  return state;
}
