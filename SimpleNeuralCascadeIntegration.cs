using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Simplified integration script for Neural Cascade Windows 3.1 experience.
/// This version avoids compilation errors and works with existing codebase.
/// </summary>
public class SimpleNeuralCascadeIntegration : MonoBehaviour
{
    [Header("Setup Configuration")]
    public bool autoInitialize = true;
    public bool debugMode = true;
    public bool skipBootSequence = false;
    
    [Header("Required Assets")]
    public Texture2D[] cursorTextures;
    public Texture2D[] iconTextures;
    public TMP_FontAsset windows31Font; // Changed from Font to TMP_FontAsset
    
    [Header("Experience Settings")]
    public float minDesktopTime = 120f;
    public float maxDesktopTime = 300f;
    public int minConversationMessages = 8;
    public int maxConversationMessages = 20;
    
    private bool isSetupComplete = false;

    void Start()
    {
        if (autoInitialize)
        {
            StartCoroutine(InitializeExperience());
        }
    }

    IEnumerator InitializeExperience()
    {
        Debug.Log("=== NEURAL CASCADE WINDOWS 3.1 INITIALIZATION ===");
        
        // Step 1: Initialize cursor system
        yield return StartCoroutine(SetupCursorController());
        
        // Step 2: Initialize desktop system  
        yield return StartCoroutine(SetupDesktopManager());
        
        // Step 3: Update simulation controller
        yield return StartCoroutine(UpdateSimulationController());
        
        // Step 4: Verify systems
        yield return StartCoroutine(VerifySetup());
        
        isSetupComplete = true;
        Debug.Log("=== NEURAL CASCADE READY ===");
    }

    IEnumerator SetupCursorController()
    {
        Debug.Log("Setting up cursor system...");
        
        if (CursorController.Instance == null)
        {
            GameObject cursorGO = new GameObject("CursorController");
            cursorGO.transform.SetParent(transform);
            CursorController cursor = cursorGO.AddComponent<CursorController>();
            
            if (cursorTextures != null && cursorTextures.Length > 0)
            {
                cursor.cursorTextures = cursorTextures;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("✓ Cursor system ready");
    }

    IEnumerator SetupDesktopManager()
    {
        Debug.Log("Setting up Windows 3.1 desktop...");
        
        if (Windows31DesktopManager.Instance == null)
        {
            GameObject desktopGO = new GameObject("Windows31DesktopManager");
            desktopGO.transform.SetParent(transform);
            Windows31DesktopManager desktop = desktopGO.AddComponent<Windows31DesktopManager>();
            
            if (windows31Font != null)
            {
                desktop.windows31Font = windows31Font;
            }
            
            if (iconTextures != null)
            {
                desktop.iconTextures = iconTextures;
            }
            
            desktop.skipBootOnRestart = skipBootSequence;
            // Note: minDesktopTime and maxDesktopTime are not properties of Windows31DesktopManager
            // They are managed by SimulationController
            
            yield return new WaitForSeconds(0.2f);
        }
        
        Debug.Log("✓ Desktop system ready");
    }

    IEnumerator UpdateSimulationController()
    {
        Debug.Log("Updating simulation controller...");
        
        // Find existing simulation controller or create new one
        SimulationController simController = SimulationController.Instance;
        if (simController == null)
        {
            simController = FindObjectOfType<SimulationController>();
        }
        
        if (simController != null)
        {
            // Update settings
            simController.debugMode = debugMode;
            simController.skipBootSequence = skipBootSequence;
            simController.minDesktopTime = minDesktopTime;
            simController.maxDesktopTime = maxDesktopTime;
            simController.minTerminalMessages = minConversationMessages;
            simController.maxTerminalMessages = maxConversationMessages;
        }
        
        yield return new WaitForSeconds(0.1f);
        Debug.Log("✓ Simulation controller updated");
    }

    IEnumerator VerifySetup()
    {
        Debug.Log("Verifying system integration...");
        
        bool allGood = true;
        
        // Check cursor
        if (CursorController.Instance == null)
        {
            Debug.LogWarning("⚠ CursorController not found");
            allGood = false;
        }
        
        // Check desktop
        if (Windows31DesktopManager.Instance == null)
        {
            Debug.LogWarning("⚠ Windows31DesktopManager not found");
            allGood = false;
        }
        
        // Check simulation controller
        if (SimulationController.Instance == null)
        {
            Debug.LogWarning("⚠ SimulationController not found");
            allGood = false;
        }
        
        // Check dialogue engine
        if (DialogueEngine.Instance == null || !DialogueEngine.Instance.IsReady())
        {
            Debug.LogWarning("⚠ DialogueEngine not ready");
            allGood = false;
        }
        
        if (allGood)
        {
            Debug.Log("✓ All systems verified and ready");
        }
        else
        {
            Debug.LogWarning("⚠ Some systems have issues but proceeding anyway");
        }
        
        yield return new WaitForSeconds(0.1f);
    }

    #region Debug Controls

    void Update()
    {
        if (!debugMode) return;
        
        // Debug key controls
        if (Input.GetKeyDown(KeyCode.F1))
        {
            LogSystemStatus();
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ForceDesktopMode();
        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ForceTerminalMode();
        }
        
        if (Input.GetKeyDown(KeyCode.F4))
        {
            TriggerCrisis();
        }
        
        if (Input.GetKeyDown(KeyCode.F9))
        {
            ResetExperience();
        }
    }

    public void LogSystemStatus()
    {
        Debug.Log("=== SYSTEM STATUS ===");
        
        if (CursorController.Instance != null)
        {
            Debug.Log($"Cursor: Position {CursorController.Instance.GetCurrentPosition()}, Moving: {CursorController.Instance.IsMoving()}");
        }
        
        if (Windows31DesktopManager.Instance != null)
        {
            Debug.Log($"Desktop: {Windows31DesktopManager.Instance.GetDebugInfo()}");
        }
        
        if (SimulationController.Instance != null)
        {
            Debug.Log($"Simulation: {SimulationController.Instance.GetDebugInfo()}");
        }
        
        if (DialogueState.Instance != null)
        {
            Debug.Log($"Dialogue State - Tension: {DialogueState.Instance.globalTension:F2}, Awareness: {DialogueState.Instance.metaAwareness:F2}");
        }
    }

    public void ForceDesktopMode()
    {
        Debug.Log("Forcing desktop mode");
        
        if (Windows31DesktopManager.Instance != null)
        {
            Windows31DesktopManager.Instance.ShowDesktop();
        }
        
        if (MatrixTerminalManager.Instance != null)
        {
            MatrixTerminalManager.Instance.DisableTerminal();
        }
        
        if (CursorController.Instance != null)
        {
            CursorController.Instance.StartIdleMovement();
        }
    }

    public void ForceTerminalMode()
    {
        Debug.Log("Forcing terminal mode");
        
        if (Windows31DesktopManager.Instance != null)
        {
            Windows31DesktopManager.Instance.TriggerConversationMode();
        }
        
        if (MatrixTerminalManager.Instance != null)
        {
            MatrixTerminalManager.Instance.EnableTerminal();
        }
        
        if (CursorController.Instance != null)
        {
            CursorController.Instance.SetPrecisionMode(true);
            CursorController.Instance.StopIdleMovement();
        }
    }

    public void TriggerCrisis()
    {
        Debug.Log("Triggering crisis mode");
        
        if (SimulationController.Instance != null)
        {
            SimulationController.Instance.TriggerCrisisMode();
        }
        
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
        
        if (SimulationController.Instance != null)
        {
            SimulationController.Instance.ForceSessionReset();
        }
        
        if (DialogueState.Instance != null)
        {
            DialogueState.Instance.Reset();
        }
        
        if (CursorController.Instance != null)
        {
            CursorController.Instance.ResetFatigue();
            CursorController.Instance.SetTensionLevel(0.3f);
            CursorController.Instance.SetConfidenceLevel(0.7f);
        }
    }

    #endregion

    #region Public API

    public bool IsReady()
    {
        return isSetupComplete;
    }

    public void InjectViewerMessage(string username, string message)
    {
        if (DialogueEngine.Instance != null)
        {
            DialogueEngine.Instance.EnqueueUserMessage(username, message);
        }
        
        Debug.Log($"Injected viewer message - {username}: {message}");
    }

    public void MoveCursorTo(Vector2 position, bool immediate = false)
    {
        if (CursorController.Instance != null)
        {
            CursorController.Instance.MoveTo(position, immediate);
        }
    }

    public void ClickAt(Vector2 position, bool doubleClick = false)
    {
        if (CursorController.Instance != null)
        {
            if (doubleClick)
                CursorController.Instance.Click(position, true);
            else
                CursorController.Instance.Click(position);
        }
    }

    public void SetDesktopActivity(Windows31DesktopManager.DesktopActivity activity)
    {
        if (Windows31DesktopManager.Instance != null)
        {
            Windows31DesktopManager.Instance.BeginActivity(activity);
        }
    }

    #endregion

    void OnGUI()
    {
        if (!debugMode || !isSetupComplete) return;
        
        // Simple debug overlay
        GUILayout.BeginArea(new Rect(10, 10, 250, 300));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("Neural Cascade Debug", "label");
        GUILayout.Space(5);
        
        // Status info
        if (SimulationController.Instance != null)
        {
            GUILayout.Label($"Mode: {SimulationController.Instance.currentMode}");
            GUILayout.Label($"Crisis: {SimulationController.Instance.IsInCrisisMode()}");
            GUILayout.Label($"Viewers: {SimulationController.Instance.GetViewerCount()}");
        }
        
        if (DialogueState.Instance != null)
        {
            GUILayout.Label($"Tension: {DialogueState.Instance.globalTension:F2}");
            GUILayout.Label($"Awareness: {DialogueState.Instance.metaAwareness:F2}");
        }
        
        GUILayout.Space(10);
        
        // Control buttons
        if (GUILayout.Button("System Status (F1)"))
            LogSystemStatus();
            
        if (GUILayout.Button("Force Desktop (F2)"))
            ForceDesktopMode();
            
        if (GUILayout.Button("Force Terminal (F3)"))
            ForceTerminalMode();
            
        if (GUILayout.Button("Trigger Crisis (F4)"))
            TriggerCrisis();
            
        if (GUILayout.Button("Reset Experience (F9)"))
            ResetExperience();
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}