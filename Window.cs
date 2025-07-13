using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Fixed Window class with correct enum references
/// </summary>
public class Window : MonoBehaviour
{
    [Header("Window Properties")]
    public string title = "Window";
    public Vector2 size = new Vector2(400, 300);
    public int memoryUsage = 256; // KB
    public bool isResizable = true;
    public bool isMovable = true;
    public bool isMinimizable = true;

    [Header("Visual Style")]
    public Color borderColor = new Color32(128, 128, 128, 255);
    public Color titleBarColor = new Color32(0, 0, 128, 255);
    public Color titleBarActiveColor = new Color32(0, 0, 128, 255);
    public Color titleBarInactiveColor = new Color32(128, 128, 128, 255);
    public Color windowBackgroundColor = new Color32(192, 192, 192, 255);

    // UI Components
    private RectTransform windowRect;
    private Image windowBorder;
    private Image titleBar;
    private TMP_Text titleText;
    private Button closeButton;
    private Button minimizeButton;
    private Button maximizeButton;
    
    [HideInInspector] public RectTransform contentArea;
    
    // Window State
    private bool isActive = false;
    private bool isMaximized = false;
    private bool isDragging = false;
    private Vector2 originalSize;
    private Vector2 originalPosition;
    private Vector2 dragOffset;

    // Events
    public Action OnClose;
    public Action OnMinimize;
    public Action OnMaximize;
    public Action OnFocus;

    void Awake()
    {
        CreateWindowStructure();
        SetupEventHandlers();
    }

    public void Initialize(string windowTitle, Vector2 windowSize, int memUsage)
    {
        title = windowTitle;
        size = windowSize;
        memoryUsage = memUsage;
        
        if (titleText != null)
            titleText.text = title;
            
        SetSize(size);
        
        // Position randomly but keep on screen
        Vector2 position = new Vector2(
            UnityEngine.Random.Range(50, Screen.width - size.x - 50),
            UnityEngine.Random.Range(100, Screen.height - size.y - 100)
        );
        SetPosition(position);
    }

    void CreateWindowStructure()
    {
        // Main window container
        windowRect = GetComponent<RectTransform>();
        if (windowRect == null)
        {
            windowRect = gameObject.AddComponent<RectTransform>();
        }

        // Window border/background
        windowBorder = gameObject.AddComponent<Image>();
        windowBorder.color = windowBackgroundColor;
        windowBorder.raycastTarget = true;

        // Create title bar
        CreateTitleBar();
        
        // Create window buttons
        CreateWindowButtons();
        
        // Create content area
        CreateContentArea();
    }

    void CreateTitleBar()
    {
        GameObject titleBarGO = new GameObject("TitleBar");
        titleBarGO.transform.SetParent(transform, false);
        
        titleBar = titleBarGO.AddComponent<Image>();
        titleBar.color = titleBarActiveColor;
        titleBar.raycastTarget = true;
        
        RectTransform titleBarRect = titleBar.rectTransform;
        titleBarRect.anchorMin = new Vector2(0, 1);
        titleBarRect.anchorMax = new Vector2(1, 1);
        titleBarRect.offsetMin = new Vector2(2, -22);
        titleBarRect.offsetMax = new Vector2(-2, -2);

        // Title text
        GameObject titleTextGO = new GameObject("TitleText");
        titleTextGO.transform.SetParent(titleBarGO.transform, false);
        
        titleText = titleTextGO.AddComponent<TextMeshProUGUI>();
        titleText.text = title;
        titleText.fontSize = 11;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.MidlineLeft;
        titleText.raycastTarget = false;
        
        RectTransform titleTextRect = titleText.rectTransform;
        titleTextRect.anchorMin = Vector2.zero;
        titleTextRect.anchorMax = new Vector2(0.7f, 1);
        titleTextRect.offsetMin = new Vector2(4, 0);
        titleTextRect.offsetMax = new Vector2(0, 0);

        // Make title bar draggable
        if (isMovable)
        {
            WindowDragHandler dragHandler = titleBarGO.AddComponent<WindowDragHandler>();
            dragHandler.window = this;
        }
    }

    void CreateWindowButtons()
    {
        // Close button
        CreateWindowButton("Close", new Vector2(-18, 4), new Vector2(-4, -4), "×", 
            titleBarActiveColor, CloseWindow, out closeButton);
        
        if (isMinimizable)
        {
            // Minimize button  
            CreateWindowButton("Minimize", new Vector2(-34, 4), new Vector2(-20, -4), "_", 
                titleBarActiveColor, MinimizeWindow, out minimizeButton);
        }
        
        // Maximize button
        CreateWindowButton("Maximize", new Vector2(-50, 4), new Vector2(-36, -4), "□", 
            titleBarActiveColor, MaximizeWindow, out maximizeButton);
    }

    void CreateWindowButton(string name, Vector2 offsetMin, Vector2 offsetMax, 
                           string buttonText, Color bgColor, Action onClick, out Button button)
    {
        GameObject buttonGO = new GameObject(name + "Button");
        buttonGO.transform.SetParent(titleBar.transform, false);
        
        button = buttonGO.AddComponent<Button>();
        Image buttonImg = buttonGO.AddComponent<Image>();
        buttonImg.color = bgColor;
        
        RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1, 0);
        buttonRect.anchorMax = new Vector2(1, 1);
        buttonRect.offsetMin = offsetMin;
        buttonRect.offsetMax = offsetMax;

        // Button text
        GameObject textGO = new GameObject("ButtonText");
        textGO.transform.SetParent(buttonGO.transform, false);
        
        TMP_Text text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = buttonText;
        text.fontSize = buttonText == "_" ? 10 : 12;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;

        button.onClick.AddListener(() => onClick());
    }

    void CreateContentArea()
    {
        GameObject contentAreaGO = new GameObject("ContentArea");
        contentAreaGO.transform.SetParent(transform, false);
        
        // Background for content area
        Image contentBg = contentAreaGO.AddComponent<Image>();
        contentBg.color = Color.white;
        
        RectTransform contentAreaRect = contentAreaGO.GetComponent<RectTransform>();
        contentAreaRect.anchorMin = Vector2.zero;
        contentAreaRect.anchorMax = Vector2.one;
        contentAreaRect.offsetMin = new Vector2(2, 2);
        contentAreaRect.offsetMax = new Vector2(-2, -24);

        contentArea = contentAreaRect;
    }

    void SetupEventHandlers()
    {
        // Click handler for window focus
        WindowClickHandler clickHandler = gameObject.AddComponent<WindowClickHandler>();
        clickHandler.window = this;
    }

    #region Public Methods

    public void SetSize(Vector2 newSize)
    {
        size = newSize;
        windowRect.sizeDelta = size;
    }

    public void SetPosition(Vector2 position)
    {
        windowRect.anchoredPosition = position;
    }

    public void SetContent(GameObject content)
    {
        if (content != null && contentArea != null)
        {
            content.transform.SetParent(contentArea, false);
            
            RectTransform contentRect = content.GetComponent<RectTransform>();
            if (contentRect != null)
            {
                contentRect.anchorMin = Vector2.zero;
                contentRect.anchorMax = Vector2.one;
                contentRect.offsetMin = contentRect.offsetMax = Vector2.zero;
            }
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;
        
        if (titleBar != null)
        {
            titleBar.color = active ? titleBarActiveColor : titleBarInactiveColor;
        }
        
        if (active)
        {
            OnFocus?.Invoke();
        }
    }

    public void CloseWindow()
    {
        OnClose?.Invoke();
    }

    public void MinimizeWindow()
    {
        gameObject.SetActive(false);
        OnMinimize?.Invoke();
    }

    public void MaximizeWindow()
    {
        if (!isMaximized)
        {
            originalSize = size;
            originalPosition = windowRect.anchoredPosition;
            
            SetSize(new Vector2(Screen.width - 20, Screen.height - 50));
            SetPosition(new Vector2(10, 25));
            
            isMaximized = true;
        }
        else
        {
            SetSize(originalSize);
            SetPosition(originalPosition);
            isMaximized = false;
        }
        
        OnMaximize?.Invoke();
    }

    public void StartDrag(Vector2 mousePosition)
    {
        if (!isMovable || isMaximized) return;
        
        isDragging = true;
        dragOffset = mousePosition - windowRect.anchoredPosition;
    }

    public void UpdateDrag(Vector2 mousePosition)
    {
        if (!isDragging) return;
        
        Vector2 newPosition = mousePosition - dragOffset;
        
        // Keep window on screen
        newPosition.x = Mathf.Clamp(newPosition.x, 0, Screen.width - size.x);
        newPosition.y = Mathf.Clamp(newPosition.y, 30, Screen.height - 30);
        
        SetPosition(newPosition);
    }

    public void EndDrag()
    {
        isDragging = false;
    }

    #endregion

    public bool IsActive => isActive;
    public bool IsDragging => isDragging;
}

/// <summary>
/// Handles window dragging behavior
/// </summary>
public class WindowDragHandler : MonoBehaviour, 
    UnityEngine.EventSystems.IBeginDragHandler,
    UnityEngine.EventSystems.IDragHandler,
    UnityEngine.EventSystems.IEndDragHandler
{
    public Window window;

    public void OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (window != null)
        {
            window.StartDrag(eventData.position);
        }
    }

    public void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (window != null)
        {
            window.UpdateDrag(eventData.position);
        }
    }

    public void OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (window != null)
        {
            window.EndDrag();
        }
    }
}

/// <summary>
/// Handles window focus clicks
/// </summary>
public class WindowClickHandler : MonoBehaviour,
    UnityEngine.EventSystems.IPointerClickHandler
{
    public Window window;

    public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (window != null)
        {
            window.SetActive(true);
        }
    }
}

/// <summary>
/// Desktop icon with authentic Windows 3.1 styling and behavior
/// </summary>
public class DesktopIcon : MonoBehaviour,
    UnityEngine.EventSystems.IPointerClickHandler
{
    [Header("Icon Properties")]
    public string iconName = "Icon";
    public Windows31DesktopManager.ProgramType programType;
    public Texture2D iconTexture;

    [Header("Visual Style")]
    public Color selectedColor = new Color32(0, 0, 128, 128);
    public Color normalColor = Color.clear;

    // UI Components
    private Image iconImage;
    private Image iconBackground;
    private TMP_Text iconLabel;
    private RectTransform iconRect;

    // State
    private bool isSelected = false;
    private float lastClickTime = 0f;
    private const float doubleClickTime = 0.3f;

    // Events
    public Action OnDoubleClick;
    public Action OnSingleClick;

    void Awake()
    {
        iconRect = GetComponent<RectTransform>();
        if (iconRect == null)
        {
            iconRect = gameObject.AddComponent<RectTransform>();
        }
    }

    void Start()
    {
        if (iconBackground == null)
        {
            CreateIconStructure();
        }
    }

    void CreateIconStructure()
    {
        if (iconRect == null)
        {
            iconRect = GetComponent<RectTransform>();
            if (iconRect == null)
            {
                iconRect = gameObject.AddComponent<RectTransform>();
            }
        }
        
        iconRect.sizeDelta = new Vector2(64, 80);

        // Background for selection
        iconBackground = gameObject.GetComponent<Image>();
        if (iconBackground == null)
        {
            iconBackground = gameObject.AddComponent<Image>();
        }
        iconBackground.color = normalColor;
        iconBackground.raycastTarget = true;

        // Icon image
        GameObject iconImageGO = new GameObject("IconImage");
        iconImageGO.transform.SetParent(transform, false);
        
        iconImage = iconImageGO.AddComponent<Image>();
        if (iconTexture != null)
        {
            Sprite iconSprite = Sprite.Create(iconTexture, 
                new Rect(0, 0, iconTexture.width, iconTexture.height), 
                Vector2.one * 0.5f);
            iconImage.sprite = iconSprite;
        }
        iconImage.raycastTarget = false;
        
        RectTransform iconImageRect = iconImage.rectTransform;
        iconImageRect.anchorMin = new Vector2(0, 0.3f);
        iconImageRect.anchorMax = new Vector2(1, 1);
        iconImageRect.offsetMin = new Vector2(8, 0);
        iconImageRect.offsetMax = new Vector2(-8, -5);

        // Icon label
        GameObject iconLabelGO = new GameObject("IconLabel");
        iconLabelGO.transform.SetParent(transform, false);
        
        iconLabel = iconLabelGO.AddComponent<TextMeshProUGUI>();
        iconLabel.text = iconName;
        iconLabel.fontSize = 10;
        iconLabel.color = Color.black;
        iconLabel.alignment = TextAlignmentOptions.Center;
        iconLabel.enableWordWrapping = true;
        iconLabel.raycastTarget = false;
        
        RectTransform iconLabelRect = iconLabel.rectTransform;
        iconLabelRect.anchorMin = new Vector2(0, 0);
        iconLabelRect.anchorMax = new Vector2(1, 0.3f);
        iconLabelRect.offsetMin = new Vector2(2, 0);
        iconLabelRect.offsetMax = new Vector2(-2, 0);
    }

    public void Initialize(string name, Windows31DesktopManager.ProgramType program, Vector2 position)
    {
        iconName = name;
        programType = program;
        
        if (iconRect == null)
        {
            iconRect = GetComponent<RectTransform>();
            if (iconRect == null)
            {
                iconRect = gameObject.AddComponent<RectTransform>();
            }
        }
        
        iconRect.anchoredPosition = position;
        
        if (iconBackground == null || iconLabel == null)
        {
            CreateIconStructure();
        }
        
        if (iconLabel != null)
        {
            iconLabel.text = iconName;
        }
        
        SetIconTexture(program);
    }

    void SetIconTexture(Windows31DesktopManager.ProgramType program)
    {
        Color iconColor = Color.white;
        
        switch (program)
        {
            case Windows31DesktopManager.ProgramType.FileManager:
                iconColor = new Color32(255, 255, 0, 255); // Yellow folder
                break;
            case Windows31DesktopManager.ProgramType.Notepad:
                iconColor = new Color32(255, 255, 255, 255); // White document
                break;
            case Windows31DesktopManager.ProgramType.SystemMonitor:
                iconColor = new Color32(128, 128, 128, 255); // Gray system
                break;
            case Windows31DesktopManager.ProgramType.Terminal:
                iconColor = new Color32(0, 255, 0, 255); // Green terminal
                break;
            case Windows31DesktopManager.ProgramType.Solitaire:
                iconColor = new Color32(255, 0, 0, 255); // Red cards
                break;
            case Windows31DesktopManager.ProgramType.Calculator:
                iconColor = new Color32(192, 192, 192, 255); // Gray calculator
                break;
        }
        
        if (iconImage != null)
        {
            iconImage.color = iconColor;
        }
    }

    public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        float currentTime = Time.time;
        
        if (currentTime - lastClickTime < doubleClickTime)
        {
            TriggerDoubleClick();
        }
        else
        {
            SetSelected(true);
            OnSingleClick?.Invoke();
        }
        
        lastClickTime = currentTime;
    }

    public void TriggerDoubleClick()
    {
        OnDoubleClick?.Invoke();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (iconBackground != null)
        {
            iconBackground.color = selected ? selectedColor : normalColor;
        }
        
        if (iconLabel != null)
        {
            iconLabel.color = selected ? Color.white : Color.black;
        }
    }

    public bool IsSelected => isSelected;
}