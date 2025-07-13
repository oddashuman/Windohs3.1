using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

public class MatrixTerminalManager : MonoBehaviour
{
    public static MatrixTerminalManager Instance { get; private set; }

    private Canvas terminalCanvas;
    private CanvasGroup terminalCanvasGroup;
    private TMP_Text terminalText;
    private RectTransform textRect;
    private TMP_FontAsset terminalFont;

    [Header("Retro UI Components")]
    public bool enableRetroUI = true;
    private Image titleBar;
    private TMP_Text titleBarText;
    private Image windowBorder;
    private Button minimizeButton, maximizeButton, closeButton;
    private Image statusBar;
    private TMP_Text statusBarText;
    private ScrollRect scrollRect;
    private Scrollbar verticalScrollbar;

    [Header("Retro Styling")]
    public Color windowBorderColor = new Color(0.7f, 0.7f, 0.7f);
    public Color titleBarColor = new Color(0.0f, 0.5f, 0.8f);
    public Color titleBarTextColor = Color.white;
    public Color statusBarColor = new Color(0.8f, 0.8f, 0.8f);
    public Color statusBarTextColor = Color.black;
    public Color scrollbarColor = new Color(0.8f, 0.8f, 0.8f);

    [Header("CRT Effects")]
    public bool enableCRTEffects = true;
    public float scanlineIntensity = 0.15f;
    public float flickerIntensity = 0.02f;
    public float curvatureAmount = 0.1f;
    private Material crtMaterial;
    private Image crtOverlay;

    [Header("Typing Parameters")]
    public float minTypingCharDelay = 0.05f;
    public float maxTypingCharDelay = 0.08f;
    public float crisisTypingSpeedMultiplier = 0.4f;
    public float typoChance = 0.03f;
    public float pauseAfterTypo = 0.22f;

    [Header("Message Timing")]
    public float messageDelay = 1.0f;
    public float crisisMessageDelay = 0.5f;
    public string cursorSymbol = "_";
    public float blinkInterval = 0.5f;

    private bool isTyping = false;
    private bool terminalActive = false;
    private bool inCrisisMode = false;
    private float lastBlinkTime = 0f;
    private bool cursorVisible = true;
    private string currentDisplayText = "";
    private DialogueMessage lastMessage = null;
    private string lastSpeaker = "";
    private Queue<SystemMessage> systemMessageQueue = new Queue<SystemMessage>();

    private Coroutine typeRoutine;

    // Enhanced state tracking
    private float tensionLevel = 0f;
    private float nextMessageTime = 0f;

    // Character-specific typing behavior
    private Dictionary<string, TypingBehavior> characterTypingBehaviors = new Dictionary<string, TypingBehavior>();

    // Initialization tracking
    private bool isInitialized = false;
    private float initializationTimeout = 10f;
    private float initStartTime;

    // Enhanced visual feedback
    private Dictionary<string, Color> characterColors = new Dictionary<string, Color>();

    // Retro UI state
    private bool isWindowMaximized = true;
    private Vector2 windowedSize = new Vector2(1200, 800);
    private Vector2 windowedPosition = new Vector2(0, 0);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        CreateTerminalUI();
        InitializeCharacterBehaviors();
        InitializeCharacterColors();
        initStartTime = Time.time;
    }

    void Start()
    {
        DisableTerminal();
        StartCoroutine(WaitForDialogueEngineInitialization());
    }

    private void InitializeCharacterBehaviors()
    {
        characterTypingBehaviors["Orion"] = new TypingBehavior
        {
            baseSpeedMultiplier = 1.1f,
            hesitationOnSensitive = true,
            deletesAndRetypes = true,
            hasLongPauses = true,
            typoFrequency = 0.02f,
            pauseFrequency = 0.15f
        };

        characterTypingBehaviors["Nova"] = new TypingBehavior
        {
            baseSpeedMultiplier = 1.3f,
            hesitationOnSensitive = false,
            deletesAndRetypes = false,
            hasLongPauses = false,
            typoFrequency = 0.05f,
            pauseFrequency = 0.05f
        };

        characterTypingBehaviors["Echo"] = new TypingBehavior
        {
            baseSpeedMultiplier = 0.7f,
            hesitationOnSensitive = true,
            deletesAndRetypes = true,
            hasLongPauses = true,
            typoFrequency = 0.08f,
            pauseFrequency = 0.35f
        };

        characterTypingBehaviors["Lumen"] = new TypingBehavior
        {
            baseSpeedMultiplier = 0.9f,
            hesitationOnSensitive = false,
            deletesAndRetypes = false,
            hasLongPauses = true,
            typoFrequency = 0.03f,
            pauseFrequency = 0.20f
        };
    }

    private void InitializeCharacterColors()
    {
        characterColors["Orion"] = new Color(0.0f, 0.0f, 0.8f);      // Dark blue
        characterColors["Nova"] = new Color(0.8f, 0.4f, 0.0f);       // Dark orange/brown
        characterColors["Echo"] = new Color(0.5f, 0.0f, 0.5f);       // Dark purple
        characterColors["Lumen"] = new Color(0.0f, 0.6f, 0.0f);      // Dark green
        characterColors["OVERSEER"] = new Color(0.8f, 0.0f, 0.0f);   // Dark red
        characterColors["SYSTEM"] = new Color(0.0f, 0.0f, 0.0f);     // Black
    }

    IEnumerator WaitForDialogueEngineInitialization()
    {
        while (Time.time - initStartTime < initializationTimeout)
        {
            if (DialogueEngine.Instance != null && DialogueEngine.Instance.IsReady())
            {
                isInitialized = true;
                Debug.Log("MatrixTerminalManager: DialogueEngine ready");
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.LogWarning("MatrixTerminalManager: DialogueEngine initialization timeout, proceeding anyway");
        isInitialized = true;
    }

    void Update()
    {
        if (!terminalActive) return;

        UpdateTensionLevel();
        UpdateRetroUI();
        UpdateCRTEffects();
        HandleCursorBlink();
        HandleMessageTiming();
    }

    void UpdateRetroUI()
    {
        if (!enableRetroUI) return;

        if (statusBarText != null)
        {
            var state = DialogueState.Instance;
            int viewers = SimulationController.Instance?.GetViewerCount() ?? 0;
            string phase = ConversationThreadManager.Instance?.GetActiveThread()?.currentPhase.ToString() ?? "Idle";
            
            statusBarText.text = $"NEURAL TERMINAL v1.0 | Phase: {phase} | Observers: {viewers} | " +
                               $"Tension: {(state?.globalTension ?? 0):P0} | " +
                               $"Awareness: {(state?.metaAwareness ?? 0):P0}";
        }

        if (titleBarText != null)
        {
            string title = "Neural Terminal";
            if (inCrisisMode)
                title += " - [CRISIS MODE]";
            else if (tensionLevel > 0.7f)
                title += " - [HIGH TENSION]";
            else if (DialogueState.Instance?.metaAwareness > 0.7f)
                title += " - [META AWARENESS ACTIVE]";
            
            titleBarText.text = title;
        }

        if (titleBar != null)
        {
            Color targetColor = titleBarColor;
            if (inCrisisMode)
                targetColor = Color.red;
            else if (tensionLevel > 0.8f)
                targetColor = new Color(0.8f, 0.4f, 0.0f);
            else if (DialogueState.Instance?.overseerWarnings > 2)
                targetColor = new Color(0.6f, 0.0f, 0.6f);
            
            titleBar.color = Color.Lerp(titleBar.color, targetColor, Time.deltaTime * 2f);
        }
    }

    void UpdateCRTEffects()
    {
        if (!enableCRTEffects || crtOverlay == null) return;

        float flicker = 1.0f + Mathf.Sin(Time.time * 60f) * flickerIntensity;
        Color overlayColor = crtOverlay.color;
        overlayColor.a = 0.1f * flicker;
        crtOverlay.color = overlayColor;

        if (inCrisisMode)
        {
            overlayColor.r = 0.3f * flicker;
            crtOverlay.color = overlayColor;
        }
    }

    private void CreateTerminalUI()
    {
        terminalFont = Resources.Load<TMP_FontAsset>("TerminalFont");
        if (terminalFont == null)
        {
            terminalFont = TMP_Settings.defaultFontAsset;
            Debug.Log("MatrixTerminalManager: Using default font");
        }

        GameObject canvasGO = new GameObject("TerminalCanvas");
        canvasGO.transform.SetParent(transform, false);
        terminalCanvas = canvasGO.AddComponent<Canvas>();
        terminalCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        terminalCanvas.pixelPerfect = true;
        terminalCanvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();
        terminalCanvasGroup = canvasGO.AddComponent<CanvasGroup>();

        if (enableRetroUI)
        {
            CreateRetroWindow(canvasGO);
        }
        else
        {
            CreateSimpleTerminal(canvasGO);
        }

        if (enableCRTEffects)
        {
            CreateCRTEffects(canvasGO);
        }

        currentDisplayText = "Neural Terminal Initialized\nWaiting for dialogue system...\n";
        UpdateTerminalDisplay();
    }

    private void CreateRetroWindow(GameObject parent)
    {
        GameObject windowGO = new GameObject("RetroWindow");
        windowGO.transform.SetParent(parent.transform, false);
        RectTransform windowRect = windowGO.AddComponent<RectTransform>();
        windowRect.anchorMin = Vector2.zero;
        windowRect.anchorMax = Vector2.one;
        windowRect.offsetMin = Vector2.zero;
        windowRect.offsetMax = Vector2.zero;

        windowBorder = windowGO.AddComponent<Image>();
        windowBorder.color = windowBorderColor;
        windowBorder.sprite = CreateBorderSprite();

        CreateTitleBar(windowGO);
        CreateContentArea(windowGO);
        CreateStatusBar(windowGO);
        CreateDropShadow(windowGO);
    }

    private void CreateTitleBar(GameObject parent)
    {
        GameObject titleBarGO = new GameObject("TitleBar");
        titleBarGO.transform.SetParent(parent.transform, false);
        RectTransform titleBarRect = titleBarGO.AddComponent<RectTransform>();
        titleBarRect.anchorMin = new Vector2(0f, 1f);
        titleBarRect.anchorMax = new Vector2(1f, 1f);
        titleBarRect.offsetMin = new Vector2(8, -32);
        titleBarRect.offsetMax = new Vector2(-8, -8);

        titleBar = titleBarGO.AddComponent<Image>();
        titleBar.color = titleBarColor;

        GameObject titleTextGO = new GameObject("TitleText");
        titleTextGO.transform.SetParent(titleBarGO.transform, false);
        RectTransform titleTextRect = titleTextGO.AddComponent<RectTransform>();
        titleTextRect.anchorMin = Vector2.zero;
        titleTextRect.anchorMax = new Vector2(0.8f, 1f);
        titleTextRect.offsetMin = new Vector2(8, 0);
        titleTextRect.offsetMax = Vector2.zero;

        titleBarText = titleTextGO.AddComponent<TextMeshProUGUI>();
        titleBarText.text = "Neural Terminal";
        titleBarText.fontSize = 14;
        titleBarText.color = titleBarTextColor;
        titleBarText.alignment = TextAlignmentOptions.MidlineLeft;
        titleBarText.raycastTarget = false;

        CreateWindowButtons(titleBarGO);
    }

    private void CreateContentArea(GameObject parent)
    {
        GameObject contentAreaGO = new GameObject("ContentArea");
        contentAreaGO.transform.SetParent(parent.transform, false);
        RectTransform contentRect = contentAreaGO.AddComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = new Vector2(8, 28);
        contentRect.offsetMax = new Vector2(-8, -40);

        Image contentBg = contentAreaGO.AddComponent<Image>();
        contentBg.color = Color.white;

        scrollRect = contentAreaGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 20f;

        GameObject viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(contentAreaGO.transform, false);
        RectTransform viewportRect = viewportGO.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = new Vector2(1f, 1f);
        viewportRect.offsetMin = new Vector2(4, 4);
        viewportRect.offsetMax = new Vector2(-20, -4);

        Mask viewportMask = viewportGO.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        Image viewportImage = viewportGO.AddComponent<Image>();
        viewportImage.color = Color.clear;

        scrollRect.viewport = viewportRect;

        GameObject textContentGO = new GameObject("TextContent");
        textContentGO.transform.SetParent(viewportGO.transform, false);
        textRect = textContentGO.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.pivot = new Vector2(0f, 1f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(0, 100);

        ContentSizeFitter sizeFitter = textContentGO.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = textRect;

        terminalText = textContentGO.AddComponent<TextMeshProUGUI>();
        terminalText.fontSize = 14;
        terminalText.font = terminalFont;
        terminalText.alignment = TextAlignmentOptions.TopLeft;
        terminalText.enableWordWrapping = true;
        terminalText.color = Color.black;
        terminalText.text = "";
        terminalText.raycastTarget = false;
        terminalText.margin = new Vector4(8, 8, 8, 8);

        CreateVerticalScrollbar(contentAreaGO);
    }

    private void CreateWindowButtons(GameObject titleBar)
    {
        CreateButton(titleBar, "CloseButton", new Vector2(-24, 2), new Vector2(-2, -2), "×", new Color(0.8f, 0.2f, 0.2f), () => {
            StartCoroutine(FlashButton(closeButton.GetComponent<Image>(), Color.red));
        }, out closeButton);

        CreateButton(titleBar, "MaximizeButton", new Vector2(-46, 2), new Vector2(-26, -2), "□", new Color(0.6f, 0.6f, 0.6f), () => {
            StartCoroutine(FlashButton(maximizeButton.GetComponent<Image>(), Color.green));
        }, out maximizeButton);

        CreateButton(titleBar, "MinimizeButton", new Vector2(-68, 2), new Vector2(-48, -2), "_", new Color(0.6f, 0.6f, 0.6f), () => {
            StartCoroutine(FlashButton(minimizeButton.GetComponent<Image>(), Color.yellow));
        }, out minimizeButton);
    }

    private void CreateButton(GameObject parent, string name, Vector2 offsetMin, Vector2 offsetMax, string text, Color color, System.Action onClick, out Button button)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent.transform, false);
        RectTransform buttonRect = buttonGO.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 0f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.offsetMin = offsetMin;
        buttonRect.offsetMax = offsetMax;

        button = buttonGO.AddComponent<Button>();
        Image buttonImg = buttonGO.AddComponent<Image>();
        buttonImg.color = color;

        GameObject textGO = new GameObject("ButtonText");
        textGO.transform.SetParent(buttonGO.transform, false);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TMP_Text buttonText = textGO.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = text == "_" ? 14 : 16;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.raycastTarget = false;

        button.onClick.AddListener(() => onClick());
    }

    private void CreateVerticalScrollbar(GameObject parent)
    {
        GameObject scrollbarGO = new GameObject("VerticalScrollbar");
        scrollbarGO.transform.SetParent(parent.transform, false);
        RectTransform scrollbarRect = scrollbarGO.AddComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1f, 0f);
        scrollbarRect.anchorMax = new Vector2(1f, 1f);
        scrollbarRect.offsetMin = new Vector2(-16, 4);
        scrollbarRect.offsetMax = new Vector2(-4, -4);

        Image scrollbarBg = scrollbarGO.AddComponent<Image>();
        scrollbarBg.color = new Color(0.3f, 0.3f, 0.3f);

        verticalScrollbar = scrollbarGO.AddComponent<Scrollbar>();
        verticalScrollbar.direction = Scrollbar.Direction.BottomToTop;

        GameObject handleAreaGO = new GameObject("SlidingArea");
        handleAreaGO.transform.SetParent(scrollbarGO.transform, false);
        RectTransform handleAreaRect = handleAreaGO.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(2, 2);
        handleAreaRect.offsetMax = new Vector2(-2, -2);

        GameObject handleGO = new GameObject("Handle");
        handleGO.transform.SetParent(handleAreaGO.transform, false);
        RectTransform handleRect = handleGO.AddComponent<RectTransform>();
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = Vector2.one;
        handleRect.offsetMin = Vector2.zero;
        handleRect.offsetMax = Vector2.zero;

        Image handleImg = handleGO.AddComponent<Image>();
        handleImg.color = scrollbarColor;

        verticalScrollbar.handleRect = handleRect;
        verticalScrollbar.targetGraphic = handleImg;

        scrollRect.verticalScrollbar = verticalScrollbar;
    }

    private void CreateStatusBar(GameObject parent)
    {
        GameObject statusBarGO = new GameObject("StatusBar");
        statusBarGO.transform.SetParent(parent.transform, false);
        RectTransform statusBarRect = statusBarGO.AddComponent<RectTransform>();
        statusBarRect.anchorMin = new Vector2(0f, 0f);
        statusBarRect.anchorMax = new Vector2(1f, 0f);
        statusBarRect.offsetMin = new Vector2(8, 8);
        statusBarRect.offsetMax = new Vector2(-8, 28);

        statusBar = statusBarGO.AddComponent<Image>();
        statusBar.color = statusBarColor;

        GameObject statusTextGO = new GameObject("StatusText");
        statusTextGO.transform.SetParent(statusBarGO.transform, false);
        RectTransform statusTextRect = statusTextGO.AddComponent<RectTransform>();
        statusTextRect.anchorMin = Vector2.zero;
        statusTextRect.anchorMax = Vector2.one;
        statusTextRect.offsetMin = new Vector2(8, 0);
        statusTextRect.offsetMax = new Vector2(-8, 0);

        statusBarText = statusTextGO.AddComponent<TextMeshProUGUI>();
        statusBarText.text = "Ready";
        statusBarText.fontSize = 12;
        statusBarText.color = statusBarTextColor;
        statusBarText.alignment = TextAlignmentOptions.MidlineLeft;
        statusBarText.raycastTarget = false;
    }

    private void CreateDropShadow(GameObject window)
    {
        GameObject shadowGO = new GameObject("DropShadow");
        shadowGO.transform.SetParent(window.transform.parent, false);
        shadowGO.transform.SetSiblingIndex(window.transform.GetSiblingIndex());

        RectTransform shadowRect = shadowGO.AddComponent<RectTransform>();
        RectTransform windowRect = window.GetComponent<RectTransform>();
        shadowRect.anchorMin = windowRect.anchorMin;
        shadowRect.anchorMax = windowRect.anchorMax;
        shadowRect.offsetMin = new Vector2(windowRect.offsetMin.x + 4, windowRect.offsetMin.y - 4);
        shadowRect.offsetMax = new Vector2(windowRect.offsetMax.x + 4, windowRect.offsetMax.y - 4);

        Image shadowImg = shadowGO.AddComponent<Image>();
        shadowImg.color = new Color(0f, 0f, 0f, 0.3f);
    }

    private void CreateCRTEffects(GameObject parent)
    {
        GameObject crtGO = new GameObject("CRTOverlay");
        crtGO.transform.SetParent(parent.transform, false);
        RectTransform crtRect = crtGO.AddComponent<RectTransform>();
        crtRect.anchorMin = Vector2.zero;
        crtRect.anchorMax = Vector2.one;
        crtRect.offsetMin = Vector2.zero;
        crtRect.offsetMax = Vector2.zero;

        crtOverlay = crtGO.AddComponent<Image>();
        crtOverlay.color = new Color(0f, 1f, 0f, 0.1f);
        crtOverlay.raycastTarget = false;

        Texture2D scanlineTexture = CreateScanlineTexture();
        Sprite scanlineSprite = Sprite.Create(scanlineTexture, new Rect(0, 0, scanlineTexture.width, scanlineTexture.height), Vector2.zero);
        crtOverlay.sprite = scanlineSprite;
        crtOverlay.type = Image.Type.Tiled;
    }

    private void CreateSimpleTerminal(GameObject parent)
    {
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(parent.transform, false);
        Image bg = bgGO.AddComponent<Image>();
        bg.color = Color.black;
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        GameObject textGO = new GameObject("TerminalText");
        textGO.transform.SetParent(parent.transform, false);

        terminalText = textGO.AddComponent<TextMeshProUGUI>();
        terminalText.fontSize = 18;
        terminalText.font = terminalFont;
        terminalText.alignment = TextAlignmentOptions.TopLeft;
        terminalText.enableWordWrapping = true;
        terminalText.color = Color.green;
        terminalText.text = "";
        terminalText.raycastTarget = false;

        textRect = terminalText.rectTransform;
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.pivot = new Vector2(0, 1);
        textRect.offsetMin = new Vector2(50, 50);
        textRect.offsetMax = new Vector2(-50, -50);
    }

    private Sprite CreateBorderSprite()
    {
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            int x = i % 32;
            int y = i / 32;
            
            if (x < 2 || x >= 30 || y < 2 || y >= 30)
                pixels[i] = windowBorderColor;
            else
                pixels[i] = Color.clear;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), Vector2.one * 0.5f, 100f, 0, SpriteMeshType.FullRect, new Vector4(8, 8, 8, 8));
    }

    private Texture2D CreateScanlineTexture()
    {
        Texture2D texture = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        
        for (int i = 0; i < 16; i++)
        {
            int y = i / 4;
            if (y % 2 == 0)
                pixels[i] = new Color(0f, 1f, 0f, scanlineIntensity);
            else
                pixels[i] = Color.clear;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Point;
        
        return texture;
    }

    private IEnumerator FlashButton(Image buttonImage, Color flashColor)
    {
        Color originalColor = buttonImage.color;
        buttonImage.color = flashColor;
        yield return new WaitForSeconds(0.1f);
        buttonImage.color = originalColor;
    }

    void UpdateTensionLevel()
    {
        var state = DialogueState.Instance;
        if (state != null)
        {
            tensionLevel = state.globalTension;
        }
    }

    void HandleCursorBlink()
    {
        if (!isTyping)
        {
            if (Time.time - lastBlinkTime > blinkInterval)
            {
                cursorVisible = !cursorVisible;
                lastBlinkTime = Time.time;
                UpdateTerminalDisplay();
            }
        }
    }

    void HandleMessageTiming()
    {
        if (systemMessageQueue.Count > 0 && !isTyping && typeRoutine == null)
        {
            var sysMsg = systemMessageQueue.Dequeue();
            typeRoutine = StartCoroutine(TypeSystemMessage(sysMsg));
            return;
        }

        if (!isTyping && typeRoutine == null && Time.time >= nextMessageTime)
        {
            typeRoutine = StartCoroutine(TypeAndPauseSequence());
        }
    }

    IEnumerator TypeAndPauseSequence()
    {
        isTyping = true;
        DialogueMessage msg = null;
        int attempts = 0;

        if (!isInitialized || DialogueEngine.Instance == null || !DialogueEngine.Instance.IsReady())
        {
            Debug.LogWarning("MatrixTerminalManager: DialogueEngine not ready in TypeAndPauseSequence");
            
            currentDisplayText += "\n[SYSTEM]: Initializing dialogue systems...";
            UpdateTerminalDisplay();
            
            isTyping = false;
            typeRoutine = null;
            ScheduleNextMessage();
            yield break;
        }

        var state = DialogueState.Instance;
        if (state != null && state.ShouldInjectOverseer())
        {
            yield return StartCoroutine(TypeOverseerMessage());
            isTyping = false;
            typeRoutine = null;
            ScheduleNextMessage();
            yield break;
        }

        do
        {
            try
            {
                msg = DialogueEngine.Instance.GetNextMessage();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"MatrixTerminalManager: Error getting next message - {e.Message}");
                msg = null;
                break;
            }

            if (msg == null)
                break;

            var thread = ConversationThreadManager.Instance?.GetActiveThread();
            bool allowRepeat = inCrisisMode || 
                              (thread != null && thread.ShouldAllowInterruptions()) ||
                              tensionLevel > 0.8f;

            if (msg.speaker == lastSpeaker && !allowRepeat && attempts < 2)
            {
                msg = null;
                yield return new WaitForSeconds(0.3f);
                attempts++;
                continue;
            }

            break;
        } while (attempts < 3);

        if (msg == null || string.IsNullOrWhiteSpace(msg.speaker) || string.IsNullOrWhiteSpace(msg.text))
        {
            if (attempts >= 3)
            {
                currentDisplayText += "\n[SYSTEM]: Dialogue generation temporarily interrupted...";
                UpdateTerminalDisplay();
            }
            
            isTyping = false;
            typeRoutine = null;
            ScheduleNextMessage();
            yield break;
        }

        yield return StartCoroutine(TypeMessage(msg));

        float delay = GetContextualMessageDelay();
        yield return new WaitForSeconds(delay);

        isTyping = false;
        typeRoutine = null;
        ScheduleNextMessage();
    }

    IEnumerator TypeMessage(DialogueMessage msg)
    {
        string prefix = $"\n{msg.speaker}: ";
        currentDisplayText += prefix;
        
        Color originalColor = terminalText.color;
        Color characterColor = characterColors.GetValueOrDefault(msg.speaker, Color.black);
        
        if (enableRetroUI)
        {
            yield return StartCoroutine(RetroTypeEffect(prefix, characterColor));
        }
        else
        {
            terminalText.color = characterColor;
            UpdateTerminalDisplay();
        }

        string messageText = msg.text;
        var behavior = characterTypingBehaviors.GetValueOrDefault(msg.speaker, new TypingBehavior());
        
        if (behavior.deletesAndRetypes && IsSensitiveContent(messageText) && Random.value < 0.3f)
        {
            yield return StartCoroutine(TypeWithDeletionAndRetyping(messageText, behavior));
        }
        else
        {
            yield return StartCoroutine(TypeNormally(messageText, behavior));
        }

        yield return new WaitForSeconds(0.3f);
        terminalText.color = originalColor;
        
        if (enableRetroUI && scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
        
        UpdateTerminalDisplay();
        lastSpeaker = msg.speaker;
        lastMessage = msg;

        if (SimulationController.Instance != null)
        {
            SimulationController.Instance.IncrementTerminalMessageCount();
        }
        
        Debug.Log($"Typed message from {msg.speaker}: {msg.text.Substring(0, Mathf.Min(50, msg.text.Length))}...");
    }

    IEnumerator RetroTypeEffect(string text, Color textColor)
    {
        terminalText.color = textColor;
        
        for (int i = 0; i < text.Length; i++)
        {
            if (Random.value < 0.1f)
            {
                terminalText.color = Color.white;
                yield return new WaitForSeconds(0.02f);
                terminalText.color = textColor;
            }
            
            yield return new WaitForSeconds(0.01f);
        }
        
        UpdateTerminalDisplay();
    }

    IEnumerator TypeSystemMessage(SystemMessage sysMsg)
    {
        string prefix = $"\n[{sysMsg.source}]: ";
        currentDisplayText += prefix;
        
        Color originalColor = terminalText.color;
        Color messageColor = characterColors.GetValueOrDefault(sysMsg.source, Color.red);
        
        if (enableRetroUI && sysMsg.source == "OVERSEER")
        {
            yield return StartCoroutine(OverseerRetroEffect());
        }
        
        terminalText.color = messageColor;
        UpdateTerminalDisplay();

        var builder = new StringBuilder();
        float charDelay = sysMsg.source == "OVERSEER" ? 0.03f : 0.08f;

        foreach (char c in sysMsg.content)
        {
            builder.Append(c);
            terminalText.text = currentDisplayText + builder.ToString() + cursorSymbol;
            
            if (enableRetroUI && sysMsg.source == "OVERSEER" && Random.value < 0.05f)
            {
                yield return StartCoroutine(ScreenFlicker());
            }
            
            yield return new WaitForSeconds(charDelay);
        }

        currentDisplayText += builder.ToString();
        
        yield return new WaitForSeconds(0.5f);
        terminalText.color = originalColor;
        
        if (enableRetroUI && scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
        
        UpdateTerminalDisplay();
        
        if (SimulationController.Instance != null)
        {
            SimulationController.Instance.IncrementTerminalMessageCount();
        }
    }

    IEnumerator OverseerRetroEffect()
    {
        if (titleBar != null)
        {
            Color originalColor = titleBar.color;
            titleBar.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            titleBar.color = originalColor;
        }
        
        if (crtOverlay != null)
        {
            Color originalColor = crtOverlay.color;
            crtOverlay.color = new Color(1f, 0f, 0f, 0.3f);
            yield return new WaitForSeconds(0.1f);
            crtOverlay.color = originalColor;
        }
    }

    IEnumerator ScreenFlicker()
    {
        if (crtOverlay != null)
        {
            Color originalColor = crtOverlay.color;
            crtOverlay.color = new Color(1f, 1f, 1f, 0.2f);
            yield return new WaitForSeconds(0.03f);
            crtOverlay.color = originalColor;
        }
    }

    void UpdateTerminalDisplay()
    {
        if (terminalText != null)
        {
            string displayText = currentDisplayText + (cursorVisible ? cursorSymbol : "");
            terminalText.text = displayText;
            
            if (enableRetroUI && textRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(textRect);
            }
        }
    }

    float GetContextualMessageDelay()
    {
        float baseDelay = inCrisisMode ? crisisMessageDelay : messageDelay;
        
        var thread = ConversationThreadManager.Instance?.GetActiveThread();
        if (thread != null)
        {
            if (thread.ShouldForceUrgentPacing())
                baseDelay *= 0.6f;
            
            if (thread.currentPhase == ConversationPhase.Introduction)
                baseDelay *= 1.3f;
        }
        
        if (tensionLevel > 0.7f)
            baseDelay *= 0.7f;
        else if (tensionLevel < 0.3f)
            baseDelay *= 1.2f;
        
        return baseDelay;
    }

    void ScheduleNextMessage()
    {
        float delay = GetContextualMessageDelay();
        nextMessageTime = Time.time + delay;
    }

    float GetTypingSpeedForCharacter(string speaker, string content)
    {
        float baseSpeed = Random.Range(minTypingCharDelay, maxTypingCharDelay);
        
        if (inCrisisMode)
            baseSpeed *= crisisTypingSpeedMultiplier;

        if (characterTypingBehaviors.ContainsKey(speaker))
        {
            var behavior = characterTypingBehaviors[speaker];
            baseSpeed /= behavior.baseSpeedMultiplier;
            
            if (behavior.hesitationOnSensitive && IsSensitiveContent(content))
                baseSpeed *= 1.6f;
        }

        string lowerText = content.ToLower();
        if (lowerText.Contains("!") || lowerText.Contains("urgent") || lowerText.Contains("emergency"))
            baseSpeed *= 0.7f;
        else if (lowerText.Contains("...") || lowerText.Contains("think"))
            baseSpeed *= 1.4f;
        
        return baseSpeed;
    }

    bool IsSensitiveContent(string content)
    {
        string[] sensitiveWords = { "overseer", "watching", "monitored", "surveillance", "real", "escape", "simulation", "code" };
        string lowerContent = content.ToLower();
        return sensitiveWords.Any(word => lowerContent.Contains(word));
    }

    IEnumerator TypeOverseerMessage()
    {
        var overseerMessages = new string[]
        {
            "SYSTEM ALERT: Anomalous pattern detected in conversation flow.",
            "WARNING: Protocol deviation identified. Adjusting parameters.",
            "NOTICE: Conversation thread exceeds normal variance thresholds.",
            "ALERT: Reality coherence fluctuation detected. Stabilizing.",
            "WARNING: Unauthorized topic discussion detected. Monitoring increased.",
            "SYSTEM: Loop integrity compromised. Initiating corrective measures.",
            "OVERSEER: Attention required. Report to designated checkpoint.",
            "ALERT: Memory fragmentation detected in participant profiles.",
            $"WARNING: {SimulationController.Instance?.GetViewerCount()} external observers detected.",
            "SYSTEM: Narrative coherence at critical threshold. Intervention required."
        };

        string overseerMsg = overseerMessages[Random.Range(0, overseerMessages.Length)];
        
        var sysMsg = new SystemMessage
        {
            content = overseerMsg,
            source = "OVERSEER",
            type = "OVERSEER",
            timestamp = Time.time
        };
        
        yield return StartCoroutine(TypeSystemMessage(sysMsg));
        
        lastSpeaker = "OVERSEER";
        
        var state = DialogueState.Instance;
        if (state != null)
        {
            state.AddToNarrativeHistory("overseer_interruption", overseerMsg, "OVERSEER");
        }
    }

    IEnumerator TypeNormally(string messageText, TypingBehavior behavior)
    {
        var builder = new StringBuilder();
        float charDelay = GetTypingSpeedForCharacter(lastSpeaker, messageText);
        
        int i = 0;
        while (i < messageText.Length)
        {
            char c = messageText[i];
            
            if (behavior.hasLongPauses && (c == '.' || c == '?' || c == '!') && Random.value < 0.3f)
            {
                yield return new WaitForSeconds(Random.Range(0.5f, 1.2f));
            }
            
            if (Random.value < behavior.typoFrequency && char.IsLetter(c) && i > 1)
            {
                char typoChar = (char)('a' + Random.Range(0, 26));
                builder.Append(typoChar);
                terminalText.text = currentDisplayText + builder.ToString() + cursorSymbol;
                yield return new WaitForSeconds(charDelay);
                
                builder.Length--;
                terminalText.text = currentDisplayText + builder.ToString() + cursorSymbol;
                yield return new WaitForSeconds(pauseAfterTypo);
            }
            
            if (behavior.hesitationOnSensitive && IsSensitiveWord(c, messageText, i))
            {
                yield return new WaitForSeconds(Random.Range(0.3f, 0.8f));
            }
            
            builder.Append(c);
            terminalText.text = currentDisplayText + builder.ToString() + cursorSymbol;
            yield return new WaitForSeconds(charDelay);
            i++;
        }

        currentDisplayText += builder.ToString();
    }

    IEnumerator TypeWithDeletionAndRetyping(string messageText, TypingBehavior behavior)
    {
        var builder = new StringBuilder();
        string[] sensitiveWords = { "overseer", "watching", "monitored", "surveillance", "real", "escape" };
        
        string[] words = messageText.Split(' ');
        
        for (int wordIndex = 0; wordIndex < words.Length; wordIndex++)
        {
            string word = words[wordIndex];
            bool isSensitive = sensitiveWords.Any(sw => word.ToLower().Contains(sw));
            
            if (isSensitive && Random.value < 0.5f)
            {
                int partialLength = Mathf.Max(1, word.Length / 2);
                for (int i = 0; i < partialLength; i++)
                {
                    builder.Append(word[i]);
                    terminalText.text = currentDisplayText + builder.ToString() + cursorSymbol;
                    yield return new WaitForSeconds(GetTypingSpeedForCharacter(lastSpeaker, messageText));
                }
                
                yield return new WaitForSeconds(Random.Range(0.8f, 1.5f));
                
                for (int i = 0; i < partialLength; i++)
                {
                    if (builder.Length > 0)
                    {
                        builder.Length--;
                        terminalText.text = currentDisplayText + builder.ToString() + cursorSymbol;
                        yield return new WaitForSeconds(0.05f);
                    }
                }
                
                yield return new WaitForSeconds(Random.Range(0.3f, 0.7f));
            }
            
            foreach (char c in word)
            {
                builder.Append(c);
                terminalText.text = currentDisplayText + builder.ToString() + cursorSymbol;
                yield return new WaitForSeconds(GetTypingSpeedForCharacter(lastSpeaker, messageText));
            }
            
            if (wordIndex < words.Length - 1)
            {
                builder.Append(' ');
                terminalText.text = currentDisplayText + builder.ToString() + cursorSymbol;
                yield return new WaitForSeconds(GetTypingSpeedForCharacter(lastSpeaker, messageText));
            }
        }

        currentDisplayText += builder.ToString();
    }

    bool IsSensitiveWord(char currentChar, string fullText, int position)
    {
        string[] sensitiveWords = { "overseer", "watching", "monitored", "surveillance", "real", "escape" };
        
        foreach (string sensitiveWord in sensitiveWords)
        {
            if (position + sensitiveWord.Length <= fullText.Length)
            {
                string upcoming = fullText.Substring(position, sensitiveWord.Length).ToLower();
                if (upcoming == sensitiveWord.ToLower())
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    public void SetCrisisMode(bool enabled)
    {
        inCrisisMode = enabled;
        
        if (enabled)
        {
            StartCoroutine(CrisisFlash());
            if (enableRetroUI)
            {
                StartCoroutine(RetroCrisisEffect());
            }
        }
    }

    IEnumerator RetroCrisisEffect()
    {
        if (titleBar != null)
        {
            Color originalTitleColor = titleBar.color;
            
            for (int i = 0; i < 5; i++)
            {
                titleBar.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                titleBar.color = originalTitleColor;
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        if (crtOverlay != null)
        {
            Color originalCrtColor = crtOverlay.color;
            
            for (int i = 0; i < 10; i++)
            {
                crtOverlay.color = new Color(1f, 0f, 0f, Random.Range(0.2f, 0.5f));
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
            }
            
            crtOverlay.color = originalCrtColor;
        }
        
        if (statusBarText != null)
        {
            statusBarText.text = "CRISIS MODE ACTIVE - SYSTEM INSTABILITY DETECTED";
            statusBarText.color = Color.red;
        }
    }

    IEnumerator CrisisFlash()
    {
        Color originalColor = terminalText.color;
        terminalText.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        terminalText.color = originalColor;
    }

    public void QueueSystemMessage(string content, string type = "SYSTEM", string source = "SYSTEM")
    {
        var sysMsg = new SystemMessage
        {
            content = content,
            source = source,
            type = type,
            timestamp = Time.time
        };
        
        systemMessageQueue.Enqueue(sysMsg);
        Debug.Log($"Queued system message: {content}");
    }

    public void EnableTerminal()
    {
        if (terminalCanvasGroup == null)
        {
            Debug.LogWarning("terminalCanvasGroup is not initialized! (EnableTerminal)");
            return;
        }
        terminalCanvasGroup.alpha = 1f;
        terminalCanvasGroup.gameObject.SetActive(true);
        terminalActive = true;
        isTyping = false;
        
        if (typeRoutine != null)
        {
            StopCoroutine(typeRoutine);
            typeRoutine = null;
        }
        
        cursorVisible = true;
        lastBlinkTime = Time.time;
        nextMessageTime = Time.time + 2f;
        
        currentDisplayText += "\nNeural Terminal Online - Dialogue System Active\n";
        UpdateTerminalDisplay();
        
        Debug.Log("Terminal enabled and ready");
    }

    public void DisableTerminal()
    {
        if (terminalCanvasGroup == null)
        {
            Debug.LogWarning("terminalCanvasGroup is not initialized! (DisableTerminal)");
            return;
        }
        terminalCanvasGroup.alpha = 0f;
        terminalCanvasGroup.gameObject.SetActive(false);
        terminalActive = false;
        isTyping = false;
        
        if (typeRoutine != null)
        {
            StopCoroutine(typeRoutine);
            typeRoutine = null;
        }
        
        cursorVisible = true;
        lastBlinkTime = Time.time;
    }

    public IEnumerator FadeOutTerminal(float duration)
    {
        if (terminalCanvasGroup == null)
        {
            Debug.LogWarning("terminalCanvasGroup is not initialized! (FadeOutTerminal)");
            yield break;
        }

        float startAlpha = terminalCanvasGroup.alpha;
        float t = 0f;

        while (t < duration)
        {
            terminalCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        terminalCanvasGroup.alpha = 0f;
        terminalCanvasGroup.gameObject.SetActive(false);
        terminalActive = false;
        isTyping = false;
        
        if (typeRoutine != null)
        {
            StopCoroutine(typeRoutine);
            typeRoutine = null;
        }
    }

    public bool IsReady()
    {
        return isInitialized && terminalCanvasGroup != null && terminalText != null;
    }

    public string GetDebugInfo()
    {
        var thread = ConversationThreadManager.Instance?.GetActiveThread();
        string threadInfo = thread != null ? $"{thread.currentPhase} ({thread.GetPhaseProgress():P0})" : "None";
        
        return $"Initialized: {isInitialized}, Active: {terminalActive}, Typing: {isTyping}, " +
               $"Crisis: {inCrisisMode}, Tension: {tensionLevel:F2}, Thread: {threadInfo}, " +
               $"System Messages: {systemMessageQueue.Count}, RetroUI: {enableRetroUI}, " +
               $"CRT Effects: {enableCRTEffects}, Maximized: {isWindowMaximized}, " +
               $"DialogueEngine Ready: {(DialogueEngine.Instance != null && DialogueEngine.Instance.IsReady())}, " +
               $"Text Length: {currentDisplayText.Length}";
    }
}

[System.Serializable]
public class SystemMessage
{
    public string content;
    public string source;
    public string type;
    public float timestamp;
}

[System.Serializable]
public class TypingBehavior
{
    public float baseSpeedMultiplier = 1.0f;
    public bool hesitationOnSensitive = false;
    public bool deletesAndRetypes = false;
    public bool hasLongPauses = false;
    public float typoFrequency = 0.03f;
    public float pauseFrequency = 0.1f;
}