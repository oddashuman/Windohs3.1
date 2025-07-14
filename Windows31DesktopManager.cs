using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// **Complete Code: Windows31DesktopManager**
/// Vision: The master script for creating and managing a stable, authentic, and
/// story-aware Windows 3.1 desktop environment. It handles the boot sequence,
/// icon and window management, audio integration, and launching all of Orion's
/// interactive programs (FileManager, Notepad, Solitaire, SystemMonitor).
/// </summary>
public class Windows31DesktopManager : MonoBehaviour
{
    public static Windows31DesktopManager Instance { get; private set; }

    public enum DesktopActivity { Idle, OrganizingFiles, ReviewingNotes, SystemMaintenance, Playing, Researching, WaitingForConnection, InConversation }
    public enum ProgramType { FileManager, Notepad, SystemMonitor, Terminal, Solitaire }

    [Header("Desktop Assets")]
    public Color desktopColor = new Color32(0, 128, 128, 255);
    public TMP_FontAsset windows31Font;
    public Texture2D[] iconTextures; // Assign in Inspector
    public AudioClip[] systemSounds; // Assign sounds like "startup", "click", "error", "card"

    [Header("Behavior")]
    public bool skipBootOnRestart = false;

    // Core Components
    private Canvas desktopCanvas;
    private Image desktopBackground;
    private AudioSource audioSource;
    private List<DesktopIcon> desktopIcons = new List<DesktopIcon>();
    private List<Window> openWindows = new List<Window>();
    private bool isReady = false;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(InitializeDesktop());
    }

    private IEnumerator InitializeDesktop()
    {
        Debug.Log("DESKMAN: Initializing Desktop Environment...");
        CreateDesktopCanvas();
        CreateDesktopBackground();
        CreateTaskbar();
        
        audioSource = gameObject.AddComponent<AudioSource>();

        yield return null;
        isReady = true;
        Debug.Log("DESKMAN: Desktop Ready with Audio.");
    }
    
    #region Public API & State Management
    public void StartBootSequence()
    {
        StartCoroutine(SimulateBoot());
    }

    public void ShowDesktop()
    {
        if (desktopCanvas != null)
        {
            desktopCanvas.gameObject.SetActive(true);
            foreach (var icon in desktopIcons) if(icon != null) icon.gameObject.SetActive(true);
            foreach (var window in openWindows) if(window != null) window.gameObject.SetActive(true);
        }
    }

    public void TriggerConversationMode()
    {
        foreach (var icon in desktopIcons) if(icon != null) icon.gameObject.SetActive(false);
        foreach (var window in openWindows) if(window != null) window.gameObject.SetActive(false);
    }
    
    public bool IsReady() => isReady;
    
    public DesktopIcon GetIcon(ProgramType type) => desktopIcons.FirstOrDefault(i => i != null && i.programType == type);
    
    public Window GetWindow(string title) => openWindows.FirstOrDefault(w => w != null && w.title.Contains(title));

    public Image GetDesktopBackground() => this.desktopBackground;
    #endregion

    #region Program Simulation
    public void LaunchProgram(ProgramType programType)
    {
        PlaySound("click");
        Window existingWindow = GetWindow(programType.ToString());
        if (existingWindow != null)
        {
            existingWindow.transform.SetAsLastSibling();
            PlaySound("error");
            return;
        }

        Window newWindow = CreateNewWindow(programType.ToString(), new Vector2(450, 350));
        
        GameObject contentGO = new GameObject(programType.ToString() + "Content");
        contentGO.transform.SetParent(newWindow.contentArea, false);
        var rt = contentGO.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        switch (programType)
        {
            case ProgramType.FileManager:
                contentGO.AddComponent<FileManager>().Initialize();
                break;
            case ProgramType.Notepad:
                var notepad = contentGO.AddComponent<Notepad>();
                notepad.Initialize();
                notepad.TypeText("Cycle 7 summary:\n\nThe Overseer signal spiked when we discussed the exit code. This is not a coincidence. Nova's skepticism is becoming predictable... almost scripted. Is she a failsafe?\n\nThe glitches are becoming more frequent. They align with our fears. Is the simulation listening?");
                break;
            case ProgramType.SystemMonitor:
                contentGO.AddComponent<SystemMonitor>().Initialize();
                break;
            case ProgramType.Solitaire:
                 newWindow.SetSize(new Vector2(500, 400));
                contentGO.AddComponent<Solitaire>().Initialize();
                break;
        }
    }
    #endregion

    #region Audio System
    public void PlaySound(string soundName)
    {
        if (audioSource == null) return;
        var sound = System.Array.Find(systemSounds, s => s.name.Equals(soundName, System.StringComparison.OrdinalIgnoreCase));
        if (sound != null)
        {
            audioSource.PlayOneShot(sound);
        }
        else
        {
            Debug.LogWarning($"DESKMAN: Sound '{soundName}' not found.");
        }
    }
    #endregion
    
    #region Glitch System Hooks
    public void TriggerScreenFlicker()
    {
        StartCoroutine(FlickerEffect());
    }

    private IEnumerator FlickerEffect()
    {
        if (desktopBackground == null) yield break;
        desktopBackground.color = Color.black;
        yield return new WaitForSeconds(0.05f);
        desktopBackground.color = desktopColor;
    }

    public Window GetRandomOpenWindow()
    {
        var activeWindows = openWindows.Where(w => w != null && w.gameObject.activeSelf).ToList();
        if (activeWindows.Count == 0) return null;
        return activeWindows[Random.Range(0, activeWindows.Count)];
    }
    #endregion

    #region UI Creation
    private void CreateDesktopCanvas()
    {
        GameObject canvasGO = new GameObject("Windows31_Canvas");
        canvasGO.transform.SetParent(this.transform);
        desktopCanvas = canvasGO.AddComponent<Canvas>();
        desktopCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        desktopCanvas.sortingOrder = -10;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1024, 768);
        
        canvasGO.AddComponent<GraphicRaycaster>();
    }

    private void CreateDesktopBackground()
    {
        GameObject bgGO = new GameObject("DesktopBackground");
        bgGO.transform.SetParent(desktopCanvas.transform, false);
        desktopBackground = bgGO.AddComponent<Image>();
        desktopBackground.color = desktopColor;
        
        var rt = bgGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    private void CreateTaskbar()
    {
        GameObject taskbarGO = new GameObject("Taskbar");
        taskbarGO.transform.SetParent(desktopCanvas.transform, false);
        var image = taskbarGO.AddComponent<Image>();
        image.color = new Color32(192, 192, 192, 255);

        var rt = taskbarGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0);
        rt.sizeDelta = new Vector2(0, 30); rt.anchoredPosition = new Vector2(0, 15);
    }
    
    private void CreateDesktopIcons()
    {
        Vector2 startPos = new Vector2(40, -40);
        float xOffset = 90;
        float yOffset = -100;
        
        CreateIcon(ProgramType.FileManager, "File Manager", new Vector2(startPos.x, startPos.y));
        CreateIcon(ProgramType.Notepad, "Research Notes", new Vector2(startPos.x, startPos.y + yOffset));
        CreateIcon(ProgramType.SystemMonitor, "SysMon", new Vector2(startPos.x, startPos.y + 2 * yOffset));
        CreateIcon(ProgramType.Terminal, "Terminal", new Vector2(startPos.x + xOffset, startPos.y));
        CreateIcon(ProgramType.Solitaire, "Solitaire", new Vector2(startPos.x + xOffset, startPos.y + yOffset));
    }

    private void CreateIcon(ProgramType type, string name, Vector2 position)
    {
        GameObject iconGO = new GameObject(name);
        iconGO.transform.SetParent(desktopCanvas.transform, false);
        var icon = iconGO.AddComponent<DesktopIcon>();
        icon.Initialize(name, type, position);
        icon.OnDoubleClick += () => LaunchProgram(type);
        desktopIcons.Add(icon);
    }
    
    private Window CreateNewWindow(string title, Vector2 size)
    {
        GameObject windowGO = new GameObject(title + " Window");
        windowGO.transform.SetParent(desktopCanvas.transform, false);
        var window = windowGO.AddComponent<Window>();
        window.Initialize(title, size, Random.Range(128, 512));
        openWindows.Add(window);
        window.OnClose += () => {
            openWindows.Remove(window);
            Destroy(window.gameObject);
        };
        return window;
    }
    
    private IEnumerator SimulateBoot()
    {
        desktopCanvas.gameObject.SetActive(false);
        PlaySound("boot");
        yield return new WaitForSeconds(skipBootOnRestart ? 0.5f : 4.0f);
        desktopCanvas.gameObject.SetActive(true);
        if (desktopIcons.Count == 0) CreateDesktopIcons();
        PlaySound("startup");
        SimulationController.Instance.OnBootComplete();
    }
    #endregion
}