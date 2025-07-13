using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Master setup script for the Neural Cascade Windows 3.1 experience.
/// This script demonstrates the complete integration and setup of all systems.
/// Place this on a GameObject in your scene to initialize the entire experience.
/// </summary>
public class NeuralCascadeSetup : MonoBehaviour
{
    [Header("Setup Configuration")]
    public bool autoInitialize = true;
    public bool debugMode = true;
    public bool skipBootSequence = false;
    
    [Header("Required Assets (Assign in Inspector)")]
    public Texture2D[] cursorTextures;
    public Texture2D[] iconTextures;
    public TMP_FontAsset windows31FontAsset; // Changed from Font to TMP_FontAsset
    public AudioClip[] systemSounds;
    
    [Header("Experience Settings")]
    public float minDesktopTime = 120f;    // 2 minutes for testing
    public float maxDesktopTime = 300f;    // 5 minutes for testing  
    public int minConversationMessages = 8;
    public int maxConversationMessages = 20;
    
    private bool isSetupComplete = false;

    void Start()
    {
        if (autoInitialize)
        {
            StartCoroutine(InitializeNeuralCascadeExperience());
        }
    }

    /// <summary>
    /// Complete initialization sequence for the Neural Cascade experience
    /// </summary>
    public IEnumerator InitializeNeuralCascadeExperience()
    {
        Debug.Log("=== NEURAL CASCADE INITIALIZATION ===");
        
        // Step 1: Initialize core dialogue system
        yield return StartCoroutine(InitializeDialogueSystem());
        
        // Step 2: Initialize cursor controller
        yield return StartCoroutine(InitializeCursorSystem());
        
        // Step 3: Initialize Windows 3.1 desktop
        yield return StartCoroutine(InitializeDesktopSystem());
        
        // Step 4: Initialize terminal system  
        yield return StartCoroutine(InitializeTerminalSystem());
        
        // Step 5: Initialize simulation controller
        yield return StartCoroutine(InitializeSimulationController());
        
        // Step 6: Verify all systems
        yield return StartCoroutine(VerifySystemIntegration());
        
        isSetupComplete = true;
        Debug.Log("=== NEURAL CASCADE READY ===");
        
        // Start the experience
        BeginExperience();
    }

    IEnumerator InitializeDialogueSystem()
    {
        Debug.Log("Initializing dialogue system...");
        
        // DialogueState (singleton)
        if (DialogueState.Instance == null)
        {
            var dsGO = new GameObject("DialogueState");
            dsGO.transform.SetParent(transform);
            dsGO.AddComponent<DialogueState>();
            yield return new WaitForSeconds(0.1f);
        }

        // TopicManager
        if (TopicManager.Instance == null)
        {
            var tmGO = new GameObject("TopicManager");
            tmGO.transform.SetParent(transform);
            tmGO.AddComponent<TopicManager>();
            yield return new WaitForSeconds(0.1f);
        }

        // ConversationThreadManager
        if (ConversationThreadManager.Instance == null)
        {
            var ctmGO = new GameObject("ConversationThreadManager");
            ctmGO.transform.SetParent(transform);
            ctmGO.AddComponent<ConversationThreadManager>();
            yield return new WaitForSeconds(0.1f);
        }

        // DialogueEngine
        if (DialogueEngine.Instance == null)
        {
            var deGO = new GameObject("DialogueEngine");
            deGO.transform.SetParent(transform);
            var dialogueEngine = deGO.AddComponent<DialogueEngine>();
            yield return new WaitForSeconds(0.2f);
        }

        // Wait for dialogue engine to be ready
        float timeout = 10f;
        float start = Time.time;
        while ((DialogueEngine.Instance == null || !DialogueEngine.Instance.IsReady()) 
               && Time.time - start < timeout)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("Dialogue system initialized ✓");
    }

    IEnumerator InitializeCursorSystem()
    {
        Debug.Log("Initializing cursor system...");
        
        if (CursorController.Instance == null)
        {
            var cursorGO = new GameObject("CursorController");
            cursorGO.transform.SetParent(transform);
            var cursorController = cursorGO.AddComponent<CursorController>();
            
            // Assign cursor textures
            if (cursorTextures != null && cursorTextures.Length > 0)
            {
                cursorController.cursorTextures = cursorTextures;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("Cursor system initialized ✓");
    }

    IEnumerator InitializeDesktopSystem()
    {
        Debug.Log("Initializing Windows 3.1 desktop system...");
        
        if (Windows31DesktopManager.Instance == null)
        {
            var desktopGO = new GameObject("Windows31DesktopManager");
            desktopGO.transform.SetParent(transform);
            var desktopManager = desktopGO.AddComponent<Windows31DesktopManager>();
            
            // Configure desktop settings
            if (windows31FontAsset != null)
            {
                desktopManager.windows31Font = windows31FontAsset;
            }
            
            if (iconTextures != null)
            {
                desktopManager.iconTextures = iconTextures;
            }
            
            desktopManager.skipBootOnRestart = skipBootSequence;
            
            yield return new WaitForSeconds(0.2f);
        }
        
        // Wait for desktop to be ready
        float timeout = 10f;
        float start = Time.time;
        while ((Windows31DesktopManager.Instance == null || !Windows31DesktopManager.Instance.IsReady()) 
               && Time.time - start < timeout)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("Desktop system initialized ✓");
    }

    IEnumerator InitializeTerminalSystem()
    {
        Debug.Log("Initializing terminal system...");
        
        if (MatrixTerminalManager.Instance == null)
        {
            var terminalGO = new GameObject("MatrixTerminalManager");
            terminalGO.transform.SetParent(transform);
            var terminalManager = terminalGO.AddComponent<MatrixTerminalManager>();
            
            // Configure terminal settings for Windows 3.1 integration
            terminalManager.enableRetroUI = true;
            terminalManager.enableCRTEffects = true;
            
            yield return new WaitForSeconds(0.2f);
        }
        
        // Wait for terminal to be ready
        float timeout = 10f;
        float start = Time.time;
        while ((MatrixTerminalManager.Instance == null || !MatrixTerminalManager.Instance.IsReady()) 
               && Time.time - start < timeout)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("Terminal system initialized ✓");
    }

    IEnumerator InitializeSimulationController()
    {
        Debug.Log("Initializing simulation controller...");
        
        if (SimulationController.Instance == null)
        {
            var simGO = new GameObject("SimulationController");
            simGO.transform.SetParent(transform);
            var simController = simGO.AddComponent<SimulationController>();
            
            // Configure simulation settings
            simController.debugMode = debugMode;
            simController.skipBootSequence = skipBootSequence;
            simController.minDesktopTime = minDesktopTime;
            simController.maxDesktopTime = maxDesktopTime;
            simController.minTerminalMessages = minConversationMessages;
            simController.maxTerminalMessages = maxConversationMessages;
            
            yield return new WaitForSeconds(0.3f);
        }
        
        // Wait for simulation controller to be ready
        float timeout = 15f;
        float start = Time.time;
        while ((SimulationController.Instance == null || !SimulationController.Instance.IsReady()) 
               && Time.time - start < timeout)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("Simulation controller initialized ✓");
    }

    IEnumerator VerifySystemIntegration()
    {
        Debug.Log("Verifying system integration...");
        
        bool allSystemsReady = true;
        string status = "System Status:\n";
        
        // Check DialogueEngine
        if (DialogueEngine.Instance != null && DialogueEngine.Instance.IsReady())
        {
            status += "✓ DialogueEngine: Ready\n";
        }
        else
        {
            status += "✗ DialogueEngine: Not Ready\n";
            allSystemsReady = false;
        }
        
        // Check CursorController
        if (CursorController.Instance != null)
        {
            status += "✓ CursorController: Ready\n";
        }
        else
        {
            status += "✗ CursorController: Not Ready\n";
            allSystemsReady = false;
        }
        
        // Check Windows31DesktopManager
        if (Windows31DesktopManager.Instance != null && Windows31DesktopManager.Instance.IsReady())
        {
            status += "✓ Windows31DesktopManager: Ready\n";
        }
        else
        {
            status += "✗ Windows31DesktopManager: Not Ready\n";
            allSystemsReady = false;
        }
        
        // Check MatrixTerminalManager
        if (MatrixTerminalManager.Instance != null && MatrixTerminalManager.Instance.IsReady())
        {
            status += "✓ MatrixTerminalManager: Ready\n";
        }
        else
        {
            status += "✗ MatrixTerminalManager: Not Ready\n";
            allSystemsReady = false;
        }
        
        // Check SimulationController
        if (SimulationController.Instance != null && SimulationController.Instance.IsReady())
        {
            status += "✓ SimulationController: Ready\n";
        }
        else
        {
            status += "✗ SimulationController: Not Ready\n";
            allSystemsReady = false;
        }
        
        Debug.Log(status);
        
        if (!allSystemsReady)
        {
            Debug.LogWarning("Some systems are not ready, but proceeding anyway");
        }
        
        yield return new WaitForSeconds(0.1f);
    }

    void BeginExperience()
    {
        Debug.Log("Starting Neural Cascade Windows 3.1 Experience");
        
        // The SimulationController will handle the rest of the experience flow
        if (SimulationController.Instance != null)
        {
            Debug.Log("Experience is now running - watch Orion use his computer!");
        }
        else
        {
            Debug.LogError("SimulationController not found - experience cannot start");
        }
    }

    #region Debug Helpers

    void Update()
    {
        if (!debugMode || !isSetupComplete) return;
        
        // Debug controls
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
        
        if (DialogueState.Instance != null)
        {
            var debugState = DialogueState.Instance.GetDebugState();
            foreach (var kvp in debugState)
            {
                Debug.Log($"DialogueState.{kvp.Key}: {kvp.Value}");
            }
        }
        
        if (DialogueEngine.Instance != null)
        {
            var debugInfo = DialogueEngine.Instance.GetConversationDebugInfo();
            foreach (string line in debugInfo)
            {
                Debug.Log(line);
            }
        }
        
        if (SimulationController.Instance != null)
        {
            Debug.Log($"SimulationController: {SimulationController.Instance.GetDebugInfo()}");
        }
        
        if (Windows31DesktopManager.Instance != null)
        {
            Debug.Log($"DesktopManager: {Windows31DesktopManager.Instance.GetDebugInfo()}");
        }
        
        if (MatrixTerminalManager.Instance != null)
        {
            Debug.Log($"TerminalManager: {MatrixTerminalManager.Instance.GetDebugInfo()}");
        }
    }

    public void ForceDesktopMode()
    {
        Debug.Log("Forcing desktop mode");
        
        if (SimulationController.Instance != null)
        {
            SimulationController.Instance.StopAllCoroutines();
            SimulationController.Instance.currentMode = SimulationController.Mode.DesktopActivity;
            
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
    }

    public void ForceTerminalMode()
    {
        Debug.Log("Forcing terminal mode");
        
        if (SimulationController.Instance != null)
        {
            SimulationController.Instance.StopAllCoroutines();
            SimulationController.Instance.currentMode = SimulationController.Mode.Terminal;
            
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
        Debug.Log("Resetting entire experience");
        
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

    #region Public API for External Control

    /// <summary>
    /// Manually initialize the experience (if autoInitialize is false)
    /// </summary>
    public void ManualInitialize()
    {
        if (!isSetupComplete)
        {
            StartCoroutine(InitializeNeuralCascadeExperience());
        }
    }

    /// <summary>
    /// Check if the experience is ready to run
    /// </summary>
    public bool IsExperienceReady()
    {
        return isSetupComplete &&
               DialogueEngine.Instance != null && DialogueEngine.Instance.IsReady() &&
               CursorController.Instance != null &&
               Windows31DesktopManager.Instance != null && Windows31DesktopManager.Instance.IsReady() &&
               MatrixTerminalManager.Instance != null && MatrixTerminalManager.Instance.IsReady() &&
               SimulationController.Instance != null && SimulationController.Instance.IsReady();
    }

    /// <summary>
    /// Get current experience status for external monitoring
    /// </summary>
    public string GetExperienceStatus()
    {
        if (!isSetupComplete)
            return "Initializing...";
            
        if (SimulationController.Instance != null)
        {
            return SimulationController.Instance.GetDebugInfo();
        }
        
        return "Ready";
    }

    /// <summary>
    /// Inject a viewer message into the conversation
    /// </summary>
    public void InjectViewerMessage(string username, string message)
    {
        if (DialogueEngine.Instance != null)
        {
            DialogueEngine.Instance.EnqueueUserMessage(username, message);
        }
        
        Debug.Log($"Injected viewer message - {username}: {message}");
    }

    /// <summary>
    /// Simulate specific desktop activities for testing
    /// </summary>
    public void TriggerDesktopActivity(Windows31DesktopManager.DesktopActivity activity)
    {
        if (Windows31DesktopManager.Instance != null)
        {
            Windows31DesktopManager.Instance.BeginActivity(activity);
        }
    }

    /// <summary>
    /// Move cursor to specific position (for testing)
    /// </summary>
    public void MoveCursorTo(Vector2 position, bool immediate = false)
    {
        if (CursorController.Instance != null)
        {
            CursorController.Instance.MoveTo(position, immediate);
        }
    }

    /// <summary>
    /// Click at specific position (for testing)
    /// </summary>
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

    #endregion

    #region Configuration Helpers

    /// <summary>
    /// Configure experience timing for different scenarios
    /// </summary>
    public void ConfigureExperiencePacing(ExperiencePacing pacing)
    {
        switch (pacing)
        {
            case ExperiencePacing.Demo:
                minDesktopTime = 30f;      // 30 seconds
                maxDesktopTime = 90f;      // 1.5 minutes
                minConversationMessages = 5;
                maxConversationMessages = 12;
                break;
                
            case ExperiencePacing.Testing:
                minDesktopTime = 60f;      // 1 minute
                maxDesktopTime = 180f;     // 3 minutes
                minConversationMessages = 8;
                maxConversationMessages = 20;
                break;
                
            case ExperiencePacing.Production:
                minDesktopTime = 180f;     // 3 minutes
                maxDesktopTime = 600f;     // 10 minutes
                minConversationMessages = 15;
                maxConversationMessages = 40;
                break;
                
            case ExperiencePacing.Extended:
                minDesktopTime = 300f;     // 5 minutes
                maxDesktopTime = 1200f;    // 20 minutes
                minConversationMessages = 20;
                maxConversationMessages = 60;
                break;
        }
        
        // Apply settings to simulation controller if it exists
        if (SimulationController.Instance != null)
        {
            SimulationController.Instance.minDesktopTime = minDesktopTime;
            SimulationController.Instance.maxDesktopTime = maxDesktopTime;
            SimulationController.Instance.minTerminalMessages = minConversationMessages;
            SimulationController.Instance.maxTerminalMessages = maxConversationMessages;
        }
        
        Debug.Log($"Experience pacing configured: {pacing}");
    }

    /// <summary>
    /// Set personality parameters for Orion's behavior
    /// </summary>
    public void ConfigureOrionPersonality(float organization, float curiosity, float caution)
    {
        if (Windows31DesktopManager.Instance != null)
        {
            Windows31DesktopManager.Instance.organizationLevel = Mathf.Clamp01(organization);
            Windows31DesktopManager.Instance.curiosityLevel = Mathf.Clamp01(curiosity);
            Windows31DesktopManager.Instance.caution = Mathf.Clamp01(caution);
        }
        
        Debug.Log($"Orion personality configured - Organization: {organization:F2}, Curiosity: {curiosity:F2}, Caution: {caution:F2}");
    }

    /// <summary>
    /// Enable or disable various debug features
    /// </summary>
    public void ConfigureDebugFeatures(bool enableDebug, bool showCursorPath = false, bool logAllActions = false)
    {
        debugMode = enableDebug;
        
        if (SimulationController.Instance != null)
        {
            SimulationController.Instance.debugMode = enableDebug;
        }
        
        // Additional debug configuration could go here
        
        Debug.Log($"Debug features configured - Debug: {enableDebug}, Cursor Path: {showCursorPath}, Log Actions: {logAllActions}");
    }

    #endregion

    void OnGUI()
    {
        if (!debugMode || !isSetupComplete) return;
        
        // Debug GUI overlay
        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("Neural Cascade Debug Panel", "label");
        GUILayout.Space(10);
        
        // System status
        GUILayout.Label("System Status:", "label");
        if (SimulationController.Instance != null)
        {
            GUILayout.Label($"Mode: {SimulationController.Instance.currentMode}");
            GUILayout.Label($"Viewers: {SimulationController.Instance.GetViewerCount()}");
            GUILayout.Label($"Crisis: {SimulationController.Instance.IsInCrisisMode()}");
        }
        
        if (DialogueState.Instance != null)
        {
            GUILayout.Label($"Tension: {DialogueState.Instance.globalTension:F2}");
            GUILayout.Label($"Awareness: {DialogueState.Instance.metaAwareness:F2}");
            GUILayout.Label($"Overseer Warnings: {DialogueState.Instance.overseerWarnings}");
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
        
        GUILayout.Space(10);
        
        // Pacing controls
        GUILayout.Label("Quick Pacing:", "label");
        if (GUILayout.Button("Demo Pacing"))
            ConfigureExperiencePacing(ExperiencePacing.Demo);
        if (GUILayout.Button("Testing Pacing"))
            ConfigureExperiencePacing(ExperiencePacing.Testing);
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    public enum ExperiencePacing
    {
        Demo,       // Very fast for demonstrations
        Testing,    // Moderate for development testing  
        Production, // Normal for actual experience
        Extended    // Slow for long-form content
    }
}