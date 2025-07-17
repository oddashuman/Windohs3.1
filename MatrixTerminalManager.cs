using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class MatrixTerminalManager : MonoBehaviour
{
    public static MatrixTerminalManager Instance { get; private set; }

    [Header("UI Components")]
    private CanvasGroup terminalCanvasGroup;
    private TMP_Text terminalText;
    private ScrollRect scrollRect;
    private Image titleBar;
    private TMP_Text titleBarText;

    [Header("Typing Simulation")]
    public string cursorSymbol = "_";
    public float blinkInterval = 0.5f;
    
    // FIX: Added back the properties required by NeuralCascadeSetup
    public bool enableRetroUI = true;
    public bool enableCRTEffects = true;

    private bool isTyping = false;
    private bool terminalActive = false;
    private StringBuilder currentDisplayText = new StringBuilder();
    private Coroutine typingCoroutine;
    private DialogueEngine dialogueEngine;
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
        yield return new WaitUntil(() => DialogueEngine.Instance != null && DialogueEngine.Instance.IsReady());
        dialogueEngine = DialogueEngine.Instance;
        isInitialized = true;
        Debug.Log("TERMINAL: Systems linked. Ready.");
    }

    public void InitializeUI(CanvasGroup canvasGroup, TMP_Text textComponent, ScrollRect scrollComponent, Image bar, TMP_Text barText)
    {
        terminalCanvasGroup = canvasGroup;
        terminalText = textComponent;
        scrollRect = scrollComponent;
        titleBar = bar;
        titleBarText = barText;
        DisableTerminal();
    }
    
    // FIX: Added back the IsReady() method required by SimulationController
    public bool IsReady()
    {
        return isInitialized;
    }

    void Update()
    {
        if (!terminalActive || !isInitialized || isTyping || typingCoroutine != null) return;
        
        typingCoroutine = StartCoroutine(ProcessNextMessage());
    }

    private IEnumerator ProcessNextMessage()
    {
        isTyping = true;
        yield return new WaitForSeconds(Random.Range(1.0f, 2.5f));

        DialogueMessage message = dialogueEngine.GetNextMessage();
        if (message != null && !string.IsNullOrEmpty(message.text))
        {
            yield return StartCoroutine(TypeMessage(message));
            if (SimulationController.Instance != null) 
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

        foreach (char c in message.text)
        {
            currentDisplayText.Append(c);
            UpdateTerminalDisplay();
            yield return new WaitForSeconds(0.05f / speakerProfile.GetTypingSpeedMultiplier(message.text));
        }
        
        yield return new WaitForEndOfFrame();
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
    }

    private void UpdateTerminalDisplay()
    {
        if (terminalText != null)
            terminalText.text = currentDisplayText.ToString() + (terminalActive ? cursorSymbol : "");
    }

    public void EnableTerminal()
    {
        if (terminalCanvasGroup == null) return;
        terminalCanvasGroup.alpha = 1;
        terminalCanvasGroup.interactable = true;
        terminalActive = true;
        currentDisplayText.Clear();
        currentDisplayText.Append(">>> NEURAL LINK ESTABLISHED...");
        UpdateTerminalDisplay();
    }

    public void DisableTerminal()
    {
        if (terminalCanvasGroup == null) return;
        terminalActive = false;
        terminalCanvasGroup.alpha = 0;
        terminalCanvasGroup.interactable = false;
    }
    
    public void SetCrisisMode(bool isCrisis)
    {
        if (titleBar != null)
        {
            titleBar.color = isCrisis ? Color.red : new Color32(0, 0, 128, 255);
        }
        if (titleBarText != null)
        {
            titleBarText.text = isCrisis ? "TERMINAL - !! SYSTEM ALERT !!" : "Terminal";
        }
    }

    private Color GetCharacterColor(string characterName)
    {
        switch (characterName)
        {
            case "Orion": return Color.cyan;
            case "Nova": return Color.yellow;
            case "Echo": return Color.magenta;
            case "Lumen": return Color.green;
            default: return Color.white;
        }
    }
}