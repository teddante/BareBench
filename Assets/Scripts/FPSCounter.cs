using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    // UI update frequency (seconds)
    [SerializeField] private float updateInterval = 0.25f;

    // Overlay options
    [SerializeField] private bool showOverlay = true;
    [SerializeField] private bool showInstantaneous = false; // disables per-repaint formatting by default

    // Benchmark options (no CLI/env): set in Inspector
    [SerializeField] private float warmupSeconds = 0f;              // frames ignored until this time
    [SerializeField] private float benchmarkDurationSeconds = 0f;   // 0 = run indefinitely
    [SerializeField] private bool reportOnFinish = true;            // print summary when duration elapses
    [SerializeField] private bool quitOnFinish = false;             // quit app after report
    [SerializeField] private int maxSamples = 200000;               // dt samples capacity (~0.8 MB)
    [SerializeField] private bool sampleFrameTiming = false;        // optional CPU/GPU timings

    private float accumTime;         // time since last UI update
    private int frames;              // frames since last UI update
    private float currentFPS;        // averaged fps over updateInterval

    // Stats accumulation (frametimes)
    private float[] dtSamples;
    private int sampleCount;
    private float recordedTime;
    private bool inWarmup;
    private float minDt = float.PositiveInfinity;
    private float maxDt = 0f;
    private ulong totalFrames;       // recorded frames (post-warmup)

    // Optional CPU/GPU frame timing accumulation
    private double cpuTimeSumMs;
    private double gpuTimeSumMs;
    private ulong timingSamples;

    private GUIStyle style;
    private Rect rect;
    private GUIContent overlayText = new GUIContent();

    void Awake()
    {
        // Ensure VSync is off to expose maximum FPS
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;

        style = new GUIStyle
        {
            fontSize = 20,
            normal = { textColor = Color.white }
        };
        rect = new Rect(10, 10, 520, 120);

        dtSamples = new float[Mathf.Max(1024, maxSamples)];
        inWarmup = warmupSeconds > 0f;
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        accumTime += dt;
        frames++;

        // Warmup handling
        if (inWarmup)
        {
            if (Time.unscaledTime >= warmupSeconds)
            {
                inWarmup = false;
                // reset stats at end of warmup
                sampleCount = 0;
                recordedTime = 0f;
                minDt = float.PositiveInfinity;
                maxDt = 0f;
                totalFrames = 0;
                cpuTimeSumMs = 0;
                gpuTimeSumMs = 0;
                timingSamples = 0;
            }
            else
            {
                // Skip recording during warmup
                dt = 0f; // don't add to recordedTime below
            }
        }

        // Record stats after warmup
        if (!inWarmup)
        {
            if (dt > 0f)
            {
                if (sampleCount < dtSamples.Length)
                {
                    dtSamples[sampleCount] = dt;
                    sampleCount++;
                }
                else
                {
                    // Ring buffer when full: overwrite oldest
                    int idx = (int)(totalFrames % (ulong)dtSamples.Length);
                    dtSamples[idx] = dt;
                }
                totalFrames++;
                recordedTime += dt;
                if (dt < minDt) minDt = dt;
                if (dt > maxDt) maxDt = dt;
            }

            if (sampleFrameTiming)
            {
                UnityEngine.FrameTimingManager.CaptureFrameTimings();
                UnityEngine.FrameTiming[] timings = new UnityEngine.FrameTiming[1];
                var count = UnityEngine.FrameTimingManager.GetLatestTimings(1, timings);
                if (count > 0)
                {
                    cpuTimeSumMs += timings[0].cpuFrameTime;
                    gpuTimeSumMs += timings[0].gpuFrameTime;
                    timingSamples++;
                }
            }
        }

        // Periodic UI update
        if (accumTime >= updateInterval)
        {
            var interval = Mathf.Max(accumTime, 1e-6f);
            currentFPS = frames / interval;
            frames = 0;
            accumTime = 0f;

            if (showOverlay)
            {
                overlayText.text = BuildOverlayText();
            }
        }

        // Auto-finish
        if (benchmarkDurationSeconds > 0f && !inWarmup)
        {
            if (recordedTime >= benchmarkDurationSeconds)
            {
                if (reportOnFinish)
                    ReportResults();

                if (quitOnFinish)
                {
                    Quit();
                }
                else
                {
                    enabled = false; // stop updates
                }
            }
        }
    }

    private string BuildOverlayText()
    {
        // Only minimal allocations: one string per interval
        float instant = showInstantaneous ? (1f / Mathf.Max(Time.unscaledDeltaTime, 1e-6f)) : 0f;
        string line1 = showInstantaneous
            ? $"FPS: {currentFPS:F1} (inst: {instant:F1})"
            : $"FPS: {currentFPS:F1}";

        if (totalFrames == 0)
            return line1;

        float avgFps = (float)(totalFrames / Mathf.Max(recordedTime, 1e-6f));
        float minFps = 1f / Mathf.Max(maxDt, 1e-6f);
        float maxFps = 1f / Mathf.Max(minDt, 1e-6f);

        if (sampleFrameTiming && timingSamples > 0)
        {
            double avgCpu = cpuTimeSumMs / timingSamples;
            double avgGpu = gpuTimeSumMs / timingSamples;
            return $"{line1}\navg: {avgFps:F1}  min: {minFps:F1}  max: {maxFps:F1}\nCPU: {avgCpu:F2} ms  GPU: {avgGpu:F2} ms";
        }
        else
        {
            return $"{line1}\navg: {avgFps:F1}  min: {minFps:F1}  max: {maxFps:F1}";
        }
    }

    private void ReportResults()
    {
        if (totalFrames == 0)
        {
            Debug.Log("BareBench: No frames recorded.");
            return;
        }

        int n = (int)Mathf.Min((ulong)dtSamples.Length, totalFrames);
        // If we wrapped, n is buffer size; otherwise n is sampleCount
        if (totalFrames < (ulong)dtSamples.Length) n = sampleCount;

        // Work on a local copy to avoid disturbing rolling buffer when used continuously
        float[] work = new float[n];
        System.Array.Copy(dtSamples, work, n);
        System.Array.Sort(work); // ascending dt (worst at end)

        float p99dt = work[Mathf.Clamp((int)Mathf.Floor(0.99f * (n - 1)), 0, n - 1)];
        float p999dt = work[Mathf.Clamp((int)Mathf.Floor(0.999f * (n - 1)), 0, n - 1)];

        float onePercentLow = 1f / Mathf.Max(p99dt, 1e-6f);
        float pointOnePercentLow = 1f / Mathf.Max(p999dt, 1e-6f);

        float avgFps = (float)(totalFrames / Mathf.Max(recordedTime, 1e-6f));
        float minFps = 1f / Mathf.Max(maxDt, 1e-6f);
        float maxFps = 1f / Mathf.Max(minDt, 1e-6f);

        string summary =
            $"BareBench Results\n" +
            $"Duration: {recordedTime:F2}s  Frames: {totalFrames}\n" +
            $"Avg: {avgFps:F1}  Min: {minFps:F1}  Max: {maxFps:F1}\n" +
            $"1% Low: {onePercentLow:F1}  0.1% Low: {pointOnePercentLow:F1}";

        Debug.Log(summary);

        if (showOverlay)
        {
            overlayText.text = summary;
        }
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnGUI()
    {
        if (!showOverlay) return;
        GUI.Label(rect, overlayText, style);
    }
}
