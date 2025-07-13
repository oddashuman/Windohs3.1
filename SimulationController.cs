using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Updated SimulationController that manages the complete Windows 3.1 desktop experience
/// with proper phase transitions and authentic computer usage patterns.
/// </summary>
public class SimulationController : MonoBehaviour
{
    public static SimulationController Instance { get; private set; }

    public enum Mode 
    { 
        BootSequence,    // Windows 3.1 startup
        DesktopActivity, // File management, research, casual use
        Terminal,        // Neural cascade conversations
        Crisis,          // System instability
        Shutdown,        // Session ending
        Rain,            // Legacy rain mode (stub)
        SystemBoot       // Legacy boot mode (stub)
    }

    [Header("Experience Phases")]
    public Mode currentMode = Mode.BootSequence;
    
    [Header("Desktop Activity Settings")]
    public float minDesktopTime = 180f;      // 3 minutes minimum
    public float maxDesktopTime = 600f;      // 10 minutes maximum
    public float activityVariance = 0.3f;    // 30% variance in timing
    
    [Header("Terminal Session Settings")]
    public int minTerminalMessages = 15;     // Minimum conversation length
    public int maxTerminalMessages = 40;     // Maximum before transition
    public float conversationTimeout = 1800f; // 30 minutes max session
    
    [Header("Crisis Settings")]
    public float crisisModeDuration = 180f;  // 3 minutes of crisis
    public int crisisMessageBurst = 12;      // Rapid messages during crisis
    public float crisisRecoveryTime = 60f;   // Cooldown after crisis
    
    [Header("Viewer Engagement")]
    public bool enableViewerHooks = true;
    public float viewerHookInterval = 300f;  // Every 5 minutes
    public int massViewerThreshold = 20;     // Triggers special events
    
    [Header("Debug")]
    public bool debugMode = false;
    public bool skipBootSequence = false;
    public bool forceDesktopMode = false;
    public bool forceTerminalStart = false;

    // Core Managers - Added missing references
    [HideInInspector] public Windows31DesktopManager desktopManager;
    [HideInInspector] public MatrixTerminalManager terminalManager;
    [HideInInspector] public CursorController cursorController;
    [HideInInspector] public RetroCascadeManager cascadeManager;
    [HideInInspector] public MatrixRainTextWall rainManager;

    // Session State
    private float phaseStartTime;
    private float desktopActivityDuration;
    private int terminalMessageCount = 0;
    private bool isTransitioning = false;
    private SessionType currentSessionType = SessionType.Discovery;
    private float lastViewerHookTime = 0f;
    private int simulatedViewerCount = 0;
    private float sessionStartTime;
    private int totalSessionCount = 0;
    private bool inCrisisMode = false;
    private float crisisModeStartTime;

    // Initialization
    private bool isInitialized = false;
    private bool componentsReady = false;

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        sessionStartTime = Time.time;
        StartCoroutine(InitializeComponents());
    }

    IEnumerator InitializeComponents()
    {
        Debug.Log("SimulationController: Initializing Windows 3.1 experience");

        // Initialize cursor controller first
        if (CursorController.Instance == null)
        {
            var cursorGO = new GameObject("CursorController");
            cursorGO.transform.SetParent(transform);
            cursorController = cursorGO.AddComponent<CursorController>();
        }
        else
        {
            cursorController = CursorController.Instance;
        }

        // Initialize desktop manager
        if (Windows31DesktopManager.Instance == null)
        {
            var desktopGO = new GameObject("Windows31DesktopManager");
            desktopGO.transform.SetParent(transform);
            desktopManager = desktopGO.AddComponent<Windows31DesktopManager>();
            
            if (skipBootSequence)
                desktopManager.skipBootOnRestart = true;
        }
        else
        {
            desktopManager = Windows31DesktopManager.Instance;
        }

        // Initialize terminal manager
        if (MatrixTerminalManager.Instance == null)
        {
            var terminalGO = new GameObject("MatrixTerminalManager");
            terminalGO.transform.SetParent(transform);
            terminalManager = terminalGO.AddComponent<MatrixTerminalManager>();
        }
        else
        {
            terminalManager = MatrixTerminalManager.Instance;
        }

        // Initialize stub managers for backward compatibility
        if (FindObjectOfType<RetroCascadeManager>() == null)
        {
            var cascadeGO = new GameObject("RetroCascadeManager");
            cascadeGO.transform.SetParent(transform);
            cascadeManager = cascadeGO.AddComponent<RetroCascadeManager>();
        }
        else
        {
            cascadeManager = FindObjectOfType<RetroCascadeManager>();
        }

        if (FindObjectOfType<MatrixRainTextWall>() == null)
        {
            var rainGO = new GameObject("MatrixRainTextWall");
            rainGO.transform.SetParent(transform);
            rainManager = rainGO.AddComponent<MatrixRainTextWall>();
        }
        else
        {
            rainManager = FindObjectOfType<MatrixRainTextWall>();
        }

        // Wait for components to be ready
        float timeout = 15f;
        float startTime = Time.time;
        
        while (Time.time - startTime < timeout)
        {
            bool ready = true;
            
            if (desktopManager == null || !desktopManager.IsReady())
                ready = false;
            if (terminalManager == null || !terminalManager.IsReady())
                ready = false;
            if (cursorController == null)
                ready = false;
                
            if (ready)
            {
                componentsReady = true;
                break;
            }
            
            yield return new WaitForSeconds(0.2f);
        }

        if (!componentsReady)
        {
            Debug.LogWarning("SimulationController: Component initialization timeout, proceeding anyway");
            componentsReady = true;
        }

        isInitialized = true;
        Debug.Log("SimulationController: Component initialization complete");

        // Start the experience
        yield return new WaitForSeconds(0.5f);
        StartExperience();
    }

    void Update()
    {
        if (!isInitialized || !componentsReady) return;
        
        if (debugMode) HandleDebugInput();
        UpdateViewerSimulation();
        HandleViewerHooks();
        HandlePhaseTransitions();
        UpdateCrisisMode();
        UpdatePersonalityState();
    }

    #region Experience Management

    void StartExperience()
    {
        if (forceDesktopMode || (skipBootSequence && debugMode))
        {
            Debug.Log("SimulationController: Starting directly in desktop mode");
            StartDesktopPhase();
        }
        else if (forceTerminalStart)
        {
            Debug.Log("SimulationController: Starting directly in terminal mode");
            StartTerminalPhase();
        }
        else
        {
            Debug.Log("SimulationController: Starting with boot sequence");
            StartBootPhase();
        }
    }

    void StartBootPhase()
    {
        currentMode = Mode.BootSequence;
        phaseStartTime = Time.time;
        
        if (desktopManager != null)
        {
            // Boot sequence handled by desktop manager
            // Will automatically transition to desktop when complete
        }
        
        Debug.Log("SimulationController: Boot sequence initiated");
    }

    void StartDesktopPhase()
    {
        currentMode = Mode.DesktopActivity;
        phaseStartTime = Time.time;
        totalSessionCount++;
        
        // Calculate desktop activity duration
        desktopActivityDuration = Random.Range(minDesktopTime, maxDesktopTime);
        var state = DialogueState.Instance;
        if (state != null)
        {
            // Adjust duration based on current state
            if (state.globalTension > 0.7f)
                desktopActivityDuration *= 0.7f; // Shorter when tense
            if (state.metaAwareness > 0.6f)
                desktopActivityDuration *= 0.8f; // More eager to connect when aware
            if (state.overseerWarnings > 2)
                desktopActivityDuration *= 1.2f; // More cautious when watched
        }
        
        // Ensure desktop manager is active
        if (desktopManager != null)
        {
            desktopManager.ShowDesktop();
        }
        
        // Disable terminal during desktop phase
        if (terminalManager != null)
        {
            terminalManager.DisableTerminal();
        }

        // Disable visual effects during desktop phase
        if (cascadeManager != null)
        {
            cascadeManager.DisableCascade();
        }
        if (rainManager != null)
        {
            rainManager.DisableRain();
        }
        
        // Start cursor idle movement
        if (cursorController != null)
        {
            cursorController.StartIdleMovement();
        }
        
        Debug.Log($"SimulationController: Desktop phase started for {desktopActivityDuration:F1}s (Session #{totalSessionCount})");
    }

    void StartTerminalPhase()
    {
        currentMode = Mode.Terminal;
        phaseStartTime = Time.time;
        terminalMessageCount = 0;
        
        // Determine session type
        DetermineSessionType();
        
        // Launch terminal through desktop manager
        if (desktopManager != null)
        {
            desktopManager.TriggerConversationMode();
        }
        
        // Enable terminal
        if (terminalManager != null)
        {
            terminalManager.EnableTerminal();
        }
        
        // Disable visual effects during terminal phase
        if (cascadeManager != null)
        {
            cascadeManager.DisableCascade();
        }
        if (rainManager != null)
        {
            rainManager.DisableRain();
        }
        
        // Update cursor behavior for conversation mode
        if (cursorController != null)
        {
            cursorController.SetPrecisionMode(true);
            cursorController.StopIdleMovement();
        }
        
        Debug.Log($"SimulationController: Terminal phase started (Type: {currentSessionType})");
    }

    void StartCrisisMode()
    {
        inCrisisMode = true;
        currentMode = Mode.Crisis;
        crisisModeStartTime = Time.time;
        
        // Visual crisis effects
        if (terminalManager != null)
        {
            terminalManager.SetCrisisMode(true);
        }
        
        if (cascadeManager != null)
        {
            cascadeManager.TriggerCrisisVisuals();
        }
        
        if (rainManager != null)
        {
            rainManager.TriggerCrisisVisuals();
        }
        
        // Cursor becomes erratic
        if (cursorController != null)
        {
            cursorController.SetTensionLevel(1.0f);
            cursorController.AddFatigue(0.3f);
        }
        
        // Update dialogue state
        var state = DialogueState.Instance;
        if (state != null)
        {
            state.globalTension = 1.0f;
            state.AddGlitchEvent("system_crisis", "Crisis mode activated", 3.0f);
        }
        
        Debug.Log("SimulationController: Crisis mode activated");
    }

    void DetermineSessionType()
    {
        var state = DialogueState.Instance;
        if (state == null)
        {
            currentSessionType = SessionType.Discovery;
            return;
        }

        // Determine session type based on current state
        if (state.overseerWarnings > 3 || state.rareRedGlitchOccurred)
            currentSessionType = SessionType.Crisis;
        else if (state.metaAwareness > 0.8f)
            currentSessionType = SessionType.Revelation;
        else if (state.glitchCount > 4)
            currentSessionType = SessionType.Investigation;
        else if (state.systemIntegrityCompromised)
            currentSessionType = SessionType.Reset;
        else
            currentSessionType = SessionType.Discovery;
    }

    #endregion

    #region Phase Transitions

    void HandlePhaseTransitions()
    {
        if (isTransitioning) return;

        switch (currentMode)
        {
            case Mode.BootSequence:
            case Mode.SystemBoot:
                HandleBootTransition();
                break;
                
            case Mode.DesktopActivity:
                HandleDesktopTransition();
                break;
                
            case Mode.Terminal:
                HandleTerminalTransition();
                break;
                
            case Mode.Crisis:
                HandleCrisisTransition();
                break;
                
            case Mode.Rain:
                HandleRainTransition();
                break;
        }
    }

    void HandleBootTransition()
    {
        // Check if boot sequence is complete
        if (desktopManager != null && desktopManager.IsReady())
        {
            StartCoroutine(TransitionToDesktop());
        }
    }

    void HandleDesktopTransition()
    {
        float timeInPhase = Time.time - phaseStartTime;
        
        // Minimum time requirement
        if (timeInPhase < minDesktopTime) return;
        
        // Check for forced transition conditions
        bool shouldTransition = false;
        
        if (timeInPhase >= desktopActivityDuration)
        {
            shouldTransition = true;
        }
        
        // Check for external triggers
        var state = DialogueState.Instance;
        if (state != null)
        {
            if (state.overseerWarnings > 3 && Random.value < 0.3f)
                shouldTransition = true;
            if (state.rareRedGlitchOccurred && timeInPhase > minDesktopTime * 0.8f)
                shouldTransition = true;
            if (GetViewerCount() > massViewerThreshold && Random.value < 0.2f)
                shouldTransition = true;
        }
        
        if (shouldTransition)
        {
            StartCoroutine(TransitionToTerminal());
        }
    }

    void HandleTerminalTransition()
    {
        float timeInPhase = Time.time - phaseStartTime;
        
        // Check for transition conditions
        bool shouldTransition = false;
        
        if (terminalMessageCount >= maxTerminalMessages)
        {
            shouldTransition = true;
        }
        else if (timeInPhase >= conversationTimeout)
        {
            shouldTransition = true;
        }
        
        // Check for crisis conditions
        var state = DialogueState.Instance;
        if (state != null && !inCrisisMode)
        {
            if (state.globalTension > 0.9f || state.overseerWarnings > 4)
            {
                StartCoroutine(TransitionToCrisis());
                return;
            }
        }
        
        if (shouldTransition && terminalMessageCount >= minTerminalMessages)
        {
            StartCoroutine(TransitionToDesktop());
        }
    }

    void HandleCrisisTransition()
    {
        float timeInCrisis = Time.time - crisisModeStartTime;
        
        if (timeInCrisis >= crisisModeDuration)
        {
            StartCoroutine(EndCrisisMode());
        }
    }

    void HandleRainTransition()
    {
        // Legacy rain mode - transition back to terminal after short period
        float timeInPhase = Time.time - phaseStartTime;
        if (timeInPhase > 30f) // 30 seconds max in rain mode
        {
            StartCoroutine(TransitionToTerminal());
        }
    }

    IEnumerator TransitionToDesktop()
    {
        isTransitioning = true;
        
        Debug.Log("SimulationController: Transitioning to desktop phase");
        
        // Fade out current mode
        if (currentMode == Mode.Terminal && terminalManager != null)
        {
            yield return StartCoroutine(terminalManager.FadeOutTerminal(1f));
        }
        else if (currentMode == Mode.Rain && cascadeManager != null)
        {
            yield return StartCoroutine(cascadeManager.FadeOutCascade(1f));
        }
        else if (currentMode == Mode.Rain && rainManager != null)
        {
            yield return StartCoroutine(rainManager.FadeOutRain(1f));
        }
        
        yield return new WaitForSeconds(0.5f);
        
        StartDesktopPhase();
        
        isTransitioning = false;
    }

    IEnumerator TransitionToTerminal()
    {
        isTransitioning = true;
        
        Debug.Log("SimulationController: Transitioning to terminal phase");
        
        // Fade out current visual effects
        if (cascadeManager != null)
        {
            cascadeManager.DisableCascade();
        }
        if (rainManager != null)
        {
            rainManager.DisableRain();
        }
        
        // Cursor movement to terminal icon
        if (cursorController != null && desktopManager != null)
        {
            // Simulate Orion deciding to start conversation
            yield return new WaitForSeconds(Random.Range(1f, 3f));
        }
        
        StartTerminalPhase();
        
        yield return new WaitForSeconds(0.5f);
        
        isTransitioning = false;
    }

    IEnumerator TransitionToCrisis()
    {
        isTransitioning = true;
        
        Debug.Log("SimulationController: Transitioning to crisis mode");
        
        StartCrisisMode();
        
        yield return new WaitForSeconds(0.2f);
        
        isTransitioning = false;
    }

    IEnumerator EndCrisisMode()
    {
        Debug.Log("SimulationController: Ending crisis mode");
        
        inCrisisMode = false;
        
        // Reset visual effects
        if (terminalManager != null)
        {
            terminalManager.SetCrisisMode(false);
        }
        
        if (cascadeManager != null)
        {
            cascadeManager.DisableCascade();
        }
        
        if (rainManager != null)
        {
            rainManager.DisableRain();
        }
        
        // Restore normal cursor behavior
        if (cursorController != null)
        {
            cursorController.SetTensionLevel(0.5f);
            cursorController.ResetFatigue();
        }
        
        // Decide next phase based on severity
        var state = DialogueState.Instance;
        if (state != null && state.systemIntegrityCompromised)
        {
            // Force session reset
            yield return new WaitForSeconds(crisisRecoveryTime);
            ForceSessionReset();
        }
        else
        {
            // Return to appropriate mode
            if (terminalMessageCount < minTerminalMessages)
            {
                currentMode = Mode.Terminal;
            }
            else
            {
                StartCoroutine(TransitionToDesktop());
            }
        }
    }

    #endregion

    #region Crisis Mode Management

    void UpdateCrisisMode()
    {
        if (!inCrisisMode) return;
        
        var state = DialogueState.Instance;
        if (state == null) return;
        
        // Escalate crisis effects over time
        float crisisProgress = (Time.time - crisisModeStartTime) / crisisModeDuration;
        
        if (crisisProgress > 0.5f && Random.value < 0.01f)
        {
            // Trigger additional glitches
            state.AddGlitchEvent("crisis_escalation", "System instability increasing", 2.5f);
        }
        
        if (crisisProgress > 0.8f && Random.value < 0.005f)
        {
            // Terminal effects
            if (terminalManager != null)
            {
                terminalManager.QueueSystemMessage("CRITICAL ERROR: REALITY MATRIX DESTABILIZING", "EMERGENCY", "SYSTEM");
            }
        }
    }

    public void TriggerCrisisMode()
    {
        if (!inCrisisMode)
        {
            StartCoroutine(TransitionToCrisis());
        }
    }

    #endregion

    #region Viewer Management

    void UpdateViewerSimulation()
    {
        // Simulate realistic viewer fluctuation
        if (Random.value < 0.001f) // Very occasional changes
        {
            int change = Random.Range(-2, 4);
            simulatedViewerCount = Mathf.Clamp(simulatedViewerCount + change, 1, 50);
        }
        
        // Higher viewer counts during interesting phases
        int targetViewers = 5;
        switch (currentMode)
        {
            case Mode.Terminal:
                targetViewers = 15;
                break;
            case Mode.Crisis:
                targetViewers = 25;
                break;
            case Mode.DesktopActivity:
                targetViewers = 8;
                break;
            case Mode.Rain:
                targetViewers = 12;
                break;
        }
        
        // Gradually adjust toward target
        if (simulatedViewerCount < targetViewers && Random.value < 0.01f)
        {
            simulatedViewerCount++;
        }
        else if (simulatedViewerCount > targetViewers && Random.value < 0.01f)
        {
            simulatedViewerCount--;
        }
    }

    void HandleViewerHooks()
    {
        if (!enableViewerHooks) return;
        if (Time.time - lastViewerHookTime < viewerHookInterval) return;
        
        lastViewerHookTime = Time.time;
        
        // Trigger viewer-related events
        if (simulatedViewerCount > massViewerThreshold)
        {
            TriggerMassViewerEvent();
        }
        else if (Random.value < 0.3f)
        {
            TriggerStandardViewerEvent();
        }
    }

    void TriggerMassViewerEvent()
    {
        var state = DialogueState.Instance;
        if (state != null)
        {
            state.observerDetected = true;
            state.metaAwareness += 0.1f;
            state.RegisterUser($"MassObserver_{simulatedViewerCount}");
        }
        
        if (cascadeManager != null)
        {
            cascadeManager.ShowViewerAcknowledgment(simulatedViewerCount);
        }
        else if (rainManager != null)
        {
            rainManager.ShowViewerAcknowledgment(simulatedViewerCount);
        }
        
        Debug.Log($"SimulationController: Mass viewer event triggered ({simulatedViewerCount} viewers)");
    }

    void TriggerStandardViewerEvent()
    {
        var state = DialogueState.Instance;
        if (state != null)
        {
            state.observerDetected = true;
            state.metaAwareness += 0.02f;
        }
        
        Debug.Log("SimulationController: Standard viewer event triggered");
    }

    #endregion

    #region Public API

    public void IncrementTerminalMessageCount()
    {
        terminalMessageCount++;
        Debug.Log($"SimulationController: Terminal message count: {terminalMessageCount}/{maxTerminalMessages}");
    }

    public bool IsInCrisisMode() => inCrisisMode;

    public SessionType GetCurrentSessionType() => currentSessionType;

    public int GetViewerCount() => simulatedViewerCount;

    public void ForceSessionReset()
    {
        Debug.Log("SimulationController: Forcing session reset");
        
        DialogueState.Instance?.Reset();
        StopAllCoroutines();
        
        terminalMessageCount = 0;
        totalSessionCount = 0;
        currentSessionType = SessionType.Discovery;
        inCrisisMode = false;
        
        // Reset all managers
        if (terminalManager != null)
        {
            terminalManager.DisableTerminal();
        }
        
        if (cascadeManager != null)
        {
            cascadeManager.DisableCascade();
        }
        
        if (rainManager != null)
        {
            rainManager.DisableRain();
        }
        
        if (cursorController != null)
        {
            cursorController.ResetFatigue();
            cursorController.SetTensionLevel(0.3f);
        }
        
        // Restart experience
        if (Random.value < 0.3f)
        {
            StartBootPhase();
        }
        else
        {
            StartDesktopPhase();
        }
    }

    public bool IsReady() => isInitialized && componentsReady;

    public string GetDebugInfo()
    {
        return $"Mode: {currentMode}, Session: {currentSessionType}, " +
               $"Desktop Time: {(Time.time - phaseStartTime):F1}s/{desktopActivityDuration:F1}s, " +
               $"Terminal Messages: {terminalMessageCount}/{maxTerminalMessages}, " +
               $"Viewers: {simulatedViewerCount}, Crisis: {inCrisisMode}, " +
               $"Initialized: {isInitialized}, Ready: {componentsReady}";
    }

    #endregion

    #region Personality State Updates

    void UpdatePersonalityState()
    {
        if (cursorController == null) return;
        
        var state = DialogueState.Instance;
        if (state == null) return;
        
        // Update cursor behavior based on current state
        float tension = state.globalTension;
        float awareness = state.metaAwareness;
        
        cursorController.SetTensionLevel(tension);
        
        // Adjust cursor behavior based on current mode
        switch (currentMode)
        {
            case Mode.DesktopActivity:
                cursorController.SetPrecisionMode(false);
                if (state.overseerWarnings > 2)
                {
                    cursorController.SetConfidenceLevel(0.6f); // More cautious
                }
                break;
                
            case Mode.Terminal:
                cursorController.SetPrecisionMode(true);
                if (awareness > 0.7f)
                {
                    cursorController.AddFatigue(0.001f); // Gradual mental fatigue
                }
                break;
                
            case Mode.Crisis:
                cursorController.SetTensionLevel(1.0f);
                cursorController.SetConfidenceLevel(0.3f);
                break;
        }
    }

    #endregion

    #region Debug Controls

    void HandleDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ForcePhaseTransition();
        }
        
        if (Input.GetKeyDown(KeyCode.F6))
        {
            TriggerCrisisMode();
        }
        
        if (Input.GetKeyDown(KeyCode.F7))
        {
            ForceSessionReset();
        }
        
        if (Input.GetKeyDown(KeyCode.F8))
        {
            simulatedViewerCount += 10;
            TriggerMassViewerEvent();
        }
    }

    void ForcePhaseTransition()
    {
        Debug.Log("SimulationController: Forcing phase transition");
        
        switch (currentMode)
        {
            case Mode.BootSequence:
            case Mode.SystemBoot:
                StartDesktopPhase();
                break;
            case Mode.DesktopActivity:
                StartCoroutine(TransitionToTerminal());
                break;
            case Mode.Terminal:
                StartCoroutine(TransitionToDesktop());
                break;
            case Mode.Crisis:
                StartCoroutine(EndCrisisMode());
                break;
            case Mode.Rain:
                StartCoroutine(TransitionToTerminal());
                break;
        }
    }

    #endregion

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}