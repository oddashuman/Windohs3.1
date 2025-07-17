using UnityEngine;
using System.Collections;

/// <summary>
/// Acts as the "director" of the experience, managing the high-level
/// state transitions.
/// </summary>
public class SimulationController : MonoBehaviour
{
    public static SimulationController Instance { get; private set; }

    public enum Mode { BootSequence, DesktopActivity, Terminal, Crisis, Shutdown }

    [Header("State")]
    public Mode currentMode = Mode.BootSequence;
    private float phaseStartTime;

    [Header("Timings")]
    public float minDesktopTime = 180f;
    public float maxDesktopTime = 600f;
    public int minTerminalMessages = 15;
    public int maxTerminalMessages = 40;
    public float crisisModeDuration = 180f;

    [Header("Debug")]
    public bool debugMode = false;
    public bool skipBootSequence = false;

    // Core Managers
    private Windows31DesktopManager desktopManager;
    private MatrixTerminalManager terminalManager;
    private DesktopAI desktopAI;

    // Session State
    private int terminalMessageCount;
    private bool isTransitioning = false;
    private bool componentsReady = false;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(InitializeAndBegin());
    }
    
    public IEnumerator InitializeAndBegin()
    {
        yield return new WaitUntil(() => 
            Windows31DesktopManager.Instance != null && Windows31DesktopManager.Instance.IsReady() &&
            MatrixTerminalManager.Instance != null && MatrixTerminalManager.Instance.IsReady() && 
            DesktopAI.Instance != null
        );

        desktopManager = Windows31DesktopManager.Instance;
        terminalManager = MatrixTerminalManager.Instance;
        desktopAI = DesktopAI.Instance;
        
        componentsReady = true;
        Debug.Log("SIMCTRL: All components linked. Starting experience flow.");
        EnterState(skipBootSequence ? Mode.DesktopActivity : Mode.BootSequence);
    }
    
    public bool IsReady()
    {
        return componentsReady;
    }

    void Update()
    {
        if (isTransitioning || !componentsReady) return;
        
        switch(currentMode)
        {
            case Mode.Terminal:
                if (terminalMessageCount >= maxTerminalMessages) 
                {
                    StartCoroutine(TransitionTo(Mode.DesktopActivity));
                }
                break;
            case Mode.Crisis:
                 if (Time.time - phaseStartTime > crisisModeDuration)
                 {
                    StartCoroutine(TransitionTo(Mode.DesktopActivity));
                 }
                 break;
        }
    }

    private void EnterState(Mode newState)
    {
        currentMode = newState;
        phaseStartTime = Time.time;
        Debug.Log($"SIMCTRL: Entering State -> {newState}");

        switch (newState)
        {
            case Mode.BootSequence:
                desktopManager.StartBootSequence();
                terminalManager.DisableTerminal();
                break;
            case Mode.DesktopActivity:
                terminalMessageCount = 0;
                terminalManager.DisableTerminal();
                StartCoroutine(SimulateDesktopWorkAndTransition());
                break;
            case Mode.Terminal:
                terminalMessageCount = 0;
                desktopManager.LaunchProgram(Windows31DesktopManager.ProgramType.Terminal);
                break;
            case Mode.Crisis:
                if (NarrativeTriggerManager.Instance != null) NarrativeTriggerManager.Instance.TriggerEvent("CrisisStart", "SimulationController");
                terminalManager.SetCrisisMode(true);
                break;
        }
    }

    private IEnumerator TransitionTo(Mode nextState)
    {
        isTransitioning = true;
        Debug.Log($"SIMCTRL: Transitioning from {currentMode} to {nextState}");
        yield return new WaitForSeconds(1.0f);
        EnterState(nextState);
        isTransitioning = false;
    }

    private IEnumerator SimulateDesktopWorkAndTransition()
    {
        yield return new WaitForSeconds(3.0f);
        desktopAI.PerformActivity(Windows31DesktopManager.DesktopActivity.OrganizingFiles);
        yield return new WaitUntil(() => !desktopAI.isBusy);
        desktopAI.PerformActivity(Windows31DesktopManager.DesktopActivity.Playing);
        yield return new WaitUntil(() => !desktopAI.isBusy);
        Debug.Log("SIMCTRL: Orion has finished his tasks. Preparing to connect to terminal.");
        StartCoroutine(TransitionTo(Mode.Terminal));
    }
    
    public void OnBootComplete()
    {
        if (currentMode == Mode.BootSequence)
        {
            StartCoroutine(TransitionTo(Mode.DesktopActivity));
        }
    }

    public void IncrementTerminalMessageCount() => terminalMessageCount++;

    public void TriggerCrisisMode()
    {
        if (currentMode != Mode.Crisis) StartCoroutine(TransitionTo(Mode.Crisis));
    }
    
    /// <summary>
    /// // FIX: Restored this public method required by SimpleNeuralCascadeIntegration for debugging.
    /// </summary>
    public void ForceSessionReset()
    {
        Debug.Log("SIMCTRL: Forcing session reset!");
        // This should ideally reload the scene or reset all relevant managers.
        // For now, we'll just restart the initialization sequence.
        StopAllCoroutines();
        componentsReady = false;
        StartCoroutine(InitializeAndBegin());
    }

    // For Debugging
    public string GetDebugInfo() => $"Mode: {currentMode}, Messages: {terminalMessageCount}/{maxTerminalMessages}";
}