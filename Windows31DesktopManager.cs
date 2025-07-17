using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Windows31DesktopManager : MonoBehaviour
{
    public static Windows31DesktopManager Instance { get; private set; }

    public enum DesktopActivity { Idle, OrganizingFiles, ReviewingNotes, SystemMaintenance, Playing, Researching }
    public enum ProgramType { FileManager, Notepad, SystemMonitor, Terminal, Solitaire }

    [Header("Desktop Assets")]
    public Color desktopColor = new Color32(0, 128, 128, 255);
    public TMP_FontAsset windows31Font;
    public Texture2D[] iconTextures;
    public AudioClip[] systemSounds;

    [Header("Behavior")]
    public bool skipBootOnRestart = false;

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
        CreateDesktopCanvas();
        CreateDesktopBackground();
        CreateDesktopIcons();
        audioSource = gameObject.AddComponent<AudioSource>();
        yield return null;
        isReady = true;
        Debug.Log("DESKMAN: Desktop Ready.");
    }

    public bool IsReady() => isReady;
    public Image GetDesktopBackground() => desktopBackground;
    public DesktopIcon GetIcon(ProgramType type) => desktopIcons.FirstOrDefault(i => i.programType == type);
    public Window GetWindow(string title) => openWindows.FirstOrDefault(w => w.title.Contains(title));
    public Window GetRandomOpenWindow()
    {
        var activeWindows = openWindows.Where(w => w.gameObject.activeSelf).ToList();
        return activeWindows.Count == 0 ? null : activeWindows[Random.Range(0, activeWindows.Count)];
    }

    public void LaunchProgram(ProgramType programType)
    {
        PlaySound("click");
        string title = programType.ToString();
        var existingWindow = GetWindow(title);
        if (existingWindow != null)
        {
            existingWindow.SetActive(true);
            return;
        }

        Window newWindow = CreateNewWindow(title, new Vector2(450, 350));
        GameObject contentGO = new GameObject(title + "Content");
        contentGO.transform.SetParent(newWindow.contentArea, false);
        var rt = contentGO.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = newWindow.contentArea.rect.size;

        switch (programType)
        {
            case ProgramType.FileManager: contentGO.AddComponent<FileManager>().Initialize(); break;
            case ProgramType.Notepad:
                var notepad = contentGO.AddComponent<Notepad>();
                notepad.Initialize();
                notepad.TypeText("Cycle 7 summary:\n\nThe Overseer signal spiked...");
                break;
            case ProgramType.SystemMonitor: contentGO.AddComponent<SystemMonitor>().Initialize(); break;
            case ProgramType.Solitaire:
                newWindow.size = new Vector2(500, 400);
                contentGO.AddComponent<Solitaire>().Initialize();
                break;
        }
    }

    public void PlaySound(string soundName)
    {
        var sound = System.Array.Find(systemSounds, s => s.name.Equals(soundName, System.StringComparison.OrdinalIgnoreCase));
        if (sound != null) audioSource.PlayOneShot(sound);
    }

    public void TriggerScreenFlicker() => StartCoroutine(FlickerEffect());
    private IEnumerator FlickerEffect()
    {
        desktopBackground.color = Color.black;
        yield return new WaitForSeconds(0.05f);
        desktopBackground.color = desktopColor;
    }

    public void StartBootSequence() => StartCoroutine(SimulateBoot());
    private IEnumerator SimulateBoot()
    {
        desktopCanvas.gameObject.SetActive(false);
        PlaySound("boot");
        yield return new WaitForSeconds(skipBootOnRestart ? 0.2f : 3.0f);
        desktopCanvas.gameObject.SetActive(true);
        PlaySound("startup");
        if (SimulationController.Instance != null) SimulationController.Instance.OnBootComplete();
    }

    private void OnWindowFocused(Window focusedWindow)
    {
        foreach (var w in openWindows)
        {
            if (w != focusedWindow) w.SetActive(false);
        }
    }

    // --- UI Creation ---
    private void CreateDesktopCanvas()
    {
        GameObject canvasGO = new GameObject("Windows31_Canvas");
        canvasGO.transform.SetParent(this.transform);
        desktopCanvas = canvasGO.AddComponent<Canvas>();
        desktopCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();
    }

    private void CreateDesktopBackground()
    {
        GameObject bgGO = new GameObject("DesktopBackground");
        bgGO.transform.SetParent(desktopCanvas.transform, false);
        desktopBackground = bgGO.AddComponent<Image>();
        desktopBackground.color = desktopColor;
        RectTransform rt = bgGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private void CreateDesktopIcons()
    {
        CreateIcon(ProgramType.FileManager, "File Manager", new Vector2(50, -50));
        CreateIcon(ProgramType.Notepad, "Research Notes", new Vector2(50, -150));
        CreateIcon(ProgramType.SystemMonitor, "SysMon", new Vector2(50, -250));
        CreateIcon(ProgramType.Terminal, "Terminal", new Vector2(150, -50));
        CreateIcon(ProgramType.Solitaire, "Solitaire", new Vector2(150, -150));
    }

    // FIX: Modified CreateIcon to pass the correct texture to the DesktopIcon
    private void CreateIcon(ProgramType type, string name, Vector2 position)
    {
        GameObject iconGO = new GameObject(name);
        iconGO.transform.SetParent(desktopCanvas.transform, false);
        var icon = iconGO.AddComponent<DesktopIcon>();

        Texture2D iconTexture = null;
        if (iconTextures != null && (int)type < iconTextures.Length)
        {
            iconTexture = iconTextures[(int)type];
        }

        icon.Initialize(name, type, position, windows31Font, iconTexture);
        icon.OnDoubleClick += () => LaunchProgram(type);
        desktopIcons.Add(icon);
    }

    private Window CreateNewWindow(string title, Vector2 size)
    {
        GameObject windowGO = new GameObject(title + " Window");
        windowGO.transform.SetParent(desktopCanvas.transform, false);
        var window = windowGO.AddComponent<Window>();
        window.Initialize(title, size, windows31Font);
        openWindows.Add(window);
        window.OnClose += () => { openWindows.Remove(window); Destroy(window.gameObject); };
        window.OnFocus += () => { OnWindowFocused(window); };
        OnWindowFocused(window);
        return window;
    }
    
    // --- Missing Method Implementations ---
    public string GetDebugInfo()
    {
        return $"Ready: {isReady}, Icons: {desktopIcons.Count}, Open Windows: {openWindows.Count}";
    }
    
    public void BeginActivity(DesktopActivity activity)
    {
        Debug.Log($"Starting desktop activity: {activity}");
        // This would be the hook for the DesktopAI to start doing things.
    }
}