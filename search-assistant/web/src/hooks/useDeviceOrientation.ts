import { useCallback, useEffect, useRef, useState } from "react";

interface DeviceOrientationState {
  headingDeg: number | null;
  status: string;
  live: boolean;
  supported: boolean;
  enabled: boolean;
  requestEnable: () => void;
}

// iOS Safari only, not in lib.dom.d.ts.
interface WebkitOrientationEvent extends DeviceOrientationEvent {
  webkitCompassHeading?: number;
}

type DeviceOrientationEventConstructorWithPermission = typeof DeviceOrientationEvent & {
  requestPermission?: () => Promise<"granted" | "denied">;
};

export function useDeviceOrientation(): DeviceOrientationState {
  const [headingDeg, setHeadingDeg] = useState<number | null>(null);
  const [status, setStatus] = useState("compass off");
  const [live, setLive] = useState(false);
  const [enabled, setEnabled] = useState(false);
  const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const supported = typeof window !== "undefined" && "DeviceOrientationEvent" in window;

  const onOrientation = useCallback((e: DeviceOrientationEvent) => {
    const webkitEvent = e as WebkitOrientationEvent;
    let heading: number | null = null;
    if (typeof webkitEvent.webkitCompassHeading === "number" && !Number.isNaN(webkitEvent.webkitCompassHeading)) {
      heading = webkitEvent.webkitCompassHeading;
    } else if (typeof e.alpha === "number" && e.alpha !== null) {
      heading = (360 - e.alpha) % 360;
    }
    if (heading === null) return;
    setHeadingDeg(heading);
    setStatus("live");
    setLive(true);
  }, []);

  const startListening = useCallback(() => {
    window.addEventListener("deviceorientationabsolute", onOrientation, true);
    window.addEventListener("deviceorientation", onOrientation, true);
    setStatus("waiting for signal…");
    setEnabled(true);

    timeoutRef.current = setTimeout(() => {
      setHeadingDeg((current) => {
        if (current === null) setStatus("no signal — desktop/no sensor");
        return current;
      });
    }, 3000);
  }, [onOrientation]);

  const requestEnable = useCallback(() => {
    if (!supported) {
      setStatus("no sensor on this device");
      return;
    }

    const ctor = DeviceOrientationEvent as DeviceOrientationEventConstructorWithPermission;
    if (typeof ctor.requestPermission === "function") {
      ctor
        .requestPermission()
        .then((state) => {
          if (state === "granted") startListening();
          else setStatus("permission denied");
        })
        .catch(() => setStatus("permission error"));
    } else {
      startListening();
    }
  }, [supported, startListening]);

  useEffect(() => {
    return () => {
      window.removeEventListener("deviceorientationabsolute", onOrientation, true);
      window.removeEventListener("deviceorientation", onOrientation, true);
      if (timeoutRef.current) clearTimeout(timeoutRef.current);
    };
  }, [onOrientation]);

  useEffect(() => {
    if (!supported) setStatus("no sensor on this device");
  }, [supported]);

  return { headingDeg, status, live, supported, enabled, requestEnable };
}
