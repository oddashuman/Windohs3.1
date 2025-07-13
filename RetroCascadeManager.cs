using UnityEngine;
using System.Collections;

/// <summary>
/// Null‑object stand‑in for the legacy RetroCascadeManager.
/// Matches the original public API so callers compile, but all methods are no‑ops.
/// </summary>
public class RetroCascadeManager : MonoBehaviour
{
    // Public API shim – visuals removed --------------------------------------
    public void EnableCascade()                           { /* no‑op */ }
    public void DisableCascade()                          { /* no‑op */ }
    public IEnumerator FadeOutCascade(float duration)     { yield break; }

    public void ShowFragment(string text)                 { /* no‑op */ }
    public void ShowViewerAcknowledgment(int viewerCount) { /* no‑op */ }

    public void TriggerCrisisVisuals()                    { /* no‑op */ }
    public void TriggerRealityQuestionEffect()            { /* no‑op */ }

    public void EnableBootSequence()                      { /* no‑op */ }

    // Debug helpers -----------------------------------------------------------
    public string GetDebugInfo() => "Cascade visual disabled (stub)";
}
