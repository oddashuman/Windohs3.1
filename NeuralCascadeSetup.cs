using UnityEngine;
using System.Collections;
using TMPro;

// The FMODUnity using directive is commented out to prevent errors if the package is missing.
// If you have the FMOD package installed, you can uncomment this line.
// using FMODUnity; 

/// <summary>
/// **Rewritten: NeuralCascadeSetup**
/// Vision: Master setup script for the Neural Cascade Windows 3.1 experience.
/// Initializes all systems in a clean, sequential order to ensure stability.
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

    void Start()
    {
        if (autoInitialize)
        {
            StartCoroutine(InitializeNeuralCascadeExperience());
        }
    }

    public IEnumerator InitializeNeuralCascadeExperience()
    {
        Debug.Log("### NEURAL CASCADE INITIALIZATION SEQUENCE STARTING ###");
        
        // FMOD code is commented out to prevent package-related errors.
        // if (RuntimeManager.IsInitialized) { RuntimeManager.StudioUnloadAllEvents(); }

        // Step 1: Instantiate all manager components
        InstantiateManagers();

        // Wait a frame for all Awake() methods to be called.
        yield return null;

        // Step 2: Configure the managers with assets and settings
        ConfigureManagers();

        // Step 3: Wait for all managers to report that they are ready
        yield return StartCoroutine(WaitForManagers());

        // FMOD code is commented out.
        // if (!RuntimeManager.IsInitialized) { try { RuntimeManager.Initialize(); } catch {} }

        Debug.Log("### NEURAL CASCADE SETUP COMPLETE. HANDING OFF TO SIMULATION CONTROLLER. ###");
    }

    private void InstantiateManagers()
    {
        if (FindObjectOfType<DialogueState>() == null) gameObject.AddComponent<DialogueState>();
        if (FindObjectOfType<TopicManager>() == null) gameObject.AddComponent<TopicManager>();
        if (FindObjectOfType<ConversationThreadManager>() == null) gameObject.AddComponent<ConversationThreadManager>();
        if (FindObjectOfType<DialogueEngine>() == null) gameObject.AddComponent<DialogueEngine>();
        if (FindObjectOfType<CursorController>() == null) gameObject.AddComponent<CursorController>();
        if (FindObjectOfType<Windows31DesktopManager>() == null) gameObject.AddComponent<Windows31DesktopManager>();
        if (FindObjectOfType<MatrixTerminalManager>() == null) gameObject.AddComponent<MatrixTerminalManager>();
        if (FindObjectOfType<SimulationController>() == null) gameObject.AddComponent<SimulationController>();
        if (FindObjectOfType<DesktopAI>() == null) gameObject.AddComponent<DesktopAI>();
    }

    private void ConfigureManagers()
    {
        Windows31DesktopManager.Instance.windows31Font = windows31FontAsset;
        Windows31DesktopManager.Instance.iconTextures = iconTextures;
        Windows31DesktopManager.Instance.systemSounds = systemSounds;
        Windows31DesktopManager.Instance.skipBootOnRestart = skipBootSequence;

        CursorController.Instance.cursorTextures = cursorTextures;

        MatrixTerminalManager.Instance.enableRetroUI = true;
        MatrixTerminalManager.Instance.enableCRTEffects = true;

        SimulationController.Instance.debugMode = debugMode;
        SimulationController.Instance.skipBootSequence = skipBootSequence;
        SimulationController.Instance.minDesktopTime = minDesktopTime;
        SimulationController.Instance.maxDesktopTime = maxDesktopTime;
        SimulationController.Instance.minTerminalMessages = minConversationMessages;
        SimulationController.Instance.maxTerminalMessages = maxConversationMessages;
    }

    private IEnumerator WaitForManagers()
    {
        Debug.Log("SETUP: Waiting for all managers to become ready...");
        
        yield return new WaitUntil(() => DialogueEngine.Instance != null && DialogueEngine.Instance.IsReady());
        Debug.Log("SETUP: DialogueEngine... READY");
        
        yield return new WaitUntil(() => Windows31DesktopManager.Instance != null && Windows31DesktopManager.Instance.IsReady());
        Debug.Log("SETUP: Windows31DesktopManager... READY");
        
        yield return new WaitUntil(() => MatrixTerminalManager.Instance != null && MatrixTerminalManager.Instance.IsReady());
        Debug.Log("SETUP: MatrixTerminalManager... READY");

        // FIX: The call to SimulationController.Instance.IsReady() will now succeed.
        yield return new WaitUntil(() => SimulationController.Instance != null && SimulationController.Instance.IsReady());
        Debug.Log("SETUP: SimulationController... READY");
    }
}