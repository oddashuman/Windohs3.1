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
    public enum ScreensaverMode { None, Mystify, FlyingWindows, Starfield }

    [Header("Desktop Assets")]
    public Color desktopColor = new Color32(0, 128, 128, 255);
    public TMP_FontAsset windows31Font;
    public Texture2D[] iconTextures;
    public AudioClip[] systemSounds;

    [Header("Behavior")]
    public bool skipBootOnRestart = false;
    public GameObject flyingWindowPrefab; // Assign a simple window prefab for the screensaver

    private Canvas desktopCanvas;
    private Image desktopBackground;
    private AudioSource audioSource;
    private List<DesktopIcon> desktopIcons = new List<DesktopIcon>();
    private List<Window> openWindows = new List<Window>();
    private bool isReady = false;
    private Coroutine screensaverCoroutine;
    private GameObject screensaverContainer;

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
            existingWindow.gameObject.SetActive(true);
            OnWindowFocused(existingWindow);
            return;
        }

        Window newWindow = CreateNewWindow(title, new Vector2(450, 350));
        GameObject contentGO = new GameObject(title + "Content");
        contentGO.transform.SetParent(newWindow.contentArea, false);
        var rt = contentGO.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        switch (programType)
        {
            case ProgramType.FileManager: contentGO.AddComponent<FileManager>().Initialize(); break;
            case ProgramType.Notepad:
                var notepad = contentGO.AddComponent<Notepad>();
                notepad.Initialize();
                notepad.TypeText("Cycle " + DialogueState.Instance.loopCount + " summary:\n\nThe Overseer signal spiked...");
                break;
            case ProgramType.SystemMonitor: contentGO.AddComponent<SystemMonitor>().Initialize(); break;
            case ProgramType.Solitaire:
                newWindow.size = new Vector2(500, 400);
                contentGO.AddComponent<Solitaire>().Initialize();
                break;
            case ProgramType.Terminal:
                MatrixTerminalManager.Instance.EnableTerminal();
                break;
        }
        SimulationController.Instance.ReportActivity();
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
        focusedWindow.transform.SetAsLastSibling();
    }

    public void SetScreensaver(bool active)
    {
        if (active)
        {
            if (screensaverCoroutine != null) StopCoroutine(screensaverCoroutine);
            screensaverCoroutine = StartCoroutine(RunScreensaver());
        }
        else
        {
            if (screensaverCoroutine != null) StopCoroutine(screensaverCoroutine);
            if (screensaverContainer != null) Destroy(screensaverContainer);
            CursorController.Instance.SetVisibility(true);
        }
    }

    private IEnumerator RunScreensaver()
    {
        CursorController.Instance.SetVisibility(false);
        if (screensaverContainer != null) Destroy(screensaverContainer);
        screensaverContainer = new GameObject("ScreensaverContainer");
        screensaverContainer.transform.SetParent(desktopCanvas.transform, false);

        ScreensaverMode mode = (ScreensaverMode)Random.Range(1, 4); // Pick a random screensaver
        Debug.Log($"Starting screensaver: {mode}");

        switch (mode)
        {
            case ScreensaverMode.Mystify:
                yield return StartCoroutine(MystifyScreensaver());
                break;
            case ScreensaverMode.FlyingWindows:
                yield return StartCoroutine(FlyingWindowsScreensaver());
                break;
            case ScreensaverMode.Starfield:
                yield return StartCoroutine(StarfieldScreensaver());
                break;
        }
    }

    private IEnumerator MystifyScreensaver()
    {
        // A simple implementation of the Mystify screensaver
        var lineRenderer = screensaverContainer.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.cyan;
        lineRenderer.startWidth = 2f;
        lineRenderer.endWidth = 2f;
        lineRenderer.positionCount = 2;

        Vector2 pos1 = new Vector2(Random.Range(0, Screen.width), Random.Range(0, Screen.height));
        Vector2 pos2 = new Vector2(Random.Range(0, Screen.width), Random.Range(0, Screen.height));
        Vector2 dir1 = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * 200f;
        Vector2 dir2 = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * 200f;

        while (true)
        {
            pos1 += dir1 * Time.deltaTime;
            pos2 += dir2 * Time.deltaTime;

            if (pos1.x < 0 || pos1.x > Screen.width) dir1.x *= -1;
            if (pos1.y < 0 || pos1.y > Screen.height) dir1.y *= -1;
            if (pos2.x < 0 || pos2.x > Screen.width) dir2.x *= -1;
            if (pos2.y < 0 || pos2.y > Screen.height) dir2.y *= -1;

            lineRenderer.SetPosition(0, pos1);
            lineRenderer.SetPosition(1, pos2);
            yield return null;
        }
    }

    private IEnumerator FlyingWindowsScreensaver()
    {
        // A simple implementation of the Flying Windows screensaver
        for (int i = 0; i < 10; i++)
        {
            var window = Instantiate(flyingWindowPrefab, screensaverContainer.transform);
            window.GetComponent<RectTransform>().anchoredPosition = new Vector2(Random.Range(0, Screen.width), Random.Range(0, Screen.height));
            var rb = window.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.linearDamping = 0;
            rb.angularDamping = 0;
            rb.linearVelocity = new Vector2(Random.Range(-100f, 100f), Random.Range(-100f, 100f));
        }
        yield return new WaitForSeconds(float.MaxValue); // Let the physics engine handle it
    }

    private IEnumerator StarfieldScreensaver()
    {
        // A simple implementation of the Starfield screensaver
        var particleSystem = screensaverContainer.AddComponent<ParticleSystem>();
        var main = particleSystem.main;
        main.startColor = Color.white;
        main.startSpeed = 200;
        main.startSize = 2;
        var emission = particleSystem.emission;
        emission.rateOverTime = 100;
        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 100;
        yield return new WaitForSeconds(float.MaxValue);
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

    public string GetDebugInfo()
    {
        return $"Ready: {isReady}, Icons: {desktopIcons.Count}, Open Windows: {openWindows.Count}";
    }

    public void BeginActivity(DesktopActivity activity)
    {
        Debug.Log($"Starting desktop activity: {activity}");
        DesktopAI.Instance.PerformActivity(activity);
    }
}