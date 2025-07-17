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
        yield return new WaitUntil(() => CursorController.Instance != null &&
                                       DialogueState.Instance != null &&
                                       Windows31DesktopManager.Instance != null &&
                                       Windows31DesktopManager.Instance.IsReady());

        cursorController = CursorController.Instance;
        dialogueState = DialogueState.Instance;
        desktopManager = Windows31DesktopManager.Instance;

        isInitialized = true;
        Debug.Log("GLITCHMAN: Systems linked. Ready to introduce anomalies.");
    }

    void Update()
    {
        if (!isInitialized) return;

        // The core glitch-triggering logic, now influenced by tension
        float currentChance = globalGlitchChance + (dialogueState.globalTension * tensionMultiplier * Time.deltaTime);
        if (Random.value < currentChance)
        {
            TriggerRandomGlitch(dialogueState.globalTension);
        }
    }

    /// <summary>
    /// Triggers a random, context-aware glitch from the available types.
    /// The severity of the glitch is influenced by the narrative tension.
    /// </summary>
    public void TriggerRandomGlitch(float severity)
    {
        if (!isInitialized) return;

        float rand = Random.value;

        // Higher severity increases the chance of more dramatic glitches
        if (rand < 0.4f + (severity * 0.2f))
        {
            StartCoroutine(CursorAnomaly(severity));
        }
        else if (rand < 0.7f + (severity * 0.1f))
        {
            StartCoroutine(ScreenFlicker(severity));
        }
        else
        {
            StartCoroutine(WindowDisplacement(severity));
        }
    }

    /// <summary>
    /// **Cursor Anomaly:** The cursor jumps, freezes, or jitters uncontrollably.
    /// The intensity of the anomaly is now tied to the severity parameter.
    /// </summary>
    private IEnumerator CursorAnomaly(float severity)
    {
        dialogueState.AddGlitchEvent("Cursor Anomaly", "Cursor behavior erratic", 1.5f + severity);

        Vector2 originalPosition = cursorController.GetCurrentPosition();
        float jumpDistance = 50f + (severity * 100f);
        Vector2 jumpTarget = originalPosition + new Vector2(Random.Range(-jumpDistance, jumpDistance), Random.Range(-jumpDistance, jumpDistance));

        cursorController.MoveTo(jumpTarget, true); // Instant jump
        yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        cursorController.MoveTo(originalPosition, false); // Smooth return
        SimulationController.Instance.ReportActivity();
    }

    /// <summary>
    /// **Screen Flicker:** A quick, full-screen color flash or visual noise.
    /// Higher severity can cause more intense or longer flickers.
    /// </summary>
    private IEnumerator ScreenFlicker(float severity)
    {
        dialogueState.AddGlitchEvent("Visual Corruption", "Screen flicker detected", 2.0f + severity);
        desktopManager.TriggerScreenFlicker();
        if (severity > 0.5f)
        {
            yield return new WaitForSeconds(0.1f);
            desktopManager.TriggerScreenFlicker();
        }
        yield return null;
        SimulationController.Instance.ReportActivity();
    }

    /// <summary>
    /// **Window Displacement:** An open window suddenly jumps to a new position.
    /// The distance of the jump is now influenced by the severity.
    /// </summary>
    private IEnumerator WindowDisplacement(float severity)
    {
        var window = desktopManager.GetRandomOpenWindow();
        if (window != null)
        {
            dialogueState.AddGlitchEvent("Window Anomaly", $"{window.title} displaced", 1.0f + severity);
            Vector2 currentPos = window.GetComponent<RectTransform>().anchoredPosition;
            float displacement = 20f + (severity * 50f);
            Vector2 newPos = currentPos + new Vector2(Random.Range(-displacement, displacement), Random.Range(-displacement, displacement));
            window.SetPosition(newPos);
        }
        yield return null;
        SimulationController.Instance.ReportActivity();
    }
}