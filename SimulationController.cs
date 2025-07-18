using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Production-grade simulation controller with advanced memory management,
/// performance monitoring, and 24/7 stability systems.
/// </summary>
public class SimulationController : MonoBehaviour
{
    public static SimulationController Instance { get; private set; }

    public enum Mode { BootSequence, DesktopActivity, Terminal, PostConversation, Screensaver, Shutdown, Crisis }

    [Header("State Management")]
    public Mode currentMode = Mode.BootSequence;
    private float phaseStartTime;
    private float sessionStartTime;

    [Header("Timing Configuration")]
    public float minDesktopTime = 180f;
    public float maxDesktopTime = 600f;
    public int minTerminalMessages = 15;
    public int maxTerminalMessages = 40;
    public float crisisModeDuration = 180f;
    public float screensaverIdleTime = 120f;

    [Header("Performance & Stability")]
    [Range(30, 120)] public float memoryCleanupInterval = 60f;
    [Range(5, 30)] public float performanceCheckInterval = 10f;
    [Range(10, 60)] public float healthCheckInterval = 30f;
    public bool enableAutoOptimization = true;
    public bool enablePerformanceScaling = true;

    [Header("Debug Configuration")]
    public bool debugMode = false;
    public bool skipBootSequence = false;
    public bool enableDetailedLogging = false;

    // Core system references
    private Windows31DesktopManager desktopManager;
    private MatrixTerminalManager terminalManager;
    private DesktopAI desktopAI;

    // Session state tracking
    private int terminalMessageCount;
    private bool isTransitioning = false;
    private bool componentsReady = false;
    private float lastActivityTime;
    private int currentSessionCycle = 1;

    // Performance monitoring
    private struct PerformanceMetrics
    {
        public float frameRate;
        public long memoryUsage;
        public int activeCoroutines;
        public float cpuTime;
        public bool isStable;
    }
    
    private PerformanceMetrics currentMetrics;
    private Queue<float> frameTimeHistory = new Queue<float>();
    private const int FRAME_HISTORY_SIZE = 300; // 5 seconds at 60fps
    
    // Memory management
    private float lastMemoryCleanup;
    private float lastPerformanceCheck;
    private float lastHealthCheck;
    private List<System.WeakReference> trackedCoroutines = new List<System.WeakReference>();
    
    // Stability systems
    private int consecutiveSlowFrames = 0;
    private int memoryPressureLevel = 0; // 0=normal, 1=elevated, 2=high, 3=critical
    private bool isRecovering = false;
    
    // Error recovery
    private Dictionary<string, int> errorCounts = new Dictionary<string, int>();
    private float lastErrorTime;
    private const int MAX_ERRORS_PER_MINUTE = 10;

    #region Initialization & Setup

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        
        sessionStartTime = Time.time;
        
        // Configure for continuous operation
        Application.runInBackground = true;
        QualitySettings.vSyncCount = 1; // Enable VSync for stable 60fps
        Application.targetFrameRate = 60;
        
        // Prevent device sleep
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        InitializePerformanceMonitoring();
    }

    void Start()
    {
        StartCoroutine(InitializeAndBegin());
    }

    public IEnumerator InitializeAndBegin()
    {
        Debug.Log("SIMCTRL: Enhanced initialization starting...");
        
        // Wait for all critical components
        yield return new WaitUntil(() =>
            Windows31DesktopManager.Instance != null && Windows31DesktopManager.Instance.IsReady() &&
            MatrixTerminalManager.Instance != null && MatrixTerminalManager.Instance.IsReady() &&
            DesktopAI.Instance != null
        );

        // Link component references
        desktopManager = Windows31DesktopManager.Instance;
        terminalManager = MatrixTerminalManager.Instance;
        desktopAI = DesktopAI.Instance;

        componentsReady = true;
        
        // Start monitoring systems
        StartCoroutine(PerformanceMonitoringLoop());
        StartCoroutine(MemoryManagementLoop());
        StartCoroutine(HealthCheckLoop());
        
        Debug.Log("SIMCTRL: All systems ready - Beginning experience flow");
        EnterState(skipBootSequence ? Mode.DesktopActivity : Mode.BootSequence);
    }

    private void InitializePerformanceMonitoring()
    {
        // Initialize frame time tracking
        for (int i = 0; i < FRAME_HISTORY_SIZE; i++)
        {
            frameTimeHistory.Enqueue(16.67f); // Start with 60fps assumption
        }
    }

    #endregion

    #region State Management - Enhanced

    void Update()
    {
        if (!componentsReady) return;
        
        UpdatePerformanceMetrics();
        HandleAutomaticTransitions();
        ProcessDebugInput();
    }

    private void HandleAutomaticTransitions()
    {
        if (isTransitioning) return;
        
        float timeInPhase = Time.time - phaseStartTime;
        
        switch (currentMode)
        {
            case Mode.DesktopActivity:
                if (timeInPhase > maxDesktopTime)
                {
                    StartCoroutine(TransitionTo(Mode.Terminal));
                }
                else if (TimeSinceLastActivity() > screensaverIdleTime)
                {
                    StartCoroutine(TransitionTo(Mode.Screensaver));
                }
                break;
                
            case Mode.PostConversation:
                if (timeInPhase > 300f || TimeSinceLastActivity() > screensaverIdleTime)
                {
                    StartCoroutine(TransitionTo(Mode.Screensaver));
                }
                break;
                
            case Mode.Terminal:
                if (terminalMessageCount >= maxTerminalMessages)
                {
                    StartCoroutine(TransitionTo(Mode.PostConversation));
                }
                else if (timeInPhase > 1800f) // 30 minute safety timeout
                {
                    LogWarning("Terminal phase timeout - forcing transition");
                    StartCoroutine(TransitionTo(Mode.PostConversation));
                }
                break;
                
            case Mode.Crisis:
                if (timeInPhase > crisisModeDuration)
                {
                    StartCoroutine(TransitionTo(Mode.DesktopActivity));
                }
                break;
                
            case Mode.Screensaver:
                // Screensaver ends when activity is reported
                break;
        }
        
        // Emergency recovery - if stuck in any state too long
        if (timeInPhase > 3600f) // 1 hour max per phase
        {
            LogError($"Phase {currentMode} exceeded maximum duration - forcing reset");
            StartCoroutine(TransitionTo(Mode.Shutdown));
        }
    }

    private IEnumerator TransitionTo(Mode nextState)
    {
        if (isTransitioning) yield break;
        
        isTransitioning = true;
        LogInfo($"Transitioning from {currentMode} to {nextState}");
        
        // Pre-transition cleanup
        yield return StartCoroutine(PreTransitionCleanup());
        
        // Transition delay with memory cleanup
        yield return new WaitForSeconds(1.0f);
        
        EnterState(nextState);
        isTransitioning = false;
    }

    private IEnumerator PreTransitionCleanup()
    {
        // Clean up current state
        CleanupCurrentState();
        
        // Force memory cleanup during transition
        if (enableAutoOptimization)
        {
            PerformMemoryCleanup();
            yield return null; // Give GC a frame to work
        }
        
        // Reset performance counters
        consecutiveSlowFrames = 0;
    }

    private void EnterState(Mode newState)
    {
        Mode previousState = currentMode;
        currentMode = newState;
        phaseStartTime = Time.time;
        lastActivityTime = Time.time;
        
        LogInfo($"Entered state: {newState}");
        
        switch (newState)
        {
            case Mode.BootSequence:
                EnterBootSequence();
                break;
            case Mode.DesktopActivity:
                EnterDesktopActivity();
                break;
            case Mode.Terminal:
                EnterTerminalMode();
                break;
            case Mode.PostConversation:
                EnterPostConversationMode();
                break;
            case Mode.Screensaver:
                EnterScreensaverMode();
                break;
            case Mode.Shutdown:
                EnterShutdownMode();
                break;
            case Mode.Crisis:
                EnterCrisisMode();
                break;
        }
        
        // Update narrative state
        if (DialogueState.Instance != null)
        {
            DialogueState.Instance.AddToNarrativeHistory("StateTransition", $"{previousState} -> {newState}", "SimulationController");
        }
    }

    #endregion

    #region State Implementations

    private void EnterBootSequence()
    {
        terminalMessageCount = 0;
        if (terminalManager != null) terminalManager.DisableTerminal();
        if (desktopManager != null)
        {
            desktopManager.SetScreensaver(false);
            desktopManager.StartBootSequence();
        }
    }

    private void EnterDesktopActivity()
    {
        terminalMessageCount = 0;
        if (terminalManager != null) terminalManager.DisableTerminal();
        if (desktopManager != null) desktopManager.SetScreensaver(false);
        
        StartCoroutine(ManageDesktopActivity());
    }

    private void EnterTerminalMode()
    {
        terminalMessageCount = 0;
        if (desktopManager != null)
        {
            desktopManager.SetScreensaver(false);
            desktopManager.LaunchProgram(Windows31DesktopManager.ProgramType.Terminal);
        }
    }

    private void EnterPostConversationMode()
    {
        if (terminalManager != null) terminalManager.DisableTerminal();
        if (desktopManager != null) desktopManager.SetScreensaver(false);
        
        StartCoroutine(ManagePostConversationActivity());
    }

    private void EnterScreensaverMode()
    {
        if (desktopManager != null) desktopManager.SetScreensaver(true);
        
        // Reduce performance overhead during screensaver
        if (enablePerformanceScaling)
        {
            Application.targetFrameRate = 30; // Lower framerate during screensaver
        }
    }

    private void EnterShutdownMode()
    {
        StartCoroutine(HandleShutdownSequence());
    }

    private void EnterCrisisMode()
    {
        if (NarrativeTriggerManager.Instance != null)
            NarrativeTriggerManager.Instance.TriggerEvent("CrisisStart", "SimulationController");
        if (terminalManager != null)
            terminalManager.SetCrisisMode(true);
            
        // Increase system tension
        if (DialogueState.Instance != null)
        {
            DialogueState.Instance.globalTension = 1.0f;
            DialogueState.Instance.systemIntegrityCompromised = true;
        }
    }

    #endregion

    #region Activity Management

    private IEnumerator ManageDesktopActivity()
    {
        yield return new WaitForSeconds(Random.Range(1f, 3f));
        
        // Sequence of desktop activities
        if (desktopAI != null && !desktopAI.isBusy)
        {
            desktopAI.PerformActivity(Windows31DesktopManager.DesktopActivity.OrganizingFiles);
            yield return new WaitUntil(() => !desktopAI.isBusy);
            ReportActivity();
        }
        
        yield return new WaitForSeconds(Random.Range(10f, 20f));
        
        if (desktopAI != null && !desktopAI.isBusy && currentMode == Mode.DesktopActivity)
        {
            desktopAI.PerformActivity(Windows31DesktopManager.DesktopActivity.ReviewingNotes);
            yield return new WaitUntil(() => !desktopAI.isBusy);
            ReportActivity();
        }
        
        // Check if we should transition to terminal
        float timeInPhase = Time.time - phaseStartTime;
        if (timeInPhase > minDesktopTime && currentMode == Mode.DesktopActivity)
        {
            LogInfo("Desktop activity complete - ready for terminal");
            StartCoroutine(TransitionTo(Mode.Terminal));
        }
    }

    private IEnumerator ManagePostConversationActivity()
    {
        yield return new WaitForSeconds(Random.Range(2f, 5f));
        
        if (desktopAI != null && !desktopAI.isBusy)
        {
            desktopAI.PerformActivity(Windows31DesktopManager.DesktopActivity.Playing);
            yield return new WaitUntil(() => !desktopAI.isBusy);
            ReportActivity();
        }
        
        // After some time, prepare for shutdown
        yield return new WaitForSeconds(Random.Range(60f, 120f));
        
        if (currentMode == Mode.PostConversation)
        {
            LogInfo("Post-conversation activities complete - preparing for shutdown");
            StartCoroutine(TransitionTo(Mode.Shutdown));
        }
    }

    private IEnumerator HandleShutdownSequence()
    {
        LogInfo($"Completing session cycle {currentSessionCycle}");
        
        // Cleanup before reset
        yield return StartCoroutine(PerformFullSystemCleanup());
        
        yield return new WaitForSeconds(Random.Range(3f, 8f));
        
        // Increment cycle counter
        currentSessionCycle++;
        
        // Reset the entire experience
        ForceSessionReset();
    }

    #endregion

    #region Performance Monitoring

    private void UpdatePerformanceMetrics()
    {
        // Track frame times
        float frameTime = Time.unscaledDeltaTime * 1000f; // Convert to milliseconds
        
        frameTimeHistory.Enqueue(frameTime);
        if (frameTimeHistory.Count > FRAME_HISTORY_SIZE)
            frameTimeHistory.Dequeue();
        
        // Calculate average frame rate
        float totalTime = 0f;
        foreach (float time in frameTimeHistory)
            totalTime += time;
        
        currentMetrics.frameRate = 1000f / (totalTime / frameTimeHistory.Count);
        currentMetrics.memoryUsage = System.GC.GetTotalMemory(false);
        
        // Detect performance issues
        if (frameTime > 20f) // Slower than 50fps
        {
            consecutiveSlowFrames++;
            if (consecutiveSlowFrames > 180) // 3 seconds of slow frames
            {
                HandlePerformanceDegradation();
            }
        }
        else
        {
            consecutiveSlowFrames = 0;
        }
    }

    private IEnumerator PerformanceMonitoringLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(performanceCheckInterval);
            
            if (Time.time - lastPerformanceCheck > performanceCheckInterval)
            {
                PerformPerformanceCheck();
                lastPerformanceCheck = Time.time;
            }
        }
    }

    private void PerformPerformanceCheck()
    {
        bool wasStable = currentMetrics.isStable;
        currentMetrics.isStable = currentMetrics.frameRate > 45f && currentMetrics.memoryUsage < 1073741824; // 1GB
        
        if (!currentMetrics.isStable && wasStable)
        {
            LogWarning($"Performance degradation detected - FPS: {currentMetrics.frameRate:F1}, Memory: {currentMetrics.memoryUsage / 1024 / 1024}MB");
        }
        else if (currentMetrics.isStable && !wasStable)
        {
            LogInfo("Performance stabilized");
        }
        
        // Update memory pressure level
        UpdateMemoryPressureLevel();
        
        if (enableDetailedLogging && debugMode)
        {
            LogInfo($"Performance: {currentMetrics.frameRate:F1}fps, {currentMetrics.memoryUsage / 1024 / 1024}MB, Pressure: {memoryPressureLevel}");
        }
    }

    private void HandlePerformanceDegradation()
    {
        LogWarning("Handling performance degradation");
        
        if (enableAutoOptimization)
        {
            // Emergency optimization measures
            PerformMemoryCleanup();
            
            // Reduce quality temporarily
            if (enablePerformanceScaling)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 45;
            }
            
            // Start recovery process
            if (!isRecovering)
            {
                StartCoroutine(PerformanceRecoverySequence());
            }
        }
        
        consecutiveSlowFrames = 0;
    }

    private IEnumerator PerformanceRecoverySequence()
    {
        isRecovering = true;
        LogInfo("Starting performance recovery sequence");
        
        // Wait for system to stabilize
        yield return new WaitForSeconds(10f);
        
        // Restore normal settings if performance improved
        if (currentMetrics.frameRate > 50f)
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 60;
            LogInfo("Performance recovered - settings restored");
        }
        
        isRecovering = false;
    }

    #endregion

    #region Memory Management

    private IEnumerator MemoryManagementLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(memoryCleanupInterval);
            
            if (Time.time - lastMemoryCleanup > memoryCleanupInterval)
            {
                PerformMemoryCleanup();
                lastMemoryCleanup = Time.time;
            }
        }
    }

    private void PerformMemoryCleanup()
    {
        // Clean up tracked coroutines
        CleanupTrackedCoroutines();
        
        // Force garbage collection during safe periods
        if (ShouldPerformGC())
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
        }
        
        // Clean up Unity resources
        Resources.UnloadUnusedAssets();
        
        LogInfo($"Memory cleanup completed - Usage: {System.GC.GetTotalMemory(false) / 1024 / 1024}MB");
    }

    private bool ShouldPerformGC()
    {
        // Only force GC during safe periods
        return !isTransitioning && 
               (currentMode == Mode.Screensaver || 
                currentMode == Mode.DesktopActivity && (desktopAI == null || !desktopAI.isBusy));
    }

    private void UpdateMemoryPressureLevel()
    {
        long memoryMB = currentMetrics.memoryUsage / 1024 / 1024;
        
        if (memoryMB > 1500) // 1.5GB
            memoryPressureLevel = 3; // Critical
        else if (memoryMB > 1000) // 1GB
            memoryPressureLevel = 2; // High
        else if (memoryMB > 700) // 700MB
            memoryPressureLevel = 1; // Elevated
        else
            memoryPressureLevel = 0; // Normal
            
        if (memoryPressureLevel >= 2 && enableAutoOptimization)
        {
            // Emergency memory cleanup
            PerformMemoryCleanup();
        }
    }

    private void CleanupTrackedCoroutines()
    {
        for (int i = trackedCoroutines.Count - 1; i >= 0; i--)
        {
            if (!trackedCoroutines[i].IsAlive)
            {
                trackedCoroutines.RemoveAt(i);
            }
        }
    }

    private IEnumerator PerformFullSystemCleanup()
    {
        LogInfo("Performing full system cleanup");
        
        // Stop all non-essential coroutines
        CleanupCurrentState();
        
        // Clean up dialogue state
        if (DialogueState.Instance != null)
        {
            DialogueState.Instance.Reset();
        }
        
        yield return null;
        
        // Force aggressive cleanup
        PerformMemoryCleanup();
        
        yield return new WaitForSeconds(1f);
        
        LogInfo("System cleanup completed");
    }

    #endregion

    #region Health Monitoring

    private IEnumerator HealthCheckLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(healthCheckInterval);
            
            if (Time.time - lastHealthCheck > healthCheckInterval)
            {
                PerformHealthCheck();
                lastHealthCheck = Time.time;
            }
        }
    }

    private void PerformHealthCheck()
    {
        bool systemHealthy = true;
        StringBuilder healthReport = new StringBuilder();
        
        // Check component health
        if (desktopManager == null || !desktopManager.IsReady())
        {
            systemHealthy = false;
            healthReport.AppendLine("Desktop Manager: FAILED");
        }
        
        if (terminalManager == null || !terminalManager.IsReady())
        {
            systemHealthy = false;
            healthReport.AppendLine("Terminal Manager: FAILED");
        }
        
        // Check performance metrics
        if (currentMetrics.frameRate < 30f)
        {
            systemHealthy = false;
            healthReport.AppendLine($"Frame Rate: POOR ({currentMetrics.frameRate:F1}fps)");
        }
        
        if (memoryPressureLevel >= 3)
        {
            systemHealthy = false;
            healthReport.AppendLine($"Memory: CRITICAL ({currentMetrics.memoryUsage / 1024 / 1024}MB)");
        }
        
        // Check for excessive errors
        if (GetRecentErrorCount() > MAX_ERRORS_PER_MINUTE)
        {
            systemHealthy = false;
            healthReport.AppendLine("Error Rate: TOO HIGH");
        }
        
        if (!systemHealthy)
        {
            LogWarning($"Health check failed:\n{healthReport}");
            
            // Attempt recovery
            if (enableAutoOptimization)
            {
                StartCoroutine(SystemRecoverySequence());
            }
        }
        else if (debugMode && enableDetailedLogging)
        {
            LogInfo("Health check: All systems normal");
        }
    }

    private IEnumerator SystemRecoverySequence()
    {
        LogWarning("Starting system recovery sequence");
        
        // Force immediate cleanup
        yield return StartCoroutine(PerformFullSystemCleanup());
        
        // Wait for stabilization
        yield return new WaitForSeconds(5f);
        
        // If still unhealthy, force reset
        if (currentMetrics.frameRate < 20f || memoryPressureLevel >= 3)
        {
            LogError("Recovery failed - forcing session reset");
            ForceSessionReset();
        }
        else
        {
            LogInfo("System recovery successful");
        }
    }

    #endregion

    #region Error Handling

    private void LogError(string message)
    {
        Debug.LogError($"[SIMCTRL ERROR] {message}");
        TrackError("ERROR");
    }

    private void LogWarning(string message)
    {
        Debug.LogWarning($"[SIMCTRL WARNING] {message}");
        TrackError("WARNING");
    }

    private void LogInfo(string message)
    {
        if (enableDetailedLogging || debugMode)
        {
            Debug.Log($"[SIMCTRL] {message}");
        }
    }

    private void TrackError(string errorType)
    {
        if (!errorCounts.ContainsKey(errorType))
            errorCounts[errorType] = 0;
            
        errorCounts[errorType]++;
        lastErrorTime = Time.time;
    }

    private int GetRecentErrorCount()
    {
        if (Time.time - lastErrorTime > 60f)
        {
            errorCounts.Clear();
            return 0;
        }
        
        int totalErrors = 0;
        foreach (var count in errorCounts.Values)
            totalErrors += count;
            
        return totalErrors;
    }

    #endregion

    #region Public Interface

    public void OnBootComplete()
    {
        if (currentMode == Mode.BootSequence)
        {
            StartCoroutine(TransitionTo(Mode.DesktopActivity));
        }
        
        // Restore normal framerate after boot
        if (enablePerformanceScaling)
        {
            Application.targetFrameRate = 60;
        }
    }

    public void ReportActivity()
    {
        lastActivityTime = Time.time;
        
        // Exit screensaver mode if active
        if (currentMode == Mode.Screensaver)
        {
            StartCoroutine(TransitionTo(Mode.DesktopActivity));
        }
    }

    public void IncrementTerminalMessageCount()
    {
        terminalMessageCount++;
        ReportActivity();
        
        LogInfo($"Terminal message count: {terminalMessageCount}/{maxTerminalMessages}");
    }

    public void TriggerCrisisMode()
    {
        if (currentMode != Mode.Crisis)
        {
            LogWarning("Crisis mode triggered!");
            StartCoroutine(TransitionTo(Mode.Crisis));
        }
    }

    public void ForceSessionReset()
    {
        LogInfo($"Forcing session reset - Cycle {currentSessionCycle} -> {currentSessionCycle + 1}");
        
        StopAllCoroutines();
        
        // Clean up state
        if (DialogueState.Instance != null)
            DialogueState.Instance.Reset();
            
        // Reset counters
        currentSessionCycle++;
        componentsReady = false;
        
        // Reload scene for fresh start
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    public bool IsReady() => componentsReady;

    public string GetDebugInfo()
    {
        float sessionTime = Time.time - sessionStartTime;
        float phaseTime = Time.time - phaseStartTime;
        
        return $"Mode: {currentMode}, " +
               $"Phase Time: {phaseTime:F1}s, " +
               $"Session Time: {sessionTime:F1}s, " +
               $"Cycle: {currentSessionCycle}, " +
               $"Messages: {terminalMessageCount}/{maxTerminalMessages}, " +
               $"FPS: {currentMetrics.frameRate:F1}, " +
               $"Memory: {currentMetrics.memoryUsage / 1024 / 1024}MB, " +
               $"Pressure: {memoryPressureLevel}, " +
               $"Errors: {GetRecentErrorCount()}";
    }

    #endregion

    #region Utility Methods

    private float TimeSinceLastActivity()
    {
        return Time.time - lastActivityTime;
    }

    private void CleanupCurrentState()
    {
        // Stop current state-specific coroutines
        switch (currentMode)
        {
            case Mode.DesktopActivity:
                // Desktop activities clean themselves up
                break;
                
            case Mode.Terminal:
                if (terminalManager != null)
                    terminalManager.DisableTerminal();
                break;
                
            case Mode.Screensaver:
                if (desktopManager != null)
                    desktopManager.SetScreensaver(false);
                break;
                
            case Mode.Crisis:
                if (terminalManager != null)
                    terminalManager.SetCrisisMode(false);
                break;
        }
    }

    private void ProcessDebugInput()
    {
        if (!debugMode) return;
        
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("=== SYSTEM STATUS ===");
            Debug.Log(GetDebugInfo());
            LogSystemComponentStatus();
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log("Forcing desktop mode");
            StartCoroutine(TransitionTo(Mode.DesktopActivity));
        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Debug.Log("Forcing terminal mode");
            StartCoroutine(TransitionTo(Mode.Terminal));
        }
        
        if (Input.GetKeyDown(KeyCode.F4))
        {
            Debug.Log("Triggering crisis mode");
            TriggerCrisisMode();
        }
        
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("Performing manual memory cleanup");
            PerformMemoryCleanup();
        }
        
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Debug.Log("Forcing session reset");
            ForceSessionReset();
        }
    }

    private void LogSystemComponentStatus()
    {
        Debug.Log($"Desktop Manager: {(desktopManager?.IsReady() == true ? "READY" : "NOT READY")}");
        Debug.Log($"Terminal Manager: {(terminalManager?.IsReady() == true ? "READY" : "NOT READY")}");
        Debug.Log($"Desktop AI: {(desktopAI != null ? (desktopAI.isBusy ? "BUSY" : "IDLE") : "NULL")}");
        Debug.Log($"Dialogue State: {(DialogueState.Instance != null ? "READY" : "NULL")}");
        
        if (DialogueState.Instance != null)
        {
            var state = DialogueState.Instance;
            Debug.Log($"Narrative State - Tension: {state.globalTension:F2}, " +
                     $"Awareness: {state.metaAwareness:F2}, " +
                     $"Glitches: {state.glitchCount}, " +
                     $"Loop: {state.loopCount}");
        }
    }

    #endregion

    #region Cleanup

    void OnDestroy()
    {
        StopAllCoroutines();
        
        // Clear collections
        frameTimeHistory?.Clear();
        trackedCoroutines?.Clear();
        errorCounts?.Clear();
        
        Debug.Log($"SIMCTRL: Session ended after {Time.time - sessionStartTime:F1} seconds, {currentSessionCycle} cycles");
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            LogInfo("Application paused - reducing performance");
            if (enablePerformanceScaling)
            {
                Application.targetFrameRate = 15; // Very low framerate when paused
            }
        }
        else
        {
            LogInfo("Application resumed - restoring performance");
            if (enablePerformanceScaling)
            {
                Application.targetFrameRate = currentMode == Mode.Screensaver ? 30 : 60;
            }
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            LogInfo("Application lost focus - optimizing for background");
            if (enablePerformanceScaling)
            {
                Application.targetFrameRate = 30;
            }
        }
        else
        {
            LogInfo("Application gained focus - restoring full performance");
            if (enablePerformanceScaling)
            {
                Application.targetFrameRate = currentMode == Mode.Screensaver ? 30 : 60;
            }
        }
    }

    #endregion
}