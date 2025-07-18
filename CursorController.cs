using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Production-ready cursor controller with human-like movement patterns.
/// Features: Bezier curves, Perlin noise micro-movements, fatigue simulation,
/// personality-based behavior, and zero-allocation optimization.
/// </summary>
public class CursorController : MonoBehaviour
{
    public static CursorController Instance { get; private set; }

    [Header("Cursor Visuals")]
    public Texture2D[] cursorTextures;
    private RectTransform cursorRect;
    private Image cursorImage;
    private Canvas cursorCanvas;

    [Header("Movement Physics - Enhanced")]
    [Range(200f, 1200f)] public float baseSpeed = 800f;
    [Range(0f, 1f)] public float hesitationChance = 0.15f;
    [Range(0f, 0.1f)] public float microJitterStrength = 0.02f;
    [Range(0.5f, 3f)] public float curvatureIntensity = 1.2f;

    [Header("Human Behavior Simulation")]
    [Range(0f, 1f)] public float currentFatigue = 0f;
    [Range(0f, 1f)] public float currentConfidence = 0.8f;
    [Range(0f, 1f)] public float currentTension = 0f;

    // Performance optimization - cached values
    private Vector2 currentPosition;
    private Vector2 lastFramePosition;
    private float currentSpeed;
    private bool isMoving = false;
    
    // Advanced movement system
    private struct MovementPath
    {
        public Vector2 start, control1, control2, end;
        public float duration;
        public AnimationCurve speedCurve;
    }
    
    private MovementPath currentPath;
    private Coroutine currentMovementCoroutine;
    private Coroutine idleJitterCoroutine;
    
    // Personality system
    private struct PersonalityProfile
    {
        public float precision;      // How accurate movements are
        public float confidence;     // Speed consistency
        public float nervousness;    // Jitter frequency
        public float patience;       // Hesitation likelihood
    }
    private PersonalityProfile personality;

    // Memory management
    private readonly Queue<Vector2> recentPositions = new Queue<Vector2>(10);
    private float lastGCTime;
    private const float GC_INTERVAL = 30f;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        
        InitializeCursorTextures();
        InitializeCursor();
        InitializePersonality();
    }

    void Start()
    {
        StartIdleJitter();
    }

    void Update()
    {
        UpdateMovementTracking();
        UpdateFatigueSystem();
        OptimizedMemoryManagement();
    }

    #region Initialization
    
    private void InitializeCursorTextures()
    {
        if (cursorTextures == null || cursorTextures.Length == 0)
        {
            Debug.LogWarning("CursorController: Creating fallback cursor texture");
            Texture2D fallback = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[16 * 16];
            
            // Create a simple arrow cursor pattern
            for (int i = 0; i < pixels.Length; i++)
            {
                int x = i % 16;
                int y = i / 16;
                pixels[i] = (x < 3 && y < 12) || (y < 3 && x < 8) ? Color.white : Color.clear;
            }
            
            fallback.SetPixels(pixels);
            fallback.Apply();
            cursorTextures = new Texture2D[] { fallback };
        }
    }

    private void InitializeCursor()
    {
        // Optimized cursor setup with proper canvas hierarchy
        GameObject cursorGO = new GameObject("OrionsCursor", typeof(RectTransform));
        cursorGO.transform.SetParent(transform, false);
        
        cursorCanvas = cursorGO.AddComponent<Canvas>();
        cursorCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        cursorCanvas.sortingOrder = 10000;
        cursorCanvas.pixelPerfect = true;
        
        // Optimize for performance
        var scaler = cursorGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        
        cursorRect = cursorGO.GetComponent<RectTransform>();
        cursorRect.sizeDelta = new Vector2(32, 32);
        cursorRect.pivot = new Vector2(0, 1);
        
        cursorImage = cursorGO.AddComponent<Image>();
        cursorImage.raycastTarget = false; // Performance optimization
        
        var sprite = Sprite.Create(cursorTextures[0], 
            new Rect(0, 0, cursorTextures[0].width, cursorTextures[0].height), 
            new Vector2(0, 1), 100f, 0, SpriteMeshType.FullRect);
        cursorImage.sprite = sprite;
        
        Cursor.visible = false;
        currentPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        cursorRect.position = currentPosition;
        lastFramePosition = currentPosition;
    }

    private void InitializePersonality()
    {
        // Orion's personality - methodical but slightly nervous
        personality = new PersonalityProfile
        {
            precision = Random.Range(0.7f, 0.85f),
            confidence = Random.Range(0.6f, 0.8f),
            nervousness = Random.Range(0.15f, 0.25f),
            patience = Random.Range(0.3f, 0.5f)
        };
    }

    #endregion

    #region Public API - Enhanced

    public IEnumerator MoveToAndClick(Vector2 target, string mood = "casual")
    {
        isMoving = true;
        ApplyMoodModifiers(mood);
        yield return StartCoroutine(ExecuteHumanMovement(target));
        yield return StartCoroutine(PerformClickWithRealism(false));
        isMoving = false;
        ReportActivity();
    }

    public IEnumerator MoveToAndDoubleClick(Vector2 target, string mood = "casual")
    {
        isMoving = true;
        ApplyMoodModifiers(mood);
        yield return StartCoroutine(ExecuteHumanMovement(target));
        
        // Realistic double-click timing with slight variations
        yield return StartCoroutine(PerformClickWithRealism(true));
        float doubleClickDelay = Random.Range(0.08f, 0.15f) * (1f + currentFatigue * 0.5f);
        yield return new WaitForSeconds(doubleClickDelay);
        yield return StartCoroutine(PerformClickWithRealism(true));
        
        isMoving = false;
        ReportActivity();
    }

    public IEnumerator MoveAndDrag(Vector2 startHandle, Vector2 dragVector)
    {
        isMoving = true;
        yield return StartCoroutine(ExecuteHumanMovement(startHandle));
        
        // Simulate mouse down hesitation
        yield return new WaitForSeconds(Random.Range(0.05f, 0.12f));
        
        Vector2 endPosition = startHandle + dragVector;
        yield return StartCoroutine(ExecuteHumanMovement(endPosition, true));
        
        // Simulate mouse up hesitation
        yield return new WaitForSeconds(Random.Range(0.03f, 0.08f));
        
        isMoving = false;
        ReportActivity();
    }

    public void MoveTo(Vector2 target, bool immediate)
    {
        if (immediate)
        {
            SetPositionImmediate(target);
        }
        else
        {
            if (currentMovementCoroutine != null) 
                StopCoroutine(currentMovementCoroutine);
            currentMovementCoroutine = StartCoroutine(ExecuteHumanMovement(target));
        }
    }

    #endregion

    #region Advanced Movement System

    private IEnumerator ExecuteHumanMovement(Vector2 target, bool isDragging = false)
    {
        // Pre-movement hesitation based on personality and tension
        if (!isDragging && ShouldHesitate())
        {
            float hesitationTime = CalculateHesitationTime();
            yield return new WaitForSeconds(hesitationTime);
        }

        // Calculate human-like movement path
        MovementPath path = CalculateMovementPath(currentPosition, target, isDragging);
        currentPath = path;

        // Execute movement with realistic timing
        float elapsedTime = 0f;
        Vector2 startPos = currentPosition;

        while (elapsedTime < path.duration)
        {
            float normalizedTime = elapsedTime / path.duration;
            
            // Apply speed curve for realistic acceleration/deceleration
            float curvedTime = path.speedCurve.Evaluate(normalizedTime);
            
            // Calculate position using cubic Bezier curve
            Vector2 newPosition = CalculateBezierPoint(curvedTime, 
                path.start, path.control1, path.control2, path.end);
            
            // Add micro-jitter for human imperfection
            newPosition += CalculateMicroJitter(normalizedTime);
            
            // Apply position with bounds checking
            SetPositionSafe(newPosition);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure we end exactly at target
        SetPositionSafe(target);
        
        // Update fatigue after movement
        UpdateFatigueFromMovement(Vector2.Distance(startPos, target));
    }

    private MovementPath CalculateMovementPath(Vector2 start, Vector2 end, bool isDragging)
    {
        float distance = Vector2.Distance(start, end);
        Vector2 direction = (end - start).normalized;
        
        // Calculate movement duration based on distance and current state
        float baseDuration = distance / CalculateCurrentSpeed();
        float durationMultiplier = 1f + currentFatigue * 0.3f - currentConfidence * 0.2f;
        float finalDuration = baseDuration * durationMultiplier;
        
        // Create control points for Bezier curve
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);
        
        // Add curvature based on personality and tension
        float curvature = curvatureIntensity * (0.5f + personality.nervousness + currentTension * 0.5f);
        float curveOffset = distance * 0.3f * curvature;
        
        // Randomize curve direction
        if (Random.value > 0.5f) curveOffset = -curveOffset;
        
        Vector2 control1 = start + direction * distance * 0.3f + perpendicular * curveOffset * 0.7f;
        Vector2 control2 = start + direction * distance * 0.7f + perpendicular * curveOffset * 0.3f;
        
        // Create speed curve based on movement type
        AnimationCurve speedCurve;
        if (isDragging)
        {
            // Steady speed for dragging
            speedCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }
        else
        {
            // Natural acceleration/deceleration
            speedCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 2f),
                new Keyframe(0.3f, 0.8f, 1f, 0.5f),
                new Keyframe(0.7f, 1f, 0.2f, -0.5f),
                new Keyframe(1f, 1f, -1f, 0f)
            );
        }

        return new MovementPath
        {
            start = start,
            control1 = control1,
            control2 = control2,
            end = end,
            duration = finalDuration,
            speedCurve = speedCurve
        };
    }

    private Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        // Optimized cubic Bezier calculation
        float u = 1f - t;
        float uu = u * u;
        float uuu = uu * u;
        float tt = t * t;
        float ttt = tt * t;

        return uuu * p0 + 3f * uu * t * p1 + 3f * u * tt * p2 + ttt * p3;
    }

    private Vector2 CalculateMicroJitter(float movementProgress)
    {
        if (microJitterStrength <= 0f) return Vector2.zero;
        
        // Use Perlin noise for organic jitter
        float time = Time.time * 10f;
        float noiseX = (Mathf.PerlinNoise(time, 0f) - 0.5f) * 2f;
        float noiseY = (Mathf.PerlinNoise(0f, time) - 0.5f) * 2f;
        
        // Scale jitter based on personality and current state
        float jitterScale = microJitterStrength * personality.nervousness * (1f + currentTension);
        
        // Reduce jitter at start/end of movement for realism
        float progressFactor = Mathf.Sin(movementProgress * Mathf.PI);
        
        return new Vector2(noiseX, noiseY) * jitterScale * progressFactor;
    }

    #endregion

    #region Personality & State Management

    private void ApplyMoodModifiers(string mood)
    {
        switch (mood.ToLower())
        {
            case "urgent":
                currentSpeed = baseSpeed * 1.4f;
                currentTension = Mathf.Min(1f, currentTension + 0.2f);
                break;
            case "cautious":
                currentSpeed = baseSpeed * 0.7f;
                currentTension = Mathf.Min(1f, currentTension + 0.1f);
                break;
            case "relaxed":
                currentSpeed = baseSpeed * 0.9f;
                currentTension = Mathf.Max(0f, currentTension - 0.1f);
                break;
            default:
                currentSpeed = baseSpeed;
                break;
        }
    }

    private bool ShouldHesitate()
    {
        float hesitationProbability = hesitationChance * personality.patience * (1f + currentTension);
        return Random.value < hesitationProbability;
    }

    private float CalculateHesitationTime()
    {
        float baseHesitation = Random.Range(0.1f, 0.5f);
        float personalityFactor = personality.patience * (1f + currentFatigue);
        return baseHesitation * personalityFactor;
    }

    private float CalculateCurrentSpeed()
    {
        float fatigueReduction = 1f - (currentFatigue * 0.3f);
        float confidenceBoost = 1f + (currentConfidence * 0.2f);
        float tensionEffect = 1f + (currentTension * 0.1f); // Slight speed increase when tense
        
        return currentSpeed * fatigueReduction * confidenceBoost * tensionEffect;
    }

    private void UpdateFatigueFromMovement(float distance)
    {
        // Gradually increase fatigue based on movement distance
        float fatigueIncrease = distance * 0.00001f * (1f - personality.confidence);
        currentFatigue = Mathf.Min(1f, currentFatigue + fatigueIncrease);
        
        // Slowly recover confidence with successful movements
        currentConfidence = Mathf.Min(1f, currentConfidence + 0.001f);
    }

    #endregion

    #region Idle Behavior System

    private void StartIdleJitter()
    {
        if (idleJitterCoroutine != null)
            StopCoroutine(idleJitterCoroutine);
        idleJitterCoroutine = StartCoroutine(IdleJitterLoop());
    }

    private IEnumerator IdleJitterLoop()
    {
        while (true)
        {
            if (!isMoving)
            {
                yield return new WaitForSeconds(Random.Range(2f, 8f));
                
                if (!isMoving) // Double-check we're still idle
                {
                    // Subtle idle movement
                    Vector2 jitterOffset = Random.insideUnitCircle * Random.Range(1f, 3f);
                    Vector2 targetPos = currentPosition + jitterOffset;
                    
                    // Keep within screen bounds
                    targetPos.x = Mathf.Clamp(targetPos.x, 10f, Screen.width - 10f);
                    targetPos.y = Mathf.Clamp(targetPos.y, 10f, Screen.height - 10f);
                    
                    yield return StartCoroutine(ExecuteHumanMovement(targetPos));
                }
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    #endregion

    #region Click Simulation

    private IEnumerator PerformClickWithRealism(bool isDoubleClick)
    {
        // Pre-click micro-movement (human tendency to adjust before clicking)
        Vector2 microAdjustment = Random.insideUnitCircle * Random.Range(0.5f, 1.5f);
        SetPositionSafe(currentPosition + microAdjustment);
        
        yield return new WaitForSeconds(Random.Range(0.02f, 0.05f));
        
        // Play click sound
        if (Windows31DesktopManager.Instance != null)
        {
            Windows31DesktopManager.Instance.PlaySound(isDoubleClick ? "doubleclick" : "click");
        }
        
        // Post-click micro-movement (slight recoil)
        Vector2 recoil = Random.insideUnitCircle * Random.Range(0.3f, 0.8f);
        SetPositionSafe(currentPosition + recoil);
        
        yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
    }

    #endregion

    #region Position Management

    private void SetPositionSafe(Vector2 newPosition)
    {
        // Clamp to screen bounds with margin
        newPosition.x = Mathf.Clamp(newPosition.x, 5f, Screen.width - 5f);
        newPosition.y = Mathf.Clamp(newPosition.y, 5f, Screen.height - 5f);
        
        currentPosition = newPosition;
        if (cursorRect != null)
        {
            cursorRect.position = currentPosition;
        }
    }

    private void SetPositionImmediate(Vector2 target)
    {
        SetPositionSafe(target);
        lastFramePosition = currentPosition;
    }

    #endregion

    #region Performance & Memory Management

    private void UpdateMovementTracking()
    {
        // Track recent positions for analysis (limited queue for memory efficiency)
        if (Vector2.Distance(currentPosition, lastFramePosition) > 0.1f)
        {
            recentPositions.Enqueue(currentPosition);
            if (recentPositions.Count > 10)
                recentPositions.Dequeue();
            
            lastFramePosition = currentPosition;
        }
    }

    private void UpdateFatigueSystem()
    {
        // Gradual fatigue recovery during idle periods
        if (!isMoving)
        {
            currentFatigue = Mathf.Max(0f, currentFatigue - Time.deltaTime * 0.01f);
            currentTension = Mathf.Max(0f, currentTension - Time.deltaTime * 0.05f);
        }
    }

    private void OptimizedMemoryManagement()
    {
        // Periodic garbage collection during safe periods
        if (Time.time - lastGCTime > GC_INTERVAL && !isMoving)
        {
            System.GC.Collect();
            lastGCTime = Time.time;
        }
    }

    private void ReportActivity()
    {
        if (SimulationController.Instance != null)
            SimulationController.Instance.ReportActivity();
    }

    #endregion

    #region Public Accessors

    public Vector2 GetCurrentPosition() => currentPosition;
    public bool IsMoving() => isMoving;
    
    public void SetVisibility(bool isVisible)
    {
        if (cursorCanvas != null)
            cursorCanvas.gameObject.SetActive(isVisible);
    }

    public void SetTensionLevel(float tension)
    {
        currentTension = Mathf.Clamp01(tension);
    }

    public void SetConfidenceLevel(float confidence)
    {
        currentConfidence = Mathf.Clamp01(confidence);
    }

    public void ResetFatigue()
    {
        currentFatigue = 0f;
        currentConfidence = 0.8f;
        currentTension = 0f;
    }

    #endregion

    #region Cleanup

    void OnDestroy()
    {
        if (currentMovementCoroutine != null)
            StopCoroutine(currentMovementCoroutine);
        if (idleJitterCoroutine != null)
            StopCoroutine(idleJitterCoroutine);
    }

    #endregion
}