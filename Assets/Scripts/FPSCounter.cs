using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    // Update frequency for the label
    [SerializeField] private float updateInterval = 0.25f;

    private float accumTime;   // time since last update
    private int frames;        // frames since last update
    private float currentFPS;  // smoothed fps

    private GUIStyle style;
    private Rect rect;

    void Awake()
    {
        // Make sure VSync is off so we see real FPS ceiling
        QualitySettings.vSyncCount = 0;
        // Unlimited target framerate (let the system decide)
        Application.targetFrameRate = -1;

        style = new GUIStyle
        {
            fontSize = 20,
            normal = { textColor = Color.white }
        };
        rect = new Rect(10, 10, 400, 80);
    }

    void Update()
    {
        accumTime += Time.unscaledDeltaTime;
        frames++;

        if (accumTime >= updateInterval)
        {
            currentFPS = frames / accumTime;
            frames = 0;
            accumTime = 0f;
        }
    }

    void OnGUI()
    {
        // Instantaneous FPS
        float instant = 1f / Mathf.Max(Time.unscaledDeltaTime, 1e-6f);
        GUI.Label(rect, $"FPS: {currentFPS:F1} (instant: {instant:F1})", style);
    }
}
