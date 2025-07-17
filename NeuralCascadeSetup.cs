using UnityEngine;
using System.Collections;
using TMPro;

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
        
        InstantiateManagers();
        yield return null; 
        ConfigureManagers();
        yield return StartCoroutine(WaitForManagers());

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
        // CursorController initialization is no longer handled here.
        Windows31DesktopManager.Instance.windows31Font = windows31FontAsset;
        Windows31DesktopManager.Instance.iconTextures = iconTextures;
        Windows31DesktopManager.Instance.systemSounds = systemSounds;
        Windows31DesktopManager.Instance.skipBootOnRestart = skipBootSequence;

        // This line was moved from the reverted code; it is safe to keep
        if (CursorController.Instance != null)
        {
            CursorController.Instance.cursorTextures = cursorTextures;
        }

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
        Debug.log("SETUP: MatrixTerminalManager... READY");

        yield return new WaitUntil(() => SimulationController.Instance != null && SimulationController.Instance.IsReady());
        Debug.Log("SETUP: SimulationController... READY");
    }
}