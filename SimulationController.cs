using UnityEngine;
using System.Collections;

/// <summary>
/// **Complete Code: SimulationController**
/// Vision: Acts as the "director" of the experience, managing the high-level
/// state transitions. It now delegates the specific actions within the
/// "DesktopActivity" phase to the DesktopAI and is fully integrated with all
/// narrative and environmental systems to create a cohesive, intelligent simulation.
/// </summary>
public class SimulationController : MonoBehaviour
{
    public static SimulationController Instance { get; private set; }

    public enum Mode { BootSequence, DesktopActivity, Terminal, Crisis, Shutdown }

    [Header("Experience Phases")]
    public Mode currentMode = Mode.BootSequence;

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
    private CursorController cursorController;
    private DialogueState dialogueState;
    private DesktopAI desktopAI;
    private GlitchManager glitchManager;
    private EnvironmentManager environmentManager;
    private ViewerInjectionManager viewerInjectionManager;
    private NarrativeTriggerManager narrativeTriggerManager;

    // Session State
    private float phaseStartTime;
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
    
    private IEnumerator InitializeAndBegin()
    {
        // Link to all singleton managers, which should have been created by NeuralCascadeSetup
        desktopManager = Windows31DesktopManager.Instance;
        terminalManager = MatrixTerminalManager.Instance;
        cursorController = CursorController.Instance;
        dialogueState = DialogueState.Instance;
        desktopAI = DesktopAI.Instance;
        glitchManager = GlitchManager.Instance;
        environmentManager = EnvironmentManager.Instance;
        viewerInjectionManager = ViewerInjectionManager.Instance;
        narrativeTriggerManager = NarrativeTriggerManager.Instance;

        componentsReady = desktopManager != null && terminalManager != null && cursorController != null &&
                          dialogueState != null && desktopAI != null && glitchManager != null &&
                          environmentManager != null && viewerInjectionManager != null &&
                          narrativeTriggerManager != null;

        if (!componentsReady)
        {
            Debug.LogError("SIMCTRL: One or more critical components not found! Aborting simulation.");
            yield break;
        }

        Debug.Log("SIMCTRL: All components linked. Starting experience flow.");
        yield return new WaitForSeconds(1f);
        
        EnterState(skipBootSequence ? Mode.DesktopActivity : Mode.BootSequence);
    }
    
    void Update()
    {
        if (!componentsReady || isTransitioning) return;
        
        // This state machine is now primarily for transitions triggered by events,
        // as the AI and other managers handle the duration and flow within states.
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
                desktopManager.ShowDesktop();
                terminalManager.DisableTerminal();
                StartCoroutine(SimulateDesktopWorkAndTransition());
                break;
            case Mode.Terminal:
                terminalMessageCount = 0;
                desktopManager.TriggerConversationMode();
                terminalManager.EnableTerminal();
                break;
            case Mode.Crisis:
                narrativeTriggerManager.TriggerEvent("CrisisStart", "SimulationController");
                terminalManager.SetCrisisMode(true);
                break;
        }
    }

    private IEnumerator TransitionTo(Mode nextState)
    {
        isTransitioning = true;
        Debug.Log($"SIMCTRL: Transitioning from {currentMode} to {nextState}");

        if (currentMode == Mode.Terminal)
        {
            yield return StartCoroutine(terminalManager.FadeOutTerminal(1.5f));
        }

        yield return new WaitForSeconds(1.0f);

        EnterState(nextState);

        isTransitioning = false;
    }

    /// <summary>
    /// AI-driven routine for the desktop phase. Orion performs a series of tasks
    /// that feel natural before deciding to enter the terminal.
    /// </summary>
    private IEnumerator SimulateDesktopWorkAndTransition()
    {
        yield return new WaitForSeconds(3.0f); // Orion settles in

        // Give the AI a series of context-aware objectives
        desktopAI.PerformActivity(Windows31DesktopManager.DesktopActivity.OrganizingFiles);
        yield return new WaitUntil(() => !desktopAI.isBusy);

        desktopAI.PerformActivity(Windows31DesktopManager.DesktopActivity.Playing); // Orion plays Solitaire to think
        yield return new WaitUntil(() => !desktopAI.isBusy);
        
        Debug.Log("SIMCTRL: Orion has finished his tasks. Preparing to connect to terminal.");
        
        StartCoroutine(TransitionTo(Mode.Terminal));
    }
    
    // Callback from Windows31DesktopManager
    public void OnBootComplete()
    {
        if (currentMode == Mode.BootSequence)
        {
            Debug.Log("SIMCTRL: Boot sequence complete. Transitioning to Desktop.");
            StartCoroutine(TransitionTo(Mode.DesktopActivity));
        }
    }

    public void IncrementTerminalMessageCount()
    {
        terminalMessageCount++;
    }

    public void TriggerCrisisMode()
    {
        if (currentMode != Mode.Crisis)
        {
            StartCoroutine(TransitionTo(Mode.Crisis));
        }
    }
    
    public bool IsReady() => componentsReady;
}