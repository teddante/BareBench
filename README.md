BareBench
=========

Minimal Unity 6 (URP) app for quick PC benchmarking. Opens a window and shows an FPS counter with optional summary metrics. VSync and caps are disabled by default.

Build/Run
- Open the project in Unity 6000.2.0f1 or newer.
- Build for your platform via File > Build Settings, or press Play in the Editor.

On‑Screen Overlay
- FPS updates every `updateInterval` seconds (default 0.25s).
- Toggle instantaneous FPS via `showInstantaneous` (off by default to reduce allocations).
- Overlay can be hidden via `showOverlay`.

Benchmarking
- Configure `warmupSeconds` to ignore startup frames.
- Set `benchmarkDurationSeconds` > 0 to auto‑stop and emit a summary (Avg, Min, Max, 1%/0.1% Lows). Optionally `quitOnFinish` to close after reporting.
- Optionally enable `sampleFrameTiming` to include average CPU/GPU frame times (FrameTimingManager).

Scene & Pipeline
- No post‑processing and no Global Volume.
- URP asset is simplified: no HDR, no MSAA, no shadows, no additional lights, no opaque/depth textures.
- SSAO renderer feature is disabled.

Caveats
- Variable Refresh Rate (G‑Sync/FreeSync), driver settings, power plans, or background apps can influence results.
- Laptop dGPU/IGPU switching and thermal throttling can affect sustained performance.
- FrameTimingManager GPU timings may require appropriate graphics drivers and may report 0 on some setups.

