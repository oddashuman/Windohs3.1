using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// **Rewritten: NeuralCascadeSetup**
/// Vision: Master setup script for the Neural Cascade Windows 3.1 experience.
/// Initializes all systems in a clean, sequential order to ensure stability and
/// readiness before the simulation begins. This script acts as the master
/// orchestrator for the entire project.
/// </summary>
public class NeuralCascadeSetup : MonoBehaviour
{
    [Header("Setup Configuration")]
    public bool autoInitialize = true;
    public bool debugMode = true;
    public bool skipBootSequence = false;

    [Header("Core Assets (Assign in Inspector)")]
    public Texture2D[] cursorTextures;
    public Texture2D[] iconTextures;
    public TMP_FontAsset windows31FontAsset;
    public AudioClip[] systemSounds;

    [Header("Experience Pacing")]
    public float minDesktopTime = 120f;
    public float maxDesktopTime = 300f;
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
    /// The complete, ordered initialization sequence for the experience.
    /// </summary>
    public IEnumerator InitializeNeuralCascadeExperience()
    {
        Debug.Log("### NEURAL CASCADE INITIALIZATION SEQUENCE STARTING ###");

        // Step 1: Initialize the core data and state managers.
        yield return StartCoroutine(InitializeDialogueSystem());

        // Step 2: Initialize the primary user interaction system.
        yield return StartCoroutine(InitializeCursorSystem());

        // Step 3: Initialize the main environment.
        yield return StartCoroutine(InitializeDesktopSystem());

        // Step 4: Initialize the conversation interface.
        yield return StartCoroutine(InitializeTerminalSystem());

        // Step 5: Initialize the master simulation controller.
        yield return StartCoroutine(InitializeSimulationController());

        // Step 6: Verify all systems are integrated and ready.
        yield return StartCoroutine(VerifySystemIntegration());

        isSetupComplete = true;
        Debug.Log("### NEURAL CASCADE SETUP COMPLETE. HANDING OFF TO SIMULATION CONTROLLER. ###");
    }

    private IEnumerator InitializeDialogueSystem()
    {
        Debug.Log("SETUP: Initializing Dialogue System...");
        if (DialogueState.Instance == null) { gameObject.AddComponent<DialogueState>(); }
        if (TopicManager.Instance == null) { gameObject.AddComponent<TopicManager>(); }
        if (ConversationThreadManager.Instance == null) { gameObject.AddComponent<ConversationThreadManager>(); }
        if (DialogueEngine.Instance == null) { gameObject.AddComponent<DialogueEngine>(); }

        float timeout = 10f;
        while (!DialogueEngine.Instance.IsReady() && timeout > 0)
        {
            yield return new WaitForSeconds(0.1f);
            timeout -= 0.1f;
        }
        Debug.Log(DialogueEngine.Instance.IsReady() ? "SETUP: Dialogue System... READY" : "SETUP: Dialogue System... TIMEOUT");
    }

    private IEnumerator InitializeCursorSystem()
    {
        Debug.Log("SETUP: Initializing Cursor System...");
        if (CursorController.Instance == null)
        {
            var controller = gameObject.AddComponent<CursorController>();
            controller.cursorTextures = cursorTextures;
        }
        yield return new WaitUntil(() => CursorController.Instance != null);
        Debug.Log("SETUP: Cursor System... READY");
    }

    private IEnumerator InitializeDesktopSystem()
    {
        Debug.Log("SETUP: Initializing Desktop Environment...");
        if (Windows31DesktopManager.Instance == null)
        {
            var manager = gameObject.AddComponent<Windows31DesktopManager>();
            manager.windows31Font = windows31FontAsset;
            manager.iconTextures = iconTextures;
            manager.systemSounds = systemSounds;
            manager.skipBootOnRestart = skipBootSequence;
        }
        yield return new WaitUntil(() => Windows31DesktopManager.Instance != null && Windows31DesktopManager.Instance.IsReady());
        Debug.Log("SETUP: Desktop Environment... READY");
    }

    private IEnumerator InitializeTerminalSystem()
    {
        Debug.Log("SETUP: Initializing Terminal Interface...");
        if (MatrixTerminalManager.Instance == null)
        {
            var manager = gameObject.AddComponent<MatrixTerminalManager>();
            manager.enableRetroUI = true;
            manager.enableCRTEffects = true;
        }
        yield return new WaitUntil(() => MatrixTerminalManager.Instance != null && MatrixTerminalManager.Instance.IsReady());
        Debug.Log("SETUP: Terminal Interface... READY");
    }

    private IEnumerator InitializeSimulationController()
    {
        Debug.Log("SETUP: Initializing Simulation Controller...");
        if (SimulationController.Instance == null)
        {
            var controller = gameObject.AddComponent<SimulationController>();
            controller.debugMode = debugMode;
            controller.skipBootSequence = skipBootSequence;
            controller.minDesktopTime = minDesktopTime;
            controller.maxDesktopTime = maxDesktopTime;
            controller.minTerminalMessages = minConversationMessages;
            controller.maxTerminalMessages = maxConversationMessages;
        }
        // The SimulationController initializes itself and its components.
        yield return new WaitUntil(() => SimulationController.Instance != null && SimulationController.Instance.IsReady());
        Debug.Log("SETUP: Simulation Controller... READY");
    }

    private IEnumerator VerifySystemIntegration()
    {
        Debug.Log("SETUP: Verifying all system integrations...");
        bool allSystemsReady = DialogueEngine.Instance.IsReady() &&
                               CursorController.Instance != null &&
                               Windows31DesktopManager.Instance.IsReady() &&
                               MatrixTerminalManager.Instance.IsReady() &&
                               SimulationController.Instance.IsReady();

        if (allSystemsReady)
        {
            Debug.Log("SETUP: All systems verified and integrated successfully.");
        }
        else
        {
            Debug.LogError("SETUP: CRITICAL FAILURE - One or more systems failed to initialize. Experience cannot continue.");
            // In a real build, you might show an error message here.
            enabled = false; // Disable this script.
        }
        yield return null;
    }
}