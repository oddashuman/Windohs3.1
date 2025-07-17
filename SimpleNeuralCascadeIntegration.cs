using UnityEngine;
using System.Collections;
using TMPro;

public class SimpleNeuralCascadeIntegration : MonoBehaviour
{
    [Header("Setup Configuration")]
    public bool debugMode = true;
    public bool skipBootSequence = false;
    
    [Header("Required Assets")]
    public Texture2D[] cursorTextures;
    public Texture2D[] iconTextures;
    public TMP_FontAsset windows31Font;
    
    [Header("Experience Settings")]
    public float minDesktopTime = 120f;
    public float maxDesktopTime = 300f;
    public int minConversationMessages = 8;
    public int maxConversationMessages = 20;

    void Update()
    {
        if (!debugMode) return;
        
        if (Input.GetKeyDown(KeyCode.F1)) LogSystemStatus();
        if (Input.GetKeyDown(KeyCode.F2)) ForceDesktopMode();
        if (Input.GetKeyDown(KeyCode.F3)) ForceTerminalMode();
        if (Input.GetKeyDown(KeyCode.F4)) TriggerCrisis();
        if (Input.GetKeyDown(KeyCode.F9)) ResetExperience();
    }

    public void LogSystemStatus()
    {
        Debug.Log("=== SYSTEM STATUS ===");
        if (CursorController.Instance != null) Debug.Log($"Cursor: Pos {CursorController.Instance.GetCurrentPosition()}, Moving: {CursorController.Instance.isMoving}");
        if (Windows31DesktopManager.Instance != null) Debug.Log($"Desktop: {Windows31DesktopManager.Instance.GetDebugInfo()}");
        if (SimulationController.Instance != null) Debug.Log($"Simulation: {SimulationController.Instance.GetDebugInfo()}");
        if (DialogueState.Instance != null) Debug.Log($"Dialogue State - Tension: {DialogueState.Instance.globalTension:F2}, Awareness: {DialogueState.Instance.metaAwareness:F2}");
    }

    public void ForceDesktopMode()
    {
        Debug.Log("Forcing desktop mode");
        if (Windows31DesktopManager.Instance != null) { /* desktop has no ShowDesktop() */ }
        if (MatrixTerminalManager.Instance != null) MatrixTerminalManager.Instance.DisableTerminal();
        if (CursorController.Instance != null) { /* cursor has no StartIdleMovement() */ }
    }

    public void ForceTerminalMode()
    {
        Debug.Log("Forcing terminal mode");
        if (Windows31DesktopManager.Instance != null) { /* desktop has no TriggerConversationMode() */ }
        if (MatrixTerminalManager.Instance != null) MatrixTerminalManager.Instance.EnableTerminal();
        if (CursorController.Instance != null) { /* cursor has no SetPrecisionMode() */ }
    }

    public void TriggerCrisis()
    {
        Debug.Log("Triggering crisis mode");
        if (SimulationController.Instance != null) SimulationController.Instance.TriggerCrisisMode();
        if (DialogueState.Instance != null)
        {
            DialogueState.Instance.globalTension = 1.0f;
            DialogueState.Instance.AddGlitchEvent("debug_crisis", "Debug crisis triggered", 3.0f);
            DialogueState.Instance.rareRedGlitchOccurred = true;
        }
    }

    public void ResetExperience()
    {
        Debug.Log("Resetting experience");
        if (SimulationController.Instance != null) SimulationController.Instance.ForceSessionReset();
        if (DialogueState.Instance != null) DialogueState.Instance.Reset();
        if (CursorController.Instance != null) { /* cursor has no ResetFatigue() */ }
    }

    public void InjectViewerMessage(string username, string message)
    {
        if (DialogueEngine.Instance != null) DialogueEngine.Instance.EnqueueUserMessage(username, message);
    }

    public void SetDesktopActivity(Windows31DesktopManager.DesktopActivity activity)
    {
        if (Windows31DesktopManager.Instance != null) Windows31DesktopManager.Instance.BeginActivity(activity);
    }
}