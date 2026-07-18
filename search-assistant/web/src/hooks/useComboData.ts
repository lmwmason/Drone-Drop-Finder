import { useEffect, useState } from "react";
import type { ComboBlob } from "../types";

interface ComboDataState {
  data: ComboBlob | null;
  loading: boolean;
  error: string | null;
}

const RETRY_ATTEMPTS = 3;
const RETRY_DELAY_MS = 800;

function sleep(ms: number) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

async function fetchWithRetry(url: string): Promise<ComboBlob> {
  let lastError: unknown;
  for (let attempt = 1; attempt <= RETRY_ATTEMPTS; attempt++) {
    try {
      const res = await fetch(url);
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      return (await res.json()) as ComboBlob;
    } catch (err) {
      lastError = err;
      if (attempt < RETRY_ATTEMPTS) await sleep(RETRY_DELAY_MS * attempt);
    }
  }
  throw lastError instanceof Error ? lastError : new Error("fetch failed");
}

export function useComboData(url: string | null): ComboDataState {
  const [state, setState] = useState<ComboDataState>({
    data: null,
    loading: false,
    error: null,
  });

  useEffect(() => {
    if (!url) {
      setState({ data: null, loading: false, error: null });
      return;
    }

    let cancelled = false;
    setState({ data: null, loading: true, error: null });

    fetchWithRetry(url)
      .then((data) => {
        if (!cancelled) setState({ data, loading: false, error: null });
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          const message = err instanceof Error ? err.message : "failed to load scenario";
          setState({ data: null, loading: false, error: message });
        }
      });

    return () => {
      cancelled = true;
    };
  }, [url]);

  return state;
}
