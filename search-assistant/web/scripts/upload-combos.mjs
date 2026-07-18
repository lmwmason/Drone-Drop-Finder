// Uploads the 400 precomputed combo grids (search-assistant/dist/combos/*.json,
// produced by ../../export_all_scenarios.py) to Vercel Blob, then writes
// src/data/combos-manifest.json with the resulting public URLs. Re-running
// this after a CSV refresh cleanly replaces the same blobs (deterministic
// pathnames + allowOverwrite) rather than accumulating orphaned versions.
//
// Requires BLOB_READ_WRITE_TOKEN in the environment. After `vercel link` +
// creating a Blob store in the dashboard (or `vercel blob store create`),
// get it with:
//   vercel env pull .env.local
// then run this script with:
//   node --env-file=.env.local scripts/upload-combos.mjs

import { put } from "@vercel/blob";
import { readFile, writeFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const WEB_DIR = path.resolve(__dirname, "..");
const DIST_DIR = path.resolve(WEB_DIR, "..", "dist");
const MANIFEST_IN = path.join(DIST_DIR, "manifest.json");
const MANIFEST_OUT = path.join(WEB_DIR, "src", "data", "combos-manifest.json");

async function main() {
  if (!process.env.BLOB_READ_WRITE_TOKEN) {
    console.error(
      "BLOB_READ_WRITE_TOKEN is not set. Run `vercel env pull .env.local` after creating a Blob store, " +
        "then re-run this script with `node --env-file=.env.local scripts/upload-combos.mjs`.",
    );
    process.exit(1);
  }

  const manifestRaw = await readFile(MANIFEST_IN, "utf8");
  const manifest = JSON.parse(manifestRaw);
  console.log(`Uploading ${manifest.length} combo grids…`);

  const out = [];
  for (const [i, entry] of manifest.entries()) {
    const localPath = path.join(DIST_DIR, entry.file);
    const body = await readFile(localPath);

    // Deterministic pathname — namespacing this under an airframe id later
    // (combo/{airframeId}/{alt}_{hspd}_{vspd}.json) is a one-line change
    // here whenever multi-airframe support gets built; not doing that now.
    const pathname = `combo/${entry.altitude_m}_${entry.h_speed_mps}_${entry.v_speed_mps}.json`;

    const blob = await put(pathname, body, {
      access: "public",
      addRandomSuffix: false,
      allowOverwrite: true,
      contentType: "application/json",
      cacheControlMaxAge: 31536000,
    });

    out.push({
      altitude_m: entry.altitude_m,
      h_speed_mps: entry.h_speed_mps,
      v_speed_mps: entry.v_speed_mps,
      sample_count: entry.sample_count,
      cell_count: entry.cell_count,
      url: blob.url,
    });

    if ((i + 1) % 50 === 0 || i + 1 === manifest.length) {
      console.log(`  ${i + 1}/${manifest.length}`);
    }
  }

  await writeFile(MANIFEST_OUT, JSON.stringify(out));
  console.log(`Wrote ${MANIFEST_OUT} (${out.length} entries).`);
  console.log("Remove web/public/combos/ now — it was only a local-dev stand-in for Blob.");
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});
