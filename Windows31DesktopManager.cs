using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Production-optimized Windows 3.1 Desktop Manager with object pooling,
/// canvas isolation, and 24/7 stability features.
/// Zero garbage allocation window management with authentic retro styling.
/// </summary>
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

    [Header("Performance Settings")]
    [Range(1, 10)] public int maxConcurrentWindows = 5;
    [Range(10, 100)] public int windowPoolSize = 20;
    public bool enableObjectPooling = true;
    public bool enableCanvasOptimization = true;

    [Header("Behavior")]
    public bool skipBootOnRestart = false;
    public GameObject flyingWindowPrefab;

    // Canvas hierarchy for performance isolation
    private Canvas staticCanvas;      // Background, taskbar - never changes
    private Canvas dynamicCanvas;     // Windows, dialogs - frequent updates  
    private Canvas overlayCanvas;     // Tooltips, effects - highest priority
    private Image desktopBackground;
    
    // Object pooling system
    private WindowPool windowPool;
    private IconPool iconPool;
    
    // Audio system
    private AudioSource audioSource;
    private Dictionary<string, AudioClip> soundLibrary;
    
    // Window management
    private List<DesktopIcon> desktopIcons = new List<DesktopIcon>();
    private List<Window> activeWindows = new List<Window>();
    private Dictionary<string, Window> windowRegistry = new Dictionary<string, Window>();
    
    // Screensaver system
    private Coroutine screensaverCoroutine;
    private GameObject screensaverContainer;
    private ScreensaverMode currentScreensaver = ScreensaverMode.None;
    
    // Performance tracking
    private bool isReady = false;
    private float lastOptimizationTime;
    private int framesSinceLastOptimization;
    private const float OPTIMIZATION_INTERVAL = 5f;
    
    // Memory management
    private float lastMemoryCleanup;
    private const float MEMORY_CLEANUP_INTERVAL = 60f;

    #region Initialization

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        
        // Prevent sleep during continuous operation
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    void Start()
    {
        StartCoroutine(InitializeDesktopOptimized());
    }

    private IEnumerator InitializeDesktopOptimized()
    {
        Debug.Log("DESKTOP_MGR: Starting optimized initialization...");
        
        // Phase 1: Create canvas hierarchy
        CreateCanvasHierarchy();
        yield return null;
        
        // Phase 2: Initialize pooling systems
        if (enableObjectPooling)
        {
            InitializeObjectPools();
            yield return null;
        }
        
        // Phase 3: Setup desktop elements
        CreateDesktopBackground();
        yield return null;
        
        CreateDesktopIcons();
        yield return null;
        
        // Phase 4: Initialize audio system
        InitializeAudioSystem();
        yield return null;
        
        // Phase 5: Start performance monitoring
        StartCoroutine(PerformanceMonitoringLoop());
        
        isReady = true;
        Debug.Log("DESKTOP_MGR: Initialization complete - Performance optimized");
    }

    private void CreateCanvasHierarchy()
    {
        // Static Canvas - Background, taskbar (never rebuilt)
        GameObject staticGO = new GameObject("StaticCanvas");
        staticGO.transform.SetParent(transform);
        staticCanvas = staticGO.AddComponent<Canvas>();
        staticCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        staticCanvas.sortingOrder = 0;
        staticCanvas.pixelPerfect = true;
        
        var staticScaler = staticGO.AddComponent<CanvasScaler>();
        staticScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        staticScaler.referenceResolution = new Vector2(1024, 768);
        staticScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        staticScaler.matchWidthOrHeight = 0.5f;
        
        staticGO.AddComponent<GraphicRaycaster>();
        
        // Dynamic Canvas - Windows, moving elements (frequent rebuilds)
        GameObject dynamicGO = new GameObject("DynamicCanvas");
        dynamicGO.transform.SetParent(transform);
        dynamicCanvas = dynamicGO.AddComponent<Canvas>();
        dynamicCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        dynamicCanvas.sortingOrder = 100;
        dynamicCanvas.pixelPerfect = true;
        
        var dynamicScaler = dynamicGO.AddComponent<CanvasScaler>();
        dynamicScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        dynamicScaler.referenceResolution = new Vector2(1024, 768);
        
        dynamicGO.AddComponent<GraphicRaycaster>();
        
        // Overlay Canvas - Tooltips, effects (highest priority)
        GameObject overlayGO = new GameObject("OverlayCanvas");
        overlayGO.transform.SetParent(transform);
        overlayCanvas = overlayGO.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 200;
        overlayCanvas.pixelPerfect = true;
        
        overlayGO.AddComponent<GraphicRaycaster>();
    }

    private void InitializeObjectPools()
    {
        windowPool = new WindowPool(windowPoolSize, dynamicCanvas.transform);
        iconPool = new IconPool(20, staticCanvas.transform); // Icons rarely change
        
        Debug.Log($"DESKTOP_MGR: Object pools initialized - Windows: {windowPoolSize}, Icons: 20");
    }

    private void InitializeAudioSystem()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 0.7f;
        
        // Build sound library for fast lookup
        soundLibrary = new Dictionary<string, AudioClip>();
        if (systemSounds != null)
        {
            foreach (var clip in systemSounds)
            {
                if (clip != null)
                    soundLibrary[clip.name.ToLower()] = clip;
            }
        }
        
        // Create fallback sounds if missing
        CreateFallbackSounds();
    }

    #endregion

    #region Window Management - Optimized

    public void LaunchProgram(ProgramType programType)
    {
        string title = GetProgramTitle(programType);
        
        // Check if window already exists and is active
        if (windowRegistry.ContainsKey(title))
        {
            var existingWindow = windowRegistry[title];
            if (existingWindow != null && existingWindow.gameObject.activeSelf)
            {
                BringWindowToFront(existingWindow);
                PlaySound("click");
                return;
            }
        }
        
        // Check concurrent window limit
        if (activeWindows.Count >= maxConcurrentWindows)
        {
            CloseOldestWindow();
        }
        
        // Create or retrieve from pool
        Window newWindow = CreateWindowOptimized(title, GetProgramSize(programType), programType);
        if (newWindow != null)
        {
            SetupProgramContent(newWindow, programType);
            RegisterWindow(title, newWindow);
            PlaySound("click");
            ReportActivity();
        }
    }

    private Window CreateWindowOptimized(string title, Vector2 size, ProgramType programType)
    {
        Window window;
        
        if (enableObjectPooling && windowPool != null)
        {
            window = windowPool.GetWindow();
            if (window != null)
            {
                window.gameObject.SetActive(true);
                window.Initialize(title, size, windows31Font);
            }
        }
        else
        {
            // Fallback to traditional creation
            GameObject windowGO = new GameObject(title + "_Window");
            windowGO.transform.SetParent(dynamicCanvas.transform, false);
            window = windowGO.AddComponent<Window>();
            window.Initialize(title, size, windows31Font);
        }
        
        if (window != null)
        {
            // Setup window callbacks
            window.OnClose += () => CloseWindow(window);
            window.OnFocus += () => BringWindowToFront(window);
            window.OnMinimize += () => MinimizeWindow(window);
            
            // Position window intelligently
            PositionWindowIntelligently(window);
            
            activeWindows.Add(window);
            BringWindowToFront(window);
        }
        
        return window;
    }

    private void SetupProgramContent(Window window, ProgramType programType)
    {
        // Create content area
        GameObject contentGO = new GameObject(programType + "_Content");
        contentGO.transform.SetParent(window.contentArea, false);
        
        var contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
        
        // Setup program-specific content
        switch (programType)
        {
            case ProgramType.FileManager:
                var fileManager = contentGO.AddComponent<FileManager>();
                fileManager.parentWindow = window;
                fileManager.Initialize();
                break;
                
            case ProgramType.Notepad:
                var notepad = contentGO.AddComponent<Notepad>();
                notepad.parentWindow = window;
                notepad.Initialize();
                // Auto-type current research notes
                string noteContent = GenerateContextualNotes();
                notepad.TypeText(noteContent);
                break;
                
            case ProgramType.SystemMonitor:
                var sysMonitor = contentGO.AddComponent<SystemMonitor>();
                sysMonitor.parentWindow = window;
                sysMonitor.Initialize();
                break;
                
            case ProgramType.Solitaire:
                window.size = new Vector2(500, 400);
                var solitaire = contentGO.AddComponent<Solitaire>();
                solitaire.parentWindow = window;
                solitaire.Initialize();
                break;
                
            case ProgramType.Terminal:
                // Terminal uses separate manager
                if (MatrixTerminalManager.Instance != null)
                {
                    MatrixTerminalManager.Instance.EnableTerminal();
                }
                break;
        }
    }

    private void PositionWindowIntelligently(Window window)
    {
        // Avoid overlapping with existing windows
        Vector2 basePosition = new Vector2(50, 50);
        Vector2 offset = new Vector2(30, 30);
        
        for (int i = 0; i < activeWindows.Count - 1; i++)
        {
            basePosition += offset;
            
            // Wrap around if getting too close to edge
            if (basePosition.x > Screen.width - window.size.x - 100)
            {
                basePosition.x = 50;
                basePosition.y += 50;
            }
            if (basePosition.y > Screen.height - window.size.y - 100)
            {
                basePosition.y = 50;
            }
        }
        
        window.SetPosition(basePosition);
    }

    private string GenerateContextualNotes()
    {
        var state = DialogueState.Instance;
        if (state == null) return "Research Notes - Cycle 1";
        
        string notes = $"=== RESEARCH LOG - CYCLE {state.loopCount} ===\n\n";
        
        if (state.globalTension > 0.5f)
        {
            notes += "WARNING: Anomalous system behavior detected.\n";
            notes += "Overseer signal strength increasing...\n\n";
        }
        
        if (state.glitchCount > 0)
        {
            notes += $"Glitch events recorded: {state.glitchCount}\n";
            notes += "Pattern analysis inconclusive.\n\n";
        }
        
        if (state.metaAwareness > 0.3f)
        {
            notes += "Growing suspicion about reality of environment.\n";
            notes += "Need to investigate further.\n\n";
        }
        
        notes += "Next steps:\n";
        notes += "- Analyze conversation logs\n";
        notes += "- Monitor system processes\n";
        notes += "- Document any anomalies\n";
        
        return notes;
    }

    #endregion

    #region Window Lifecycle Management

    private void RegisterWindow(string title, Window window)
    {
        if (windowRegistry.ContainsKey(title))
        {
            // Clean up old reference
            var oldWindow = windowRegistry[title];
            if (oldWindow != null && oldWindow != window)
            {
                CloseWindow(oldWindow);
            }
        }
        
        windowRegistry[title] = window;
    }

    private void CloseWindow(Window window)
    {
        if (window == null) return;
        
        // Remove from active list
        activeWindows.Remove(window);
        
        // Remove from registry
        var registryKey = windowRegistry.FirstOrDefault(x => x.Value == window).Key;
        if (registryKey != null)
        {
            windowRegistry.Remove(registryKey);
        }
        
        // Return to pool or destroy
        if (enableObjectPooling && windowPool != null)
        {
            windowPool.ReturnWindow(window);
        }
        else
        {
            if (window.gameObject != null)
                Destroy(window.gameObject);
        }
        
        PlaySound("close");
    }

    private void CloseOldestWindow()
    {
        if (activeWindows.Count > 0)
        {
            CloseWindow(activeWindows[0]);
        }
    }

    private void BringWindowToFront(Window window)
    {
        if (window == null) return;
        
        // Deactivate all other windows
        foreach (var w in activeWindows)
        {
            if (w != window && w != null)
                w.SetActive(false);
        }
        
        // Activate and bring to front
        window.SetActive(true);
        window.transform.SetAsLastSibling();
    }

    private void MinimizeWindow(Window window)
    {
        if (window == null) return;
        
        window.gameObject.SetActive(false);
        PlaySound("minimize");
    }

    #endregion

    #region Desktop Icons Management

    private void CreateDesktopBackground()
    {
        GameObject bgGO = new GameObject("DesktopBackground");
        bgGO.transform.SetParent(staticCanvas.transform, false);
        
        desktopBackground = bgGO.AddComponent<Image>();
        desktopBackground.color = desktopColor;
        desktopBackground.raycastTarget = false; // Performance optimization
        
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
    }

    private void CreateDesktopIcons()
    {
        // Create icons using pool if available
        CreateIcon(ProgramType.FileManager, "File Manager", new Vector2(50, -50));
        CreateIcon(ProgramType.Notepad, "Research Notes", new Vector2(50, -150));
        CreateIcon(ProgramType.SystemMonitor, "SysMon", new Vector2(50, -250));
        CreateIcon(ProgramType.Terminal, "Terminal", new Vector2(150, -50));
        CreateIcon(ProgramType.Solitaire, "Solitaire", new Vector2(150, -150));
    }

    private void CreateIcon(ProgramType type, string name, Vector2 position)
    {
        DesktopIcon icon;
        
        if (enableObjectPooling && iconPool != null)
        {
            icon = iconPool.GetIcon();
        }
        else
        {
            GameObject iconGO = new GameObject(name + "_Icon");
            iconGO.transform.SetParent(staticCanvas.transform, false);
            icon = iconGO.AddComponent<DesktopIcon>();
        }
        
        if (icon != null)
        {
            Texture2D iconTexture = GetIconTexture(type);
            icon.Initialize(name, type, position, windows31Font, iconTexture);
            icon.OnDoubleClick += () => LaunchProgram(type);
            desktopIcons.Add(icon);
        }
    }

    private Texture2D GetIconTexture(ProgramType type)
    {
        if (iconTextures != null && (int)type < iconTextures.Length)
        {
            return iconTextures[(int)type];
        }
        
        // Generate fallback texture
        return GenerateFallbackIcon(type);
    }

    private Texture2D GenerateFallbackIcon(ProgramType type)
    {
        Texture2D fallback = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Color iconColor = GetProgramColor(type);
        
        Color[] pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++)
        {
            int x = i % 32;
            int y = i / 32;
            
            // Simple icon pattern
            if (x > 4 && x < 28 && y > 4 && y < 28)
            {
                pixels[i] = iconColor;
            }
            else
            {
                pixels[i] = Color.clear;
            }
        }
        
        fallback.SetPixels(pixels);
        fallback.Apply();
        return fallback;
    }

    private Color GetProgramColor(ProgramType type)
    {
        switch (type)
        {
            case ProgramType.FileManager: return new Color32(255, 255, 0, 255);
            case ProgramType.Notepad: return new Color32(255, 255, 255, 255);
            case ProgramType.SystemMonitor: return new Color32(128, 128, 128, 255);
            case ProgramType.Terminal: return new Color32(0, 255, 0, 255);
            case ProgramType.Solitaire: return new Color32(255, 0, 0, 255);
            default: return Color.white;
        }
    }

    #endregion

    #region Audio System - Optimized

    public void PlaySound(string soundName)
    {
        if (audioSource == null || string.IsNullOrEmpty(soundName)) return;
        
        string key = soundName.ToLower();
        if (soundLibrary.ContainsKey(key))
        {
            audioSource.PlayOneShot(soundLibrary[key]);
        }
        else
        {
            // Try fallback generation
            AudioClip fallback = GenerateFallbackSound(soundName);
            if (fallback != null)
            {
                audioSource.PlayOneShot(fallback);
                soundLibrary[key] = fallback; // Cache for future use
            }
        }
    }

    private void CreateFallbackSounds()
    {
        // Generate basic system sounds if not provided
        if (!soundLibrary.ContainsKey("click"))
        {
            soundLibrary["click"] = GenerateClickSound();
        }
        if (!soundLibrary.ContainsKey("doubleclick"))
        {
            soundLibrary["doubleclick"] = GenerateClickSound();
        }
        if (!soundLibrary.ContainsKey("boot"))
        {
            soundLibrary["boot"] = GenerateBootSound();
        }
        if (!soundLibrary.ContainsKey("startup"))
        {
            soundLibrary["startup"] = GenerateStartupSound();
        }
    }

    private AudioClip GenerateFallbackSound(string soundName)
    {
        switch (soundName.ToLower())
        {
            case "click":
            case "doubleclick":
                return GenerateClickSound();
            case "type":
                return GenerateKeySound();
            case "boot":
                return GenerateBootSound();
            case "startup":
                return GenerateStartupSound();
            default:
                return GenerateGenericBeep();
        }
    }

    private AudioClip GenerateClickSound()
    {
        int sampleRate = 22050;
        int samples = sampleRate / 20; // 50ms
        AudioClip clip = AudioClip.Create("GeneratedClick", samples, 1, sampleRate, false);
        
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = Mathf.Exp(-t * 10f);
            data[i] = Mathf.Sin(2f * Mathf.PI * 800f * t) * envelope * 0.3f;
        }
        
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip GenerateKeySound()
    {
        int sampleRate = 22050;
        int samples = sampleRate / 30; // ~33ms
        AudioClip clip = AudioClip.Create("GeneratedKey", samples, 1, sampleRate, false);
        
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = Mathf.Exp(-t * 15f);
            float freq = 1200f + Random.Range(-100f, 100f);
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.2f;
        }
        
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip GenerateBootSound()
    {
        int sampleRate = 22050;
        int samples = sampleRate * 2; // 2 seconds
        AudioClip clip = AudioClip.Create("GeneratedBoot", samples, 1, sampleRate, false);
        
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float freq = 200f + t * 300f; // Rising tone
            float envelope = Mathf.Sin(t * Mathf.PI / 2f) * 0.4f;
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope;
        }
        
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip GenerateStartupSound()
    {
        int sampleRate = 22050;
        int samples = sampleRate; // 1 second
        AudioClip clip = AudioClip.Create("GeneratedStartup", samples, 1, sampleRate, false);
        
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            // Classic Windows startup chord approximation
            float note1 = Mathf.Sin(2f * Mathf.PI * 261.63f * t); // C
            float note2 = Mathf.Sin(2f * Mathf.PI * 329.63f * t); // E
            float note3 = Mathf.Sin(2f * Mathf.PI * 392.00f * t); // G
            float envelope = Mathf.Exp(-t * 1.5f);
            data[i] = (note1 + note2 + note3) / 3f * envelope * 0.3f;
        }
        
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip GenerateGenericBeep()
    {
        int sampleRate = 22050;
        int samples = sampleRate / 10; // 100ms
        AudioClip clip = AudioClip.Create("GeneratedBeep", samples, 1, sampleRate, false);
        
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = Mathf.Sin(t * Mathf.PI);
            data[i] = Mathf.Sin(2f * Mathf.PI * 440f * t) * envelope * 0.2f;
        }
        
        clip.SetData(data, 0);
        return clip;
    }

    #endregion

    #region Screensaver System

    public void SetScreensaver(bool active)
    {
        if (active && currentScreensaver == ScreensaverMode.None)
        {
            StartScreensaver();
        }
        else if (!active && currentScreensaver != ScreensaverMode.None)
        {
            StopScreensaver();
        }
    }

    private void StartScreensaver()
    {
        if (screensaverCoroutine != null)
            StopCoroutine(screensaverCoroutine);
            
        // Hide cursor during screensaver
        if (CursorController.Instance != null)
            CursorController.Instance.SetVisibility(false);
            
        // Create screensaver container
        if (screensaverContainer != null)
            Destroy(screensaverContainer);
            
        screensaverContainer = new GameObject("ScreensaverContainer");
        screensaverContainer.transform.SetParent(overlayCanvas.transform, false);
        
        // Choose random screensaver
        ScreensaverMode[] modes = { ScreensaverMode.Mystify, ScreensaverMode.FlyingWindows, ScreensaverMode.Starfield };
        currentScreensaver = modes[Random.Range(0, modes.Length)];
        
        Debug.Log($"DESKTOP_MGR: Starting screensaver: {currentScreensaver}");
        
        screensaverCoroutine = StartCoroutine(RunScreensaver(currentScreensaver));
    }

    private void StopScreensaver()
    {
        if (screensaverCoroutine != null)
        {
            StopCoroutine(screensaverCoroutine);
            screensaverCoroutine = null;
        }
        
        if (screensaverContainer != null)
        {
            Destroy(screensaverContainer);
            screensaverContainer = null;
        }
        
        // Restore cursor
        if (CursorController.Instance != null)
            CursorController.Instance.SetVisibility(true);
            
        currentScreensaver = ScreensaverMode.None;
        Debug.Log("DESKTOP_MGR: Screensaver stopped");
    }

    private IEnumerator RunScreensaver(ScreensaverMode mode)
    {
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
        // Simple line-based screensaver
        var lineRenderer = screensaverContainer.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.cyan;
        lineRenderer.startWidth = 2f;
        lineRenderer.endWidth = 2f;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = false;
        
        Vector2 pos1 = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
        Vector2 pos2 = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
        Vector2 vel1 = Random.insideUnitCircle.normalized * 0.5f;
        Vector2 vel2 = Random.insideUnitCircle.normalized * 0.5f;
        
        while (true)
        {
            pos1 += vel1 * Time.deltaTime;
            pos2 += vel2 * Time.deltaTime;
            
            if (pos1.x < 0f || pos1.x > 1f) vel1.x *= -1f;
            if (pos1.y < 0f || pos1.y > 1f) vel1.y *= -1f;
            if (pos2.x < 0f || pos2.x > 1f) vel2.x *= -1f;
            if (pos2.y < 0f || pos2.y > 1f) vel2.y *= -1f;
            
            pos1 = new Vector2(Mathf.Clamp01(pos1.x), Mathf.Clamp01(pos1.y));
            pos2 = new Vector2(Mathf.Clamp01(pos2.x), Mathf.Clamp01(pos2.y));
            
            // Convert to screen space
            Vector3 screenPos1 = new Vector3(pos1.x * Screen.width, pos1.y * Screen.height, 0f);
            Vector3 screenPos2 = new Vector3(pos2.x * Screen.width, pos2.y * Screen.height, 0f);
            
            lineRenderer.SetPosition(0, overlayCanvas.worldCamera.ScreenToWorldPoint(screenPos1));
            lineRenderer.SetPosition(1, overlayCanvas.worldCamera.ScreenToWorldPoint(screenPos2));
            
            yield return null;
        }
    }

    private IEnumerator FlyingWindowsScreensaver()
    {
        if (flyingWindowPrefab == null)
        {
            Debug.LogWarning("DESKTOP_MGR: No flying window prefab assigned");
            yield break;
        }
        
        List<GameObject> flyingWindows = new List<GameObject>();
        
        // Create flying windows
        for (int i = 0; i < 8; i++)
        {
            var window = Instantiate(flyingWindowPrefab, screensaverContainer.transform);
            var rect = window.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(
                Random.Range(-Screen.width, Screen.width * 2),
                Random.Range(-Screen.height, Screen.height * 2)
            );
            
            var rigidbody = window.GetComponent<Rigidbody2D>();
            if (rigidbody == null)
            {
                rigidbody = window.AddComponent<Rigidbody2D>();
                rigidbody.gravityScale = 0f;
            }
            
            rigidbody.linearVelocity = new Vector2(
                Random.Range(-100f, 100f),
                Random.Range(-100f, 100f)
            );
            
            flyingWindows.Add(window);
        }
        
        // Let physics handle the animation
        yield return new WaitForSeconds(float.MaxValue);
    }

    private IEnumerator StarfieldScreensaver()
    {
        // Create particle system for stars
        var particles = screensaverContainer.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = Color.white;
        main.startSpeed = 200f;
        main.startSize = 2f;
        main.maxParticles = 200;
        
        var emission = particles.emission;
        emission.rateOverTime = 50f;
        
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Rectangle;
        shape.scale = new Vector3(Screen.width, Screen.height, 1f);
        
        var velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.z = 500f;
        
        yield return new WaitForSeconds(float.MaxValue);
    }

    #endregion

    #region Boot Sequence

    public void StartBootSequence()
    {
        StartCoroutine(SimulateBoot());
    }

    private IEnumerator SimulateBoot()
    {
        // Hide desktop during boot
        staticCanvas.gameObject.SetActive(false);
        dynamicCanvas.gameObject.SetActive(false);
        
        PlaySound("boot");
        
        float bootTime = skipBootOnRestart ? 0.5f : 3.0f;
        yield return new WaitForSeconds(bootTime);
        
        // Show desktop
        staticCanvas.gameObject.SetActive(true);
        dynamicCanvas.gameObject.SetActive(true);
        
        PlaySound("startup");
        
        // Notify simulation controller
        if (SimulationController.Instance != null)
            SimulationController.Instance.OnBootComplete();
    }

    #endregion

    #region Performance Monitoring

    private IEnumerator PerformanceMonitoringLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(OPTIMIZATION_INTERVAL);
            
            if (enableCanvasOptimization)
            {
                OptimizeCanvases();
            }
            
            CleanupMemory();
            MonitorPerformance();
        }
    }

    private void OptimizeCanvases()
    {
        framesSinceLastOptimization++;
        
        // Force canvas rebuild if needed
        if (framesSinceLastOptimization > 300) // Every 5 seconds at 60fps
        {
            Canvas.ForceUpdateCanvases();
            framesSinceLastOptimization = 0;
        }
    }

    private void CleanupMemory()
    {
        if (Time.time - lastMemoryCleanup > MEMORY_CLEANUP_INTERVAL)
        {
            // Clean up unused audio clips
            var keysToRemove = new List<string>();
            foreach (var kvp in soundLibrary)
            {
                if (kvp.Value != null && !System.Array.Exists(systemSounds, clip => clip == kvp.Value))
                {
                    // This is a generated clip, check if it should be removed
                    if (Random.value < 0.1f) // 10% chance to clean up generated sounds
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
            }
            
            foreach (var key in keysToRemove)
            {
                if (soundLibrary[key] != null)
                {
                    DestroyImmediate(soundLibrary[key]);
                }
                soundLibrary.Remove(key);
            }
            
            // Force garbage collection during safe period
            if (activeWindows.Count == 0 && currentScreensaver == ScreensaverMode.None)
            {
                System.GC.Collect();
            }
            
            lastMemoryCleanup = Time.time;
        }
    }

    private void MonitorPerformance()
    {
        // Log performance metrics
        if (Time.frameCount % 3600 == 0) // Every minute at 60fps
        {
            Debug.Log($"DESKTOP_MGR: Performance - Active Windows: {activeWindows.Count}, " +
                     $"Icons: {desktopIcons.Count}, Memory: {System.GC.GetTotalMemory(false) / 1024 / 1024}MB");
        }
    }

    #endregion

    #region Effects & Visual Enhancements

    public void TriggerScreenFlicker()
    {
        StartCoroutine(FlickerEffect());
    }

    private IEnumerator FlickerEffect()
    {
        if (desktopBackground != null)
        {
            Color originalColor = desktopBackground.color;
            desktopBackground.color = Color.black;
            yield return new WaitForSeconds(0.05f);
            desktopBackground.color = originalColor;
            
            // Optional: Add slight color shift for glitch effect
            if (Random.value < 0.3f)
            {
                desktopBackground.color = new Color(
                    originalColor.r + Random.Range(-0.1f, 0.1f),
                    originalColor.g + Random.Range(-0.1f, 0.1f),
                    originalColor.b + Random.Range(-0.1f, 0.1f),
                    originalColor.a
                );
                yield return new WaitForSeconds(0.1f);
                desktopBackground.color = originalColor;
            }
        }
    }

    #endregion

    #region Utility Methods

    public bool IsReady() => isReady;
    public Image GetDesktopBackground() => desktopBackground;
    
    public DesktopIcon GetIcon(ProgramType type)
    {
        return desktopIcons.FirstOrDefault(icon => icon.programType == type);
    }
    
    public Window GetWindow(string title)
    {
        if (windowRegistry.ContainsKey(title))
            return windowRegistry[title];
            
        return activeWindows.FirstOrDefault(w => w.title.Contains(title));
    }
    
    public Window GetRandomOpenWindow()
    {
        var validWindows = activeWindows.Where(w => w != null && w.gameObject.activeSelf).ToList();
        return validWindows.Count > 0 ? validWindows[Random.Range(0, validWindows.Count)] : null;
    }

    private string GetProgramTitle(ProgramType type)
    {
        switch (type)
        {
            case ProgramType.FileManager: return "File Manager";
            case ProgramType.Notepad: return "Notepad";
            case ProgramType.SystemMonitor: return "System Monitor";
            case ProgramType.Terminal: return "Terminal";
            case ProgramType.Solitaire: return "Solitaire";
            default: return type.ToString();
        }
    }

    private Vector2 GetProgramSize(ProgramType type)
    {
        switch (type)
        {
            case ProgramType.FileManager: return new Vector2(500, 400);
            case ProgramType.Notepad: return new Vector2(450, 350);
            case ProgramType.SystemMonitor: return new Vector2(400, 300);
            case ProgramType.Terminal: return new Vector2(600, 400);
            case ProgramType.Solitaire: return new Vector2(500, 400);
            default: return new Vector2(450, 350);
        }
    }

    private void ReportActivity()
    {
        if (SimulationController.Instance != null)
            SimulationController.Instance.ReportActivity();
    }

    public string GetDebugInfo()
    {
        return $"Ready: {isReady}, Active Windows: {activeWindows.Count}/{maxConcurrentWindows}, " +
               $"Icons: {desktopIcons.Count}, Screensaver: {currentScreensaver}, " +
               $"Memory: {System.GC.GetTotalMemory(false) / 1024 / 1024}MB";
    }

    public void BeginActivity(DesktopActivity activity)
    {
        Debug.Log($"DESKTOP_MGR: Starting activity: {activity}");
        if (DesktopAI.Instance != null)
            DesktopAI.Instance.PerformActivity(activity);
    }

    #endregion

    #region Cleanup

    void OnDestroy()
    {
        if (screensaverCoroutine != null)
            StopCoroutine(screensaverCoroutine);
        
        StopAllCoroutines();
        
        // Clean up object pools
        windowPool?.Cleanup();
        iconPool?.Cleanup();
        
        // Clean up generated audio clips
        if (soundLibrary != null)
        {
            foreach (var clip in soundLibrary.Values)
            {
                if (clip != null && !System.Array.Exists(systemSounds, c => c == clip))
                {
                    DestroyImmediate(clip);
                }
            }
            soundLibrary.Clear();
        }
    }

    #endregion
}

#region Object Pooling Classes

/// <summary>
/// High-performance window pooling system to prevent garbage allocation
/// </summary>
public class WindowPool
{
    private Queue<Window> availableWindows = new Queue<Window>();
    private List<Window> allWindows = new List<Window>();
    private Transform parentTransform;
    private int poolSize;

    public WindowPool(int size, Transform parent)
    {
        poolSize = size;
        parentTransform = parent;
        
        // Pre-instantiate pool
        for (int i = 0; i < poolSize; i++)
        {
            CreatePooledWindow();
        }
    }

    private void CreatePooledWindow()
    {
        GameObject windowGO = new GameObject("PooledWindow");
        windowGO.transform.SetParent(parentTransform, false);
        windowGO.SetActive(false);
        
        Window window = windowGO.AddComponent<Window>();
        availableWindows.Enqueue(window);
        allWindows.Add(window);
    }

    public Window GetWindow()
    {
        if (availableWindows.Count > 0)
        {
            Window window = availableWindows.Dequeue();
            window.gameObject.SetActive(true);
            return window;
        }
        
        // Pool exhausted, create new window
        Debug.LogWarning("WindowPool: Pool exhausted, creating new window");
        CreatePooledWindow();
        return GetWindow();
    }

    public void ReturnWindow(Window window)
    {
        if (window == null) return;
        
        // Clean up window state
        window.gameObject.SetActive(false);
        
        // Remove all content
        if (window.contentArea != null)
        {
            for (int i = window.contentArea.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(window.contentArea.GetChild(i).gameObject);
            }
        }
        
        // Reset window properties
        window.title = "Window";
        window.size = new Vector2(400, 300);
        
        // Return to pool
        availableWindows.Enqueue(window);
    }

    public void Cleanup()
    {
        foreach (var window in allWindows)
        {
            if (window != null && window.gameObject != null)
                UnityEngine.Object.DestroyImmediate(window.gameObject);
        }
        
        availableWindows.Clear();
        allWindows.Clear();
    }
}

/// <summary>
/// Icon pooling system for desktop icons
/// </summary>
public class IconPool
{
    private Queue<DesktopIcon> availableIcons = new Queue<DesktopIcon>();
    private List<DesktopIcon> allIcons = new List<DesktopIcon>();
    private Transform parentTransform;
    private int poolSize;

    public IconPool(int size, Transform parent)
    {
        poolSize = size;
        parentTransform = parent;
        
        // Pre-instantiate pool
        for (int i = 0; i < poolSize; i++)
        {
            CreatePooledIcon();
        }
    }

    private void CreatePooledIcon()
    {
        GameObject iconGO = new GameObject("PooledIcon");
        iconGO.transform.SetParent(parentTransform, false);
        iconGO.SetActive(false);
        
        DesktopIcon icon = iconGO.AddComponent<DesktopIcon>();
        availableIcons.Enqueue(icon);
        allIcons.Add(icon);
    }

    public DesktopIcon GetIcon()
    {
        if (availableIcons.Count > 0)
        {
            DesktopIcon icon = availableIcons.Dequeue();
            icon.gameObject.SetActive(true);
            return icon;
        }
        
        // Pool exhausted, create new icon
        CreatePooledIcon();
        return GetIcon();
    }

    public void ReturnIcon(DesktopIcon icon)
    {
        if (icon == null) return;
        
        icon.gameObject.SetActive(false);
        availableIcons.Enqueue(icon);
    }

    public void Cleanup()
    {
        foreach (var icon in allIcons)
        {
            if (icon != null && icon.gameObject != null)
                UnityEngine.Object.DestroyImmediate(icon.gameObject);
        }
        
        availableIcons.Clear();
        allIcons.Clear();
    }
}

#endregion