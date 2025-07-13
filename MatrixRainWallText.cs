using UnityEngine;
using System.Collections;

/// <summary>
/// Null-object replacement for the old MatrixRainTextWall visual.
/// Keeps the public API intact so existing calls compile, but every
/// method is a no-op, so no rain will ever be shown.
/// </summary>
public class MatrixRainTextWall : MonoBehaviour
{
    // ─── Public API shim ────────────────────────────────────────────────────
    public void EnableRain()                                    { /* no-op */ }
    public void DisableRain()                                   { /* no-op */ }
    public IEnumerator FadeOutRain(float duration)              { yield break; }

    public void ShowFragment(string text)                       { /* no-op */ }
    public void DebugShowFragment(string text)                  { /* no-op */ }
    public void ShowViewerAcknowledgment(int viewerCount)       { /* no-op */ }

    public void TriggerCrisisVisuals()                          { /* no-op */ }
    public void TriggerRealityQuestionEffect()                  { /* no-op */ }

    // Boot-sequence helper (legacy call from SimulationController)
    public void EnableBootSequence()                            { /* no-op */ }

    // Debug helper so other scripts can print status
    public string GetDebugInfo() => "Rain visual disabled (stub)";
}
