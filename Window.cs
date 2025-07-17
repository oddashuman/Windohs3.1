using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class Window : MonoBehaviour
{
    [Header("Window Properties")]
    public string title = "Window";
    public Vector2 size = new Vector2(400, 300);

    [Header("UI Components")]
    public Image titleBar;
    public Button closeButton;
    public Button minimizeButton;
    public Button maximizeButton;
    public RectTransform contentArea;

    private RectTransform windowRect;
    private bool isMaximized = false;
    private Vector2 originalSize;
    private Vector2 originalPosition;

    public Action OnClose;
    public Action OnMinimize;
    public Action OnMaximize;
    public Action OnFocus;

    void Awake()
    {
        windowRect = GetComponent<RectTransform>();
    }

    public void Initialize(string windowTitle, Vector2 windowSize, TMP_FontAsset font)
    {
        title = windowTitle;
        size = windowSize;
        if (windowRect == null) windowRect = gameObject.AddComponent<RectTransform>();
        windowRect.sizeDelta = size;
        CreateWindowStructure(font);
        windowRect.anchoredPosition = new Vector2(UnityEngine.Random.Range(50, Screen.width - size.x - 50), UnityEngine.Random.Range(100, Screen.height - size.y - 100));
    }

    void CreateWindowStructure(TMP_FontAsset font)
    {
        var bgImage = gameObject.AddComponent<Image>();
        bgImage.color = new Color32(192, 192, 192, 255);
        gameObject.AddComponent<WindowClickHandler>().window = this;

        GameObject titleBarGO = new GameObject("TitleBar");
        titleBarGO.transform.SetParent(transform, false);
        titleBar = titleBarGO.AddComponent<Image>();
        titleBar.color = new Color32(0, 0, 128, 255);
        titleBarGO.AddComponent<WindowDragHandler>().window = this;
        RectTransform titleBarRect = titleBar.rectTransform;
        titleBarRect.anchorMin = new Vector2(0, 1);
        titleBarRect.anchorMax = new Vector2(1, 1);
        titleBarRect.offsetMin = new Vector2(2, -22);
        titleBarRect.offsetMax = new Vector2(-2, -2);

        GameObject titleTextGO = new GameObject("TitleText");
        titleTextGO.transform.SetParent(titleBarGO.transform, false);
        var titleText = titleTextGO.AddComponent<TextMeshProUGUI>();
        titleText.text = title;
        titleText.font = font;
        titleText.fontSize = 11;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.MidlineLeft;
        RectTransform titleTextRect = titleText.rectTransform;
        titleTextRect.anchorMin = Vector2.zero;
        titleTextRect.anchorMax = new Vector2(1, 1);
        titleTextRect.offsetMin = new Vector2(4, 0);
        titleTextRect.offsetMax = new Vector2(-60, 0);

        GameObject contentAreaGO = new GameObject("ContentArea");
        contentAreaGO.transform.SetParent(transform, false);
        contentAreaGO.AddComponent<Image>().color = Color.white;
        contentArea = contentAreaGO.GetComponent<RectTransform>();
        contentArea.anchorMin = Vector2.zero;
        contentArea.anchorMax = Vector2.one;
        contentArea.offsetMin = new Vector2(2, 2);
        contentArea.offsetMax = new Vector2(-2, -24);

        closeButton = CreateWindowButton("Close", new Vector2(-18, -2), "x", font);
        minimizeButton = CreateWindowButton("Minimize", new Vector2(-34, -2), "_", font);
        maximizeButton = CreateWindowButton("Maximize", new Vector2(-50, -2), "[]", font);

        closeButton.onClick.AddListener(CloseWindow);
        minimizeButton.onClick.AddListener(MinimizeWindow);
        maximizeButton.onClick.AddListener(MaximizeWindow);
    }

    Button CreateWindowButton(string name, Vector2 anchoredPosition, string text, TMP_FontAsset font)
    {
        GameObject buttonGO = new GameObject(name + "Button");
        buttonGO.transform.SetParent(titleBar.transform, false);
        var button = buttonGO.AddComponent<Button>();
        var image = buttonGO.AddComponent<Image>();
        image.color = new Color32(192, 192, 192, 255);
        RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1, 1);
        buttonRect.anchorMax = new Vector2(1, 1);
        buttonRect.sizeDelta = new Vector2(16, 16);
        buttonRect.anchoredPosition = anchoredPosition;

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonRect, false);
        var tmpText = textGO.AddComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.font = font;
        tmpText.fontSize = 10;
        tmpText.color = Color.black;
        tmpText.alignment = TextAlignmentOptions.Center;
        RectTransform textRect = tmpText.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;

        return button;
    }

    public void SetActive(bool active)
    {
        titleBar.color = active ? new Color32(0, 0, 128, 255) : new Color32(128, 128, 128, 255);
        if (active)
        {
            transform.SetAsLastSibling();
            OnFocus?.Invoke();
        }
    }

    public void CloseWindow() => OnClose?.Invoke();
    public void MinimizeWindow()
    {
        gameObject.SetActive(false);
        OnMinimize?.Invoke();
    }
    public void MaximizeWindow()
    {
        if (!isMaximized)
        {
            originalSize = windowRect.sizeDelta;
            originalPosition = windowRect.anchoredPosition;
            windowRect.anchoredPosition = new Vector2(0, -15);
            windowRect.sizeDelta = new Vector2(Screen.width, Screen.height - 30);
            isMaximized = true;
        }
        else
        {
            windowRect.sizeDelta = originalSize;
            windowRect.anchoredPosition = originalPosition;
            isMaximized = false;
        }
        OnMaximize?.Invoke();
    }

    public void SetPosition(Vector2 position)
    {
        windowRect.anchoredPosition = position;
    }
}

public class WindowDragHandler : MonoBehaviour, IDragHandler
{
    public Window window;
    public void OnDrag(PointerEventData eventData) { if(window != null) window.transform.position += (Vector3)eventData.delta; }
}

public class WindowClickHandler : MonoBehaviour, IPointerDownHandler
{
    public Window window;
    public void OnPointerDown(PointerEventData eventData) { if(window != null) window.SetActive(true); }
}