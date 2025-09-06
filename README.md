BareBench
=========

Minimal Unity 6 (URP) app for quick PC benchmarking. Opens a window and shows an FPS counter with optional summary metrics. VSync and frame caps are disabled by default.

Plug-and-Play Usage
- Build the project or press Play in the Editor. That's it.
- BareBench runs a short benchmark automatically in both Editor and builds: 2s warmup + 20s benchmark, then shows a summary on screen. The window/play mode stays open so you can read or screenshot the results. Press Esc to quit.
- To run indefinitely, disable `autoRunShortBenchmark` on the `FPSCounter` component and leave `benchmarkDurationSeconds` at 0.

Build/Run
- Open the project in Unity 6000.2.0f1 or newer.
- File > Build Settings > Build and run your platform.

On-Screen Overlay
- FPS updates every `updateInterval` seconds (default 0.25s).
- Optional instantaneous FPS via `showInstantaneous` (off by default to reduce allocations).
- Overlay can be hidden via `showOverlay` (not recommended for quick checks).
- DPI-aware scaling: adjust `uiScale` on `FPSCounter` if needed; it also auto-scales based on DPI/screen height.

Benchmarking Options (Inspector)
- `warmupSeconds`: ignore startup frames (2s auto-applied if unset).
- `benchmarkDurationSeconds`: auto-stop and emit a summary (20s auto-applied if unset and `autoRunShortBenchmark` enabled). Set 0 to run indefinitely when auto-run is disabled.
- `reportOnFinish`: print summary to Console and overlay (on by default for auto-run).
- `quitOnFinish`: close the app after reporting (off by default so users can read results).
- `sampleFrameTiming`: include average CPU/GPU frame times (FrameTimingManager) when enabled.
- `autoRunShortBenchmark`: when enabled and `benchmarkDurationSeconds` ≤ 0, applies 2s warmup + 20s benchmark for plug-and-play.
- `uiScale`: additional multiplier for overlay scaling.

Scene & Pipeline
- No global post-processing volume in the scene.
- URP asset is simplified: no HDR, no MSAA, no shadows, no additional lights, no opaque/depth textures.
- SSAO renderer feature exists but is disabled.

Caveats
- Variable Refresh Rate (G-Sync/FreeSync), driver settings, power plans, or background apps can influence results.
- Laptop dGPU/iGPU switching and thermal throttling can affect sustained performance.
- FrameTimingManager GPU timings may require appropriate graphics drivers and may report 0 on some setups.

Changelog
- Cached `FrameTiming[]` to avoid per-frame GC when `sampleFrameTiming` is enabled.
- Added DPI-aware overlay scaling (`uiScale` + automatic scaling).
- Added `autoRunShortBenchmark` toggle to allow indefinite runs when disabled.
- Removed unused assets: InputSystem actions and SceneTemplate assets.
