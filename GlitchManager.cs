using UnityEngine;
using System.Collections;

/// <summary>
/// **New Script: GlitchManager**
/// Vision: Implements the "Glitch System" from the README. This manager creates
/// visual and behavioral anomalies to enhance the simulation narrative and build
/// tension. It responds to the game's state (tension, topics) to make the
/// glitches feel meaningful rather than random.
/// </summary>
public class GlitchManager : MonoBehaviour
{
    public static GlitchManager Instance { get; private set; }

    [Header("Glitch Intensity")]
    [Range(0, 1)]
    public float globalGlitchChance = 0.05f;
    public float tensionMultiplier = 2.0f; // How much tension increases glitch frequency

    // References
    private CursorController cursorController;
    private DialogueState dialogueState;
    private Windows31DesktopManager desktopManager;

    private bool isInitialized = false;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(LinkComponents());
    }

    private IEnumerator LinkComponents()
    {
        // Link to singletons
        cursorController = CursorController.Instance;
        dialogueState = DialogueState.Instance;
        desktopManager = Windows31DesktopManager.Instance;

        isInitialized = cursorController != null && dialogueState != null && desktopManager != null;
        if (isInitialized)
        {
            Debug.Log("GLITCHMAN: Systems linked. Ready to introduce anomalies.");
        }
        else
        {
             Debug.LogError("GLITCHMAN: Failed to link to core systems!");
        }
        yield return null;
    }

    void Update()
    {
        if (!isInitialized) return;

        // The core glitch-triggering logic
        float currentChance = globalGlitchChance + (dialogueState.globalTension * tensionMultiplier);
        if (Random.value < currentChance * Time.deltaTime)
        {
            TriggerRandomGlitch();
        }
    }

    /// <summary>
    /// Triggers a random, context-aware glitch from the available types.
    /// </summary>
    public void TriggerRandomGlitch()
    {
        float rand = Random.value;

        if (rand < 0.4f)
        {
            StartCoroutine(CursorAnomaly());
        }
        else if (rand < 0.7f)
        {
            StartCoroutine(ScreenFlicker());
        }
        else
        {
            StartCoroutine(WindowDisplacement());
        }
    }

    /// <summary>
    /// **Cursor Anomaly:** The cursor jumps, freezes, or jitters uncontrollably.
    /// </summary>
    private IEnumerator CursorAnomaly()
    {
        dialogueState.AddGlitchEvent("Cursor Anomaly", "Cursor behavior erratic", 1.5f);
        
        Vector2 originalPosition = cursorController.GetCurrentPosition();
        Vector2 jumpTarget = originalPosition + new Vector2(Random.Range(-50, 50), Random.Range(-50, 50));

        cursorController.MoveTo(jumpTarget, true); // Instant jump
        yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        cursorController.MoveTo(originalPosition, false); // Smooth return
    }

    /// <summary>
    /// **Screen Flicker:** A quick, full-screen color flash or visual noise.
    /// </summary>
    private IEnumerator ScreenFlicker()
    {
        dialogueState.AddGlitchEvent("Visual Corruption", "Screen flicker detected", 2.0f);
        desktopManager.TriggerScreenFlicker();
        yield return null;
    }

    /// <summary>
    /// **Window Displacement:** An open window suddenly jumps to a new position.
    /// </summary>
    private IEnumerator WindowDisplacement()
    {
        var window = desktopManager.GetRandomOpenWindow();
        if (window != null)
        {
            dialogueState.AddGlitchEvent("Window Anomaly", $"{window.title} displaced", 1.0f);
            Vector2 currentPos = window.GetComponent<RectTransform>().anchoredPosition;
            Vector2 newPos = currentPos + new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
            window.SetPosition(newPos);
        }
        yield return null;
    }
}