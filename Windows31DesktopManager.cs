using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// CLEAN Windows 3.1 Desktop Manager - WHITE SCREEN FIXED
/// Simplified, conflict-free desktop creation
/// </summary>
public class Windows31DesktopManager : MonoBehaviour
{
    public static Windows31DesktopManager Instance { get; private set; }

    [Header("Desktop Settings")]
    public Color desktopColor = new Color32(0, 128, 128, 255); // Teal
    public bool skipBootOnRestart = true;
    public bool debugMode = true;

    // Enums needed by other scripts
    public enum DesktopActivity
    {
        Idle, OrganizingFiles, ReviewingNotes, SystemMaintenance, 
        Playing, Researching, WaitingForConnection, InConversation
    }

    public enum ProgramType
    {
        FileManager, Notepad, SystemMonitor, Terminal, Solitaire, Calculator
    }

    // Core Components
    private Canvas desktopCanvas;
    private Image desktopBackground;
    private bool isReady = false;
    private DesktopActivity currentActivity = DesktopActivity.Idle;

    void Awake()
    {
        // Singleton enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        Debug.Log("Windows31DesktopManager: Awake - Instance created");
    }

    void Start()
    {
        Debug.Log("Windows31DesktopManager: Start - Beginning desktop creation");
        
        // NUCLEAR OPTION: Clear everything first
        ClearAllCanvases();
        
        // Create desktop immediately
        CreateDesktop();
    }

    void ClearAllCanvases()
    {
        Debug.Log("Windows31DesktopManager: Clearing all existing canvases");
        
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in allCanvases)
        {
            Debug.Log($"Destroying canvas: {canvas.name}");
            DestroyImmediate(canvas.gameObject);
        }
        
        Debug.Log($"Cleared {allCanvases.Length} canvases");
    }

    void CreateDesktop()
    {
        Debug.Log("Windows31DesktopManager: Creating desktop canvas");
        
        // Create canvas
        GameObject canvasGO = new GameObject("DesktopCanvas");
        desktopCanvas = canvasGO.AddComponent<Canvas>();
        desktopCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        desktopCanvas.sortingOrder = 0; // Base layer
        
        // Add canvas scaler
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1024, 768);
        
        // Add graphics raycaster
        canvasGO.AddComponent<GraphicRaycaster>();
        
        Debug.Log("Windows31DesktopManager: Canvas created, creating background");
        
        // Create background
        CreateBackground();
        
        // Create basic UI
        CreateTaskbar();
        
        isReady = true;
        Debug.Log("Windows31DesktopManager: Desktop creation COMPLETE");
    }

    void CreateBackground()
    {
        GameObject bgGO = new GameObject("DesktopBackground");
        bgGO.transform.SetParent(desktopCanvas.transform, false);
        
        desktopBackground = bgGO.AddComponent<Image>();
        desktopBackground.color = desktopColor;
        
        // Fill screen
        RectTransform bgRect = desktopBackground.rectTransform;
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        Debug.Log($"Windows31DesktopManager: Background created with color {desktopColor}");
    }

    void CreateTaskbar()
    {
        GameObject taskbarGO = new GameObject("Taskbar");
        taskbarGO.transform.SetParent(desktopCanvas.transform, false);
        
        Image taskbarBg = taskbarGO.AddComponent<Image>();
        taskbarBg.color = new Color32(192, 192, 192, 255);
        
        RectTransform taskbarRect = taskbarBg.rectTransform;
        taskbarRect.anchorMin = new Vector2(0, 0);
        taskbarRect.anchorMax = new Vector2(1, 0);
        taskbarRect.offsetMin = Vector2.zero;
        taskbarRect.offsetMax = new Vector2(0, 30);
        
        Debug.Log("Windows31DesktopManager: Taskbar created");
    }

    void Update()
    {
        if (debugMode)
        {
            HandleDebugKeys();
        }
    }

    void HandleDebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            LogStatus();
        }
        
        if (Input.GetKeyDown(KeyCode.F10))
        {
            ForceBackgroundColor();
        }
        
        if (Input.GetKeyDown(KeyCode.F11))
        {
            RecreateDesktop();
        }
        
        if (Input.GetKeyDown(KeyCode.F12))
        {
            LogAllCanvases();
        }
    }

    void LogStatus()
    {
        Debug.Log("=== DESKTOP STATUS ===");
        Debug.Log($"Ready: {isReady}");
        Debug.Log($"Canvas: {(desktopCanvas != null ? "EXISTS" : "NULL")}");
        Debug.Log($"Background: {(desktopBackground != null ? "EXISTS" : "NULL")}");
        
        if (desktopCanvas != null)
        {
            Debug.Log($"Canvas Active: {desktopCanvas.gameObject.activeInHierarchy}");
            Debug.Log($"Canvas Order: {desktopCanvas.sortingOrder}");
        }
        
        if (desktopBackground != null)
        {
            Debug.Log($"Background Color: {desktopBackground.color}");
            Debug.Log($"Background Active: {desktopBackground.gameObject.activeInHierarchy}");
        }
        
        Debug.Log("==================");
    }

    void ForceBackgroundColor()
    {
        if (desktopBackground != null)
        {
            desktopBackground.color = desktopColor;
            Debug.Log($"Forced background color to {desktopColor}");
        }
        else
        {
            Debug.LogError("Cannot force color - background is null!");
        }
    }

    void RecreateDesktop()
    {
        Debug.Log("Recreating desktop");
        
        isReady = false;
        
        if (desktopCanvas != null)
        {
            DestroyImmediate(desktopCanvas.gameObject);
        }
        
        CreateDesktop();
    }

    void LogAllCanvases()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        Debug.Log($"=== CANVAS COUNT: {canvases.Length} ===");
        
        foreach (Canvas c in canvases)
        {
            Image bg = c.GetComponentInChildren<Image>();
            Debug.Log($"Canvas: {c.name}, Order: {c.sortingOrder}, Color: {(bg != null ? bg.color.ToString() : "No Image")}");
        }
        
        Debug.Log("================================");
    }

    public bool IsReady()
    {
        return isReady && desktopCanvas != null && desktopBackground != null;
    }

    // Public methods needed by other scripts
    public void BeginActivity(DesktopActivity activity)
    {
        currentActivity = activity;
        Debug.Log($"Windows31DesktopManager: Activity set to {activity}");
    }

    public void LaunchProgram(ProgramType programType)
    {
        Debug.Log($"Windows31DesktopManager: Would launch {programType}");
    }

    public void TriggerConversationMode()
    {
        currentActivity = DesktopActivity.InConversation;
        Debug.Log("Windows31DesktopManager: Conversation mode triggered");
    }

    public void ShowDesktop()
    {
        if (desktopCanvas != null)
        {
            desktopCanvas.gameObject.SetActive(true);
        }
    }

    public string GetDebugInfo()
    {
        return $"Ready: {IsReady()}, Activity: {currentActivity}";
    }

    void OnGUI()
    {
        if (!debugMode) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 250, 200));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("Windows 3.1 Desktop");
        GUILayout.Space(5);
        
        GUILayout.Label($"Ready: {(IsReady() ? "YES" : "NO")}");
        GUILayout.Label($"Canvas: {(desktopCanvas != null ? "OK" : "NULL")}");
        GUILayout.Label($"Background: {(desktopBackground != null ? "OK" : "NULL")}");
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("F9: Log Status"))
            LogStatus();
            
        if (GUILayout.Button("F10: Force Color"))
            ForceBackgroundColor();
            
        if (GUILayout.Button("F11: Recreate"))
            RecreateDesktop();
            
        if (GUILayout.Button("F12: Log Canvases"))
            LogAllCanvases();
        
        GUILayout.Space(10);
        
        // Color test buttons
        if (GUILayout.Button("Red")) { desktopColor = Color.red; ForceBackgroundColor(); }
        if (GUILayout.Button("Green")) { desktopColor = Color.green; ForceBackgroundColor(); }
        if (GUILayout.Button("Blue")) { desktopColor = Color.blue; ForceBackgroundColor(); }
        if (GUILayout.Button("Teal")) { desktopColor = new Color32(0, 128, 128, 255); ForceBackgroundColor(); }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}