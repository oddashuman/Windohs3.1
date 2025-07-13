using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Debugging version of Windows31DesktopManager to identify initialization issues
/// </summary>
public class Windows31DesktopManagerDebug : MonoBehaviour
{
    public static Windows31DesktopManagerDebug Instance { get; private set; }

    [Header("Desktop Settings")]
    public Color desktopColor = new Color32(0, 128, 128, 255);
    public bool skipBootOnRestart = true; // Skip boot for faster testing
    
    // UI Components
    private Canvas desktopCanvas;
    private Image desktopBackground;
    private bool isInitialized = false;
    private bool canvasCreated = false;
    private bool backgroundCreated = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        Debug.Log("DesktopManagerDebug: Awake called");
    }

    void Start()
    {
        Debug.Log("DesktopManagerDebug: Start called - beginning initialization");
        StartCoroutine(InitializeDesktopDebug());
    }

    IEnumerator InitializeDesktopDebug()
    {
        Debug.Log("DesktopManagerDebug: InitializeDesktopDebug started");
        
        yield return null; // Wait one frame
        
        Debug.Log("DesktopManagerDebug: Creating desktop canvas...");
        CreateDesktopCanvas();
        
        yield return null; // Wait another frame
        
        Debug.Log("DesktopManagerDebug: Creating desktop background...");
        CreateDesktopBackground();
        
        yield return null;
        
        Debug.Log("DesktopManagerDebug: Showing desktop...");
        ShowDesktop();
        
        isInitialized = true;
        Debug.Log("DesktopManagerDebug: Initialization complete!");
    }

    void CreateDesktopCanvas()
    {
        try
        {
            // Create desktop canvas
            GameObject desktopGO = new GameObject("DesktopCanvas");
            desktopGO.transform.SetParent(transform, false);
            
            desktopCanvas = desktopGO.AddComponent<Canvas>();
            desktopCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            desktopCanvas.sortingOrder = 50;
            desktopCanvas.pixelPerfect = true;

            // Add canvas scaler
            CanvasScaler scaler = desktopGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1024, 768);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // Add graphics raycaster
            desktopGO.AddComponent<GraphicRaycaster>();

            canvasCreated = true;
            Debug.Log("DesktopManagerDebug: Canvas created successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DesktopManagerDebug: Failed to create canvas - {e.Message}");
        }
    }

    void CreateDesktopBackground()
    {
        try
        {
            if (desktopCanvas == null)
            {
                Debug.LogError("DesktopManagerDebug: Cannot create background - canvas is null!");
                return;
            }

            // Create background object
            GameObject bgGO = new GameObject("DesktopBackground");
            bgGO.transform.SetParent(desktopCanvas.transform, false);
            
            // Add image component
            desktopBackground = bgGO.AddComponent<Image>();
            desktopBackground.color = desktopColor;
            desktopBackground.raycastTarget = true;
            
            // Set up rect transform to fill screen
            RectTransform bgRect = desktopBackground.rectTransform;
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            backgroundCreated = true;
            Debug.Log($"DesktopManagerDebug: Background created with color {desktopColor}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DesktopManagerDebug: Failed to create background - {e.Message}");
        }
    }

    void ShowDesktop()
    {
        try
        {
            if (desktopCanvas != null)
            {
                desktopCanvas.gameObject.SetActive(true);
                Debug.Log("DesktopManagerDebug: Desktop canvas activated");
            }
            else
            {
                Debug.LogError("DesktopManagerDebug: Cannot show desktop - canvas is null!");
            }

            if (desktopBackground != null)
            {
                desktopBackground.color = desktopColor; // Force color
                Debug.Log($"DesktopManagerDebug: Background color set to {desktopBackground.color}");
            }
            else
            {
                Debug.LogError("DesktopManagerDebug: Cannot set background - background is null!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DesktopManagerDebug: Failed to show desktop - {e.Message}");
        }
    }

    void Update()
    {
        // Debug keys
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Debug.Log("=== DESKTOP DEBUG STATUS ===");
            Debug.Log($"Initialized: {isInitialized}");
            Debug.Log($"Canvas Created: {canvasCreated}");
            Debug.Log($"Background Created: {backgroundCreated}");
            Debug.Log($"Canvas Exists: {desktopCanvas != null}");
            Debug.Log($"Canvas Active: {(desktopCanvas != null ? desktopCanvas.gameObject.activeInHierarchy : false)}");
            Debug.Log($"Background Exists: {desktopBackground != null}");
            Debug.Log($"Background Color: {(desktopBackground != null ? desktopBackground.color.ToString() : "N/A")}");
            Debug.Log($"Background Active: {(desktopBackground != null ? desktopBackground.gameObject.activeInHierarchy : false)}");
            
            // Check if there are any other canvases interfering
            Canvas[] allCanvases = FindObjectsOfType<Canvas>();
            Debug.Log($"Total Canvases in Scene: {allCanvases.Length}");
            foreach (Canvas canvas in allCanvases)
            {
                Debug.Log($"Canvas: {canvas.name}, Order: {canvas.sortingOrder}, Active: {canvas.gameObject.activeInHierarchy}");
            }
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            Debug.Log("DesktopManagerDebug: Force recreating desktop");
            ForceRecreateDesktop();
        }
    }

    void ForceRecreateDesktop()
    {
        // Destroy existing components
        if (desktopCanvas != null)
        {
            DestroyImmediate(desktopCanvas.gameObject);
        }

        // Reset flags
        canvasCreated = false;
        backgroundCreated = false;
        isInitialized = false;

        // Recreate
        StartCoroutine(InitializeDesktopDebug());
    }

    public bool IsReady()
    {
        return isInitialized && canvasCreated && backgroundCreated;
    }

    public string GetDebugInfo()
    {
        return $"Initialized: {isInitialized}, Canvas: {canvasCreated}, Background: {backgroundCreated}, " +
               $"Canvas Active: {(desktopCanvas != null ? desktopCanvas.gameObject.activeInHierarchy : false)}";
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(220, 10, 300, 200));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("Desktop Debug Panel");
        GUILayout.Space(5);
        
        GUILayout.Label($"Initialized: {isInitialized}");
        GUILayout.Label($"Canvas Created: {canvasCreated}");
        GUILayout.Label($"Background Created: {backgroundCreated}");
        
        if (desktopCanvas != null)
        {
            GUILayout.Label($"Canvas Active: {desktopCanvas.gameObject.activeInHierarchy}");
            GUILayout.Label($"Canvas Order: {desktopCanvas.sortingOrder}");
        }
        
        if (desktopBackground != null)
        {
            GUILayout.Label($"BG Color: {desktopBackground.color}");
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Debug Status (F3)"))
        {
            Debug.Log("Manual debug trigger");
        }
        
        if (GUILayout.Button("Force Recreate (F4)"))
        {
            ForceRecreateDesktop();
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}