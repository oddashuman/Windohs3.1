using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class DesktopIcon : MonoBehaviour, IPointerClickHandler
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
        CreateIconStructure();
    }

    void CreateIconStructure()
    {
        iconRect.sizeDelta = new Vector2(64, 80);

        iconBackground = gameObject.GetComponent<Image>();
        if (iconBackground == null)
        {
            iconBackground = gameObject.AddComponent<Image>();
        }
        iconBackground.color = normalColor;
        iconBackground.raycastTarget = true;

        GameObject iconImageGO = new GameObject("IconImage");
        iconImageGO.transform.SetParent(transform, false);
        iconImage = iconImageGO.AddComponent<Image>();
        iconImage.raycastTarget = false;
        RectTransform iconImageRect = iconImage.rectTransform;
        iconImageRect.anchorMin = new Vector2(0, 0.3f);
        iconImageRect.anchorMax = new Vector2(1, 1);
        iconImageRect.offsetMin = new Vector2(8, 0);
        iconImageRect.offsetMax = new Vector2(-8, -5);

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

    public void Initialize(string name, Windows31DesktopManager.ProgramType program, Vector2 position, TMP_FontAsset font, Texture2D texture)
    {
        iconName = name;
        programType = program;
        iconTexture = texture;
        iconRect.anchoredPosition = position;
        if (iconLabel != null)
        {
            iconLabel.text = iconName;
            iconLabel.font = font;
        }
        SetIconTexture(program);
    }

    void SetIconTexture(Windows31DesktopManager.ProgramType program)
    {
        if (iconImage != null && iconTexture != null)
        {
            Sprite iconSprite = Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height), Vector2.one * 0.5f);
            iconImage.sprite = iconSprite;
            iconImage.color = Color.white;
        }
        else if(iconImage != null)
        {
            Color iconColor = Color.white;
            switch (program)
            {
                case Windows31DesktopManager.ProgramType.FileManager: iconColor = new Color32(255, 255, 0, 255); break;
                case Windows31DesktopManager.ProgramType.Notepad: iconColor = new Color32(255, 255, 255, 255); break;
                case Windows31DesktopManager.ProgramType.SystemMonitor: iconColor = new Color32(128, 128, 128, 255); break;
                case Windows31DesktopManager.ProgramType.Terminal: iconColor = new Color32(0, 255, 0, 255); break;
                case Windows31DesktopManager.ProgramType.Solitaire: iconColor = new Color32(255, 0, 0, 255); break;
            }
            iconImage.color = iconColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        float currentTime = Time.time;
        if (currentTime - lastClickTime < doubleClickTime)
        {
            OnDoubleClick?.Invoke();
        }
        else
        {
            SetSelected(true);
            OnSingleClick?.Invoke();
        }
        lastClickTime = currentTime;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        iconBackground.color = selected ? selectedColor : normalColor;
        iconLabel.color = selected ? Color.white : Color.black;
    }
}