import type { Combo } from "../types";

interface HeaderProps {
  combo: Combo | null;
  sampleCount: number | null;
}

export function Header({ combo, sampleCount }: HeaderProps) {
  return (
    <header>
      <div className="brand">
        <span className="mark">◎</span>
        <h1>Search Guide</h1>
      </div>
      <div className="combo-readout">
        {combo && (
          <>
            ALT <b>{combo.altitude_m}m</b>&nbsp; H-SPD <b>{combo.h_speed_mps}m/s</b>&nbsp; V-SPD{" "}
            <b>{combo.v_speed_mps}m/s</b>&nbsp; N <b>{sampleCount}</b>
          </>
        )}
      </div>
    </header>
  );
}
