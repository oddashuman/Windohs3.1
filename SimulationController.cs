using UnityEngine;
using System.Collections;

/// <summary>
/// Acts as the "director" of the experience, managing the high-level
/// state transitions.
/// </summary>
public class SimulationController : MonoBehaviour
{
    public static SimulationController Instance { get; private set; }

    public enum Mode { BootSequence, DesktopActivity, Terminal, PostConversation, Screensaver, Shutdown, Crisis }

    [Header("State")]
    public Mode currentMode = Mode.BootSequence;
    private float phaseStartTime;

    [Header("Timings")]
    public float minDesktopTime = 180f;
    public float maxDesktopTime = 600f;
    public int minTerminalMessages = 15;
    public int maxTerminalMessages = 40;
    public float crisisModeDuration = 180f;
    public float screensaverIdleTime = 120f;

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
    private float lastActivityTime;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
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

        switch (currentMode)
        {
            case Mode.DesktopActivity:
            case Mode.PostConversation:
                if (Time.time - lastActivityTime > screensaverIdleTime)
                {
                    StartCoroutine(TransitionTo(Mode.Screensaver));
                }
                break;
            case Mode.Terminal:
                if (terminalMessageCount >= maxTerminalMessages)
                {
                    StartCoroutine(TransitionTo(Mode.PostConversation));
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
        lastActivityTime = Time.time;
        Debug.Log($"SIMCTRL: Entering State -> {newState}");

        switch (newState)
        {
            case Mode.BootSequence:
                desktopManager.StartBootSequence();
                terminalManager.DisableTerminal();
                desktopManager.SetScreensaver(false);
                break;
            case Mode.DesktopActivity:
                terminalMessageCount = 0;
                terminalManager.DisableTerminal();
                desktopManager.SetScreensaver(false);
                StartCoroutine(SimulateDesktopWork());
                break;
            case Mode.Terminal:
                terminalMessageCount = 0;
                desktopManager.LaunchProgram(Windows31DesktopManager.ProgramType.Terminal);
                desktopManager.SetScreensaver(false);
                break;
            case Mode.PostConversation:
                terminalManager.DisableTerminal();
                desktopManager.SetScreensaver(false);
                StartCoroutine(SimulatePostConversationWork());
                break;
            case Mode.Screensaver:
                desktopManager.SetScreensaver(true);
                break;
            case Mode.Shutdown:
                StartCoroutine(ShutdownAndRestart());
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

    private IEnumerator SimulateDesktopWork()
    {
        yield return new WaitForSeconds(3.0f);
        desktopAI.PerformActivity(Windows31DesktopManager.DesktopActivity.OrganizingFiles);
        yield return new WaitUntil(() => !desktopAI.isBusy);
        lastActivityTime = Time.time;

        yield return new WaitForSeconds(Random.Range(10f, 20f));
        desktopAI.PerformActivity(Windows31DesktopManager.DesktopActivity.ReviewingNotes);
        yield return new WaitUntil(() => !desktopAI.isBusy);
        lastActivityTime = Time.time;

        Debug.Log("SIMCTRL: Orion has finished his tasks. Preparing to connect to terminal.");
        StartCoroutine(TransitionTo(Mode.Terminal));
    }

    private IEnumerator SimulatePostConversationWork()
    {
        yield return new WaitForSeconds(3.0f);
        desktopAI.PerformActivity(Windows31DesktopManager.DesktopActivity.Playing);
        yield return new WaitUntil(() => !desktopAI.isBusy);
        lastActivityTime = Time.time;

        Debug.Log("SIMCTRL: Orion is done playing. Shutting down.");
        StartCoroutine(TransitionTo(Mode.Shutdown));
    }

    private IEnumerator ShutdownAndRestart()
    {
        // Add a shutdown sequence here if you want one
        Debug.Log("SIMCTRL: Shutting down and restarting the loop.");
        yield return new WaitForSeconds(5.0f);
        ForceSessionReset();
    }

    public void OnBootComplete()
    {
        if (currentMode == Mode.BootSequence)
        {
            StartCoroutine(TransitionTo(Mode.DesktopActivity));
        }
    }

    public void ReportActivity()
    {
        lastActivityTime = Time.time;
        if (currentMode == Mode.Screensaver)
        {
            StartCoroutine(TransitionTo(Mode.DesktopActivity));
        }
    }

    public void IncrementTerminalMessageCount() => terminalMessageCount++;

    public void TriggerCrisisMode()
    {
        if (currentMode != Mode.Crisis) StartCoroutine(TransitionTo(Mode.Crisis));
    }

    public void ForceSessionReset()
    {
        Debug.Log("SIMCTRL: Forcing session reset!");
        if (DialogueState.Instance != null) DialogueState.Instance.Reset();
        StopAllCoroutines();
        componentsReady = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public string GetDebugInfo() => $"Mode: {currentMode}, Time in Phase: {Time.time - phaseStartTime:F1}s, Messages: {terminalMessageCount}/{maxTerminalMessages}";
}