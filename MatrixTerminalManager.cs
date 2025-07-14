using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

/// <summary>
/// **Rewritten: MatrixTerminalManager**
/// Vision: Transforms the terminal from a simple text box into a dynamic stage for
/// character performance. It integrates deeply with the DialogueEngine and CharacterProfile
/// to simulate realistic, character-specific typing behaviors, including speed,
/// hesitations on sensitive topics, typos, and pacing that reflects the
/// emotional state of the conversation.
/// </summary>
public class MatrixTerminalManager : MonoBehaviour
{
    public static MatrixTerminalManager Instance { get; private set; }

    [Header("UI Components")]
    private Canvas terminalCanvas;
    private CanvasGroup terminalCanvasGroup;
    private TMP_Text terminalText;
    private ScrollRect scrollRect;
    private Image titleBar;
    private TMP_Text titleBarText;

    [Header("Typing Simulation")]
    public string cursorSymbol = "_";
    public float blinkInterval = 0.5f;

    // State
    private bool isTyping = false;
    private bool terminalActive = false;
    private bool cursorVisible = true;
    private StringBuilder currentDisplayText = new StringBuilder();
    private Coroutine typingCoroutine;

    // Integration
    private DialogueEngine dialogueEngine;
    private DialogueState dialogueState;
    
    private bool isInitialized = false;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(InitializeAndLink());
    }

    private IEnumerator InitializeAndLink()
    {
        CreateTerminalUI();

        // Link to already initialized singletons
        dialogueEngine = DialogueEngine.Instance;
        dialogueState = DialogueState.Instance;
        
        isInitialized = dialogueEngine != null && dialogueState != null;
        if (!isInitialized)
        {
            Debug.LogError("TERMINAL: Dialogue systems not found!");
            yield break;
        }

        Debug.Log("TERMINAL: Systems linked. Ready.");
        DisableTerminal();
    }
    
    void Update()
    {
        if (!terminalActive || !isInitialized) return;

        HandleCursorBlink();

        if (!isTyping && typingCoroutine == null)
        {
            typingCoroutine = StartCoroutine(ProcessNextMessage());
        }
    }

    private IEnumerator ProcessNextMessage()
    {
        isTyping = true;

        // Determine dynamic delay based on conversation state
        float delay = 1.5f; 
        if (dialogueState.globalTension > 0.7f) delay = 0.5f;
        if (dialogueState.metaAwareness > 0.6f) delay = 0.8f;
        yield return new WaitForSeconds(delay);

        var message = dialogueEngine.GetNextMessage();

        if (message != null && !string.IsNullOrEmpty(message.text))
        {
            yield return StartCoroutine(TypeMessage(message));
            SimulationController.Instance.IncrementTerminalMessageCount();
        }

        isTyping = false;
        typingCoroutine = null;
    }

    private IEnumerator TypeMessage(DialogueMessage message)
    {
        var speakerProfile = dialogueEngine.allCharacters[message.speaker];
        string prefix = $"\n<color=#{ColorUtility.ToHtmlStringRGB(GetCharacterColor(message.speaker))}>{message.speaker}:</color> ";
        currentDisplayText.Append(prefix);

        string messageText = message.text;
        
        // Use the character's profile to determine typing style
        float charDelay = 0.05f / speakerProfile.GetTypingSpeedMultiplier(messageText);

        for (int i = 0; i < messageText.Length; i++)
        {
            char c = messageText[i];

            // Simulate hesitation for sensitive topics
            if (speakerProfile.ShouldHesitateOnTopic(messageText) && i > 0 && messageText[i-1] == ' ')
            {
                if (Random.value < speakerProfile.hesitationChance)
                {
                    yield return new WaitForSeconds(Random.Range(0.3f, 0.8f));
                }
            }
            
            // Simulate typos
            if (Random.value < speakerProfile.typoFrequency)
            {
                currentDisplayText.Append((char)('a' + Random.Range(0, 26)));
                UpdateTerminalDisplay(true);
                yield return new WaitForSeconds(0.1f);
                currentDisplayText.Length--; // Backspace
            }

            currentDisplayText.Append(c);
            UpdateTerminalDisplay(true);
            yield return new WaitForSeconds(charDelay);
        }
        
        // Finalize display and scroll down
        UpdateTerminalDisplay(false);
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 0f;
    }
    
    private void UpdateTerminalDisplay(bool showCursor)
    {
        if (terminalText == null) return;
        
        if (showCursor)
        {
            terminalText.text = currentDisplayText.ToString() + cursorSymbol;
        }
        else
        {
            terminalText.text = currentDisplayText.ToString();
        }
    }
    
    private void HandleCursorBlink()
    {
        if (isTyping)
        {
            cursorVisible = true;
            return;
        }

        if (Time.time > lastBlinkTime + blinkInterval)
        {
            cursorVisible = !cursorVisible;
            lastBlinkTime = Time.time;
            
            if (cursorVisible)
            {
                 terminalText.text = currentDisplayText.ToString() + cursorSymbol;
            }
            else
            {
                 terminalText.text = currentDisplayText.ToString();
            }
        }
    }

    #region Public API
    public void EnableTerminal()
    {
        currentDisplayText.Clear();
        currentDisplayText.Append(">>> NEURAL LINK ESTABLISHED. AWAITING INCOMING TRANSMISSIONS...");
        terminalCanvasGroup.alpha = 1;
        terminalActive = true;
    }

    public void DisableTerminal()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        isTyping = false;
        typingCoroutine = null;
        terminalActive = false;
        terminalCanvasGroup.alpha = 0;
    }
    
    public IEnumerator FadeOutTerminal(float duration)
    {
        float startAlpha = terminalCanvasGroup.alpha;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            terminalCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0, t / duration);
            yield return null;
        }
        DisableTerminal();
    }

    public void SetCrisisMode(bool enabled)
    {
        // In a crisis, the terminal might flicker, show corrupted text, etc.
        if (enabled)
        {
            titleBar.color = Color.red;
            titleBarText.text = "TERMINAL - !! SYSTEM ALERT !!";
        }
        else
        {
            titleBar.color = new Color32(0, 0, 128, 255); // Blue
             titleBarText.text = "Terminal";
        }
    }
    #endregion
    
    #region UI Creation
    private void CreateTerminalUI()
    {
        GameObject canvasGO = new GameObject("Terminal_Canvas");
        canvasGO.transform.SetParent(this.transform);
        terminalCanvas = canvasGO.AddComponent<Canvas>();
        terminalCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        terminalCanvas.sortingOrder = 100;
        
        terminalCanvasGroup = canvasGO.AddComponent<CanvasGroup>();

        // Create a Window for the terminal
        GameObject windowGO = new GameObject("Terminal_Window");
        windowGO.transform.SetParent(canvasGO.transform, false);
        var window = windowGO.AddComponent<Window>();
        window.Initialize("Terminal", new Vector2(800, 600), 1024);
        
        // Get components from the created window
        this.titleBar = window.GetComponentInChildren<Image>(); // Simplification, get the main title bar image
        this.titleBarText = window.GetComponentInChildren<TMP_Text>(); // Get the title text
        
        // Create the scroll view within the window's content area
        CreateScrollView(window.contentArea);
    }

    private void CreateScrollView(RectTransform parent)
    {
        GameObject scrollArea = new GameObject("ScrollArea");
        scrollArea.transform.SetParent(parent, false);
        scrollRect = scrollArea.AddComponent<ScrollRect>();
        
        var rt = scrollArea.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollArea.transform, false);
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        viewport.AddComponent<Image>().color = Color.black;
        var viewRT = viewport.GetComponent<RectTransform>();
        viewRT.anchorMin = Vector2.zero;
        viewRT.anchorMax = Vector2.one;
        viewRT.offsetMin = viewRT.offsetMax = new Vector2(5,5);
        scrollRect.viewport = viewRT;

        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var contentFitter = content.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.sizeDelta = new Vector2(0, 0);
        scrollRect.content = contentRT;

        // Text
        GameObject textGO = new GameObject("TerminalText");
        textGO.transform.SetParent(content.transform, false);
        terminalText = textGO.AddComponent<TextMeshProUGUI>();
        terminalText.font = Windows31DesktopManager.Instance.windows31Font;
        terminalText.fontSize = 14;
        terminalText.color = Color.green;
        terminalText.alignment = TextAlignmentOptions.TopLeft;
        terminalText.enableWordWrapping = true;
    }

    private Color GetCharacterColor(string characterName)
    {
        switch (characterName)
        {
            case "Orion": return new Color32(139, 195, 255, 255); // Light Blue
            case "Nova": return new Color32(255, 165, 0, 255); // Orange
            case "Echo": return new Color32(204, 153, 255, 255); // Lavender
            case "Lumen": return new Color32(144, 238, 144, 255); // Light Green
            case "SYSTEM": return new Color32(255, 255, 0, 255); // Yellow
            case "OVERSEER": return new Color32(255, 69, 58, 255); // Red
            default: return Color.white;
        }
    }
    #endregion
    
    public bool IsReady() => isInitialized;
}