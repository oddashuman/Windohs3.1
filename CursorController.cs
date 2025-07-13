using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Enhanced cursor controller with extremely realistic human movement patterns.
/// Includes personality traits, fatigue simulation, and context-aware behaviors.
/// This is the KEY to making the experience feel like watching a real person.
/// </summary>
public class CursorController : MonoBehaviour
{
    public static CursorController Instance { get; private set; }

    [Header("Cursor Visual")]
    public RectTransform cursorObject;
    public Texture2D[] cursorTextures; // Different cursor states

    [Header("Movement Physics")]
    public float baseSpeed = 800f;
    public float accelerationTime = 0.4f;
    public float decelerationDistance = 120f;
    public AnimationCurve humanMovementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve precisionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Human Behavior Simulation")]
    public float hesitationChance = 0.15f;
    public float microCorrectionChance = 0.25f;
    public float overshotChance = 0.1f;
    public float idleMovementInterval = 4f;
    public float idleMovementRadius = 15f;

    [Header("Personality Traits")]
    public float orionCautiousness = 0.8f;
    public float orionPrecision = 0.9f;
    public float orionAnalyticalBehavior = 0.85f;

    [Header("Context Awareness")]
    public float terminalModeSpeedMultiplier = 0.8f;
    public float researchModeSpeedMultiplier = 0.7f;
    public float casualModeSpeedMultiplier = 1.2f;

    // Current state
    private Vector2 currentPosition;
    private Vector2 targetPosition;
    private Vector2 velocity;
    private bool isMoving = false;
    private float currentSpeed;
    private Coroutine currentMovement;
    private Coroutine idleMovementRoutine;

    // Behavior state
    private float tensionLevel = 0.3f;
    private float confidenceLevel = 0.7f;
    private float fatigueLevel = 0f;
    private float focusLevel = 0.8f;
    private bool isPrecisionMode = false;
    private CursorState currentState = CursorState.Arrow;
    private ContextMode currentContext = ContextMode.Desktop;

    // Advanced human behavior simulation
    private Queue<Vector2> movementHistory = new Queue<Vector2>();
    private const int maxHistoryLength = 10;
    private float lastMovementTime;
    private int consecutiveMovements = 0;
    private bool needsRest = false;

    // Click behavior
    private bool isClicking = false;
    private float lastClickTime = 0f;
    private float doubleClickWindow = 0.3f;
    private int clickCount = 0;

    // Micro-behaviors
    private float nextMicroMovementTime;
    private bool isInMicroCorrection = false;
    private Vector2 targetBeforeCorrection;

    // Activity-specific behaviors
    private Dictionary<string, MovementProfile> activityProfiles;

    public enum CursorState
    {
        Arrow, Hand, Text, Wait, Busy, Precision
    }

    public enum ContextMode
    {
        Desktop, Terminal, FileManager, Notepad, Gaming, Research
    }

    [System.Serializable]
    public class MovementProfile
    {
        public float speedMultiplier = 1f;
        public float hesitationMultiplier = 1f;
        public float precisionRequirement = 0.5f;
        public bool allowsIdleMovement = true;
        public float fatigueRate = 0.001f;
    }

    public System.Action<Vector2> OnCursorMove;
    public System.Action<Vector2> OnCursorClick;
    public System.Action<Vector2> OnCursorDoubleClick;
    public System.Action<Vector2> OnCursorRightClick;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeCursor();
        InitializeActivityProfiles();
    }

    void Start()
    {
        StartIdleMovement();
    }

    void Update()
    {
        UpdateBehaviorState();
        HandleIdleMovements();
        UpdateFatigue();
        HandleDebugInput();
    }

    #region Initialization

    void InitializeCursor()
    {
        if (cursorObject == null)
        {
            CreateCursorObject();
        }

        // Hide system cursor
        Cursor.visible = false;

        // Initialize at screen center
        currentPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        UpdateCursorPosition();
    }

    void CreateCursorObject()
    {
        GameObject cursorGO = new GameObject("OrionsCursor");
        cursorGO.transform.SetParent(transform);

        Canvas cursorCanvas = cursorGO.AddComponent<Canvas>();
        cursorCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        cursorCanvas.sortingOrder = 10000; // Always on top

        cursorObject = cursorGO.GetComponent<RectTransform>();
        cursorObject.sizeDelta = new Vector2(32, 32);

        var cursorImage = cursorGO.AddComponent<UnityEngine.UI.Image>();
        if (cursorTextures != null && cursorTextures.Length > 0)
        {
            var sprite = Sprite.Create(cursorTextures[0],
                new Rect(0, 0, cursorTextures[0].width, cursorTextures[0].height),
                new Vector2(0, 1)); // Hotspot at top-left like Windows
            cursorImage.sprite = sprite;
        }
        else
        {
            cursorImage.color = Color.white; // Fallback
        }
    }

    void InitializeActivityProfiles()
    {
        activityProfiles = new Dictionary<string, MovementProfile>
        {
            ["desktop"] = new MovementProfile
            {
                speedMultiplier = 1f,
                hesitationMultiplier = 0.8f,
                precisionRequirement = 0.6f,
                allowsIdleMovement = true,
                fatigueRate = 0.0005f
            },
            ["terminal"] = new MovementProfile
            {
                speedMultiplier = 0.7f,
                hesitationMultiplier = 1.5f,
                precisionRequirement = 0.9f,
                allowsIdleMovement = false,
                fatigueRate = 0.002f
            },
            ["file_management"] = new MovementProfile
            {
                speedMultiplier = 0.9f,
                hesitationMultiplier = 0.6f,
                precisionRequirement = 0.7f,
                allowsIdleMovement = true,
                fatigueRate = 0.001f
            },
            ["research"] = new MovementProfile
            {
                speedMultiplier = 0.6f,
                hesitationMultiplier = 2f,
                precisionRequirement = 0.8f,
                allowsIdleMovement = false,
                fatigueRate = 0.003f
            },
            ["gaming"] = new MovementProfile
            {
                speedMultiplier = 1.3f,
                hesitationMultiplier = 0.3f,
                precisionRequirement = 0.4f,
                allowsIdleMovement = true,
                fatigueRate = 0.0002f
            }
        };
    }

    #endregion

    #region State Management

    void UpdateBehaviorState()
    {
        // Update based on dialogue state
        var dialogueState = DialogueState.Instance;
        if (dialogueState != null)
        {
            tensionLevel = Mathf.Lerp(tensionLevel, dialogueState.globalTension, Time.deltaTime * 0.5f);
            
            // Tension affects movement characteristics
            if (tensionLevel > 0.7f)
            {
                currentSpeed = baseSpeed * 0.8f; // Slower when tense
                hesitationChance = 0.25f;
            }
            else if (tensionLevel < 0.3f)
            {
                currentSpeed = baseSpeed * 1.1f; // Faster when relaxed
                hesitationChance = 0.1f;
            }
            else
            {
                currentSpeed = baseSpeed;
                hesitationChance = 0.15f;
            }
        }

        // Apply context modifications
        ApplyContextModifiers();

        // Fatigue accumulates over time
        if (isMoving)
        {
            string currentActivity = GetCurrentActivity();
            if (activityProfiles.ContainsKey(currentActivity))
            {
                fatigueLevel += activityProfiles[currentActivity].fatigueRate * Time.deltaTime;
            }
        }

        fatigueLevel = Mathf.Clamp01(fatigueLevel);
        
        // Fatigue affects speed and precision
        currentSpeed *= Mathf.Lerp(1f, 0.7f, fatigueLevel);
        
        // Check if Orion needs a break
        if (fatigueLevel > 0.8f && !needsRest)
        {
            needsRest = true;
            StartCoroutine(TakeBreak());
        }
    }

    void ApplyContextModifiers()
    {
        string activity = GetCurrentActivity();
        if (activityProfiles.ContainsKey(activity))
        {
            var profile = activityProfiles[activity];
            currentSpeed *= profile.speedMultiplier;
            hesitationChance *= profile.hesitationMultiplier;
            
            if (profile.precisionRequirement > 0.8f)
            {
                isPrecisionMode = true;
            }
        }
    }

    string GetCurrentActivity()
    {
        if (currentContext == ContextMode.Terminal) return "terminal";
        if (currentContext == ContextMode.FileManager) return "file_management";
        if (currentContext == ContextMode.Notepad) return "research";
        if (currentContext == ContextMode.Gaming) return "gaming";
        return "desktop";
    }

    #endregion

    #region Movement System - The Core of Realism

    public void MoveTo(Vector2 target, bool immediate = false)
    {
        if (immediate)
        {
            StopCurrentMovement();
            currentPosition = target;
            targetPosition = target;
            UpdateCursorPosition();
            isMoving = false;
            return;
        }

        targetPosition = target;
        
        StopCurrentMovement();
        currentMovement = StartCoroutine(ExecuteRealisticMovement());
    }

    IEnumerator ExecuteRealisticMovement()
    {
        isMoving = true;
        Vector2 startPosition = currentPosition;
        float totalDistance = Vector2.Distance(startPosition, targetPosition);
        
        if (totalDistance < 5f)
        {
            isMoving = false;
            yield break;
        }

        // Pre-movement analysis and hesitation
        yield return StartCoroutine(PreMovementBehavior());

        // Generate realistic movement path
        List<Vector2> waypoints = GenerateHumanMovementPath(startPosition, targetPosition);
        
        // Execute movement through waypoints
        yield return StartCoroutine(ExecutePathMovement(waypoints));
        
        // Post-movement corrections
        yield return StartCoroutine(PostMovementBehavior());

        // Record movement for learning
        RecordMovement(targetPosition);
        
        isMoving = false;
    }

    IEnumerator PreMovementBehavior()
    {
        // Orion's analytical pause before important movements
        if (isPrecisionMode && Random.value < orionAnalyticalBehavior)
        {
            yield return new WaitForSeconds(Random.Range(0.1f, 0.4f));
        }

        // Hesitation based on tension and context
        float hesitationProbability = hesitationChance * tensionLevel;
        if (Random.value < hesitationProbability)
        {
            yield return StartCoroutine(SimulateHesitation());
        }

        // Check for distractions (simulate human attention)
        if (Random.value < 0.05f && currentContext != ContextMode.Terminal)
        {
            yield return StartCoroutine(SimulateDistraction());
        }
    }

    IEnumerator SimulateHesitation()
    {
        float hesitationTime = Random.Range(0.2f, 0.8f) * tensionLevel;
        Vector2 originalPos = currentPosition;
        
        // Small oscillating movement during hesitation
        float elapsed = 0f;
        while (elapsed < hesitationTime)
        {
            float offset = Mathf.Sin(elapsed * 15f) * 2f;
            currentPosition = originalPos + new Vector2(offset, Random.Range(-1f, 1f));
            UpdateCursorPosition();
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        currentPosition = originalPos;
        UpdateCursorPosition();
    }

    IEnumerator SimulateDistraction()
    {
        Vector2 originalTarget = targetPosition;
        Vector2 distractionPoint = currentPosition + new Vector2(
            Random.Range(-100f, 100f),
            Random.Range(-50f, 50f)
        );
        
        // Quick glance movement
        yield return StartCoroutine(QuickMoveTo(distractionPoint));
        yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        
        // Return focus to original target
        targetPosition = originalTarget;
    }

    IEnumerator QuickMoveTo(Vector2 target)
    {
        Vector2 start = currentPosition;
        float distance = Vector2.Distance(start, target);
        float moveTime = distance / (currentSpeed * 2f); // Faster for distraction
        
        float elapsed = 0f;
        while (elapsed < moveTime)
        {
            float progress = elapsed / moveTime;
            currentPosition = Vector2.Lerp(start, target, humanMovementCurve.Evaluate(progress));
            UpdateCursorPosition();
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        currentPosition = target;
        UpdateCursorPosition();
    }

    List<Vector2> GenerateHumanMovementPath(Vector2 start, Vector2 end)
    {
        List<Vector2> waypoints = new List<Vector2> { start };
        
        float distance = Vector2.Distance(start, end);
        Vector2 direction = (end - start).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);
        
        // Orion's movement characteristics
        float orionCurvature = orionPrecision * 0.5f; // More precise = less curved
        float orionVariance = (1f - orionCautiousness) * 20f; // More cautious = less variance
        
        // Add waypoints for longer movements
        if (distance > 150f)
        {
            int waypointCount = Mathf.FloorToInt(distance / 150f);
            waypointCount = Mathf.Clamp(waypointCount, 1, 4);
            
            for (int i = 1; i < waypointCount; i++)
            {
                float progress = (float)i / waypointCount;
                Vector2 basePoint = Vector2.Lerp(start, end, progress);
                
                // Natural curve with Orion's personality
                float curveAmount = Mathf.Sin(progress * Mathf.PI) * (30f * orionCurvature);
                float variance = (Random.value - 0.5f) * orionVariance;
                
                Vector2 waypoint = basePoint + perpendicular * (curveAmount + variance);
                
                // Ensure waypoint stays on screen
                waypoint.x = Mathf.Clamp(waypoint.x, 10, Screen.width - 10);
                waypoint.y = Mathf.Clamp(waypoint.y, 10, Screen.height - 10);
                
                waypoints.Add(waypoint);
            }
        }
        
        // Micro-correction near target for precision tasks
        if (isPrecisionMode && Random.value < microCorrectionChance)
        {
            Vector2 nearTarget = end + new Vector2(
                Random.Range(-5f, 5f),
                Random.Range(-5f, 5f)
            );
            waypoints.Add(nearTarget);
        }
        
        waypoints.Add(end);
        return waypoints;
    }

    IEnumerator ExecutePathMovement(List<Vector2> waypoints)
    {
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            Vector2 segmentStart = waypoints[i];
            Vector2 segmentEnd = waypoints[i + 1];
            
            yield return StartCoroutine(MoveToWaypoint(segmentStart, segmentEnd));
            
            // Brief pause between segments for longer paths
            if (waypoints.Count > 3 && i < waypoints.Count - 2)
            {
                yield return new WaitForSeconds(Random.Range(0.02f, 0.08f));
            }
        }
    }

    IEnumerator MoveToWaypoint(Vector2 start, Vector2 end)
    {
        float distance = Vector2.Distance(start, end);
        float baseTime = distance / currentSpeed;
        
        // Apply Orion's movement timing preferences
        float moveTime = baseTime * Random.Range(0.8f, 1.2f);
        
        // Fatigue affects timing
        moveTime *= (1f + fatigueLevel * 0.5f);
        
        float elapsed = 0f;
        while (elapsed < moveTime)
        {
            float progress = elapsed / moveTime;
            
            // Use different curves based on precision mode
            AnimationCurve curve = isPrecisionMode ? precisionCurve : humanMovementCurve;
            float curveProgress = curve.Evaluate(progress);
            
            currentPosition = Vector2.Lerp(start, end, curveProgress);
            
            // Add subtle jitter for realism
            if (!isPrecisionMode)
            {
                currentPosition += new Vector2(
                    (Random.value - 0.5f) * 1.5f,
                    (Random.value - 0.5f) * 1.5f
                );
            }
            
            UpdateCursorPosition();
            OnCursorMove?.Invoke(currentPosition);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        currentPosition = end;
        UpdateCursorPosition();
    }

    IEnumerator PostMovementBehavior()
    {
        // Overshoot correction for precision tasks
        if (isPrecisionMode && Random.value < overshotChance)
        {
            Vector2 overshoot = targetPosition + new Vector2(
                Random.Range(-8f, 8f),
                Random.Range(-8f, 8f)
            );
            
            currentPosition = overshoot;
            UpdateCursorPosition();
            
            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
            
            // Correct back to target
            yield return StartCoroutine(QuickMoveTo(targetPosition));
        }
        
        // Final precision adjustment
        if (Vector2.Distance(currentPosition, targetPosition) > 3f)
        {
            yield return StartCoroutine(QuickMoveTo(targetPosition));
        }
    }

    #endregion

    #region Click Behavior

    public void Click(Vector2? position = null, bool isDoubleClick = false)
    {
        StartCoroutine(PerformClick(position, isDoubleClick, false));
    }

    public void RightClick(Vector2? position = null)
    {
        StartCoroutine(PerformClick(position, false, true));
    }

    IEnumerator PerformClick(Vector2? position, bool isDoubleClick, bool isRightClick)
    {
        if (position.HasValue)
        {
            MoveTo(position.Value);
            yield return new WaitUntil(() => !isMoving);
        }
        
        // Pre-click hesitation for important actions
        if (isPrecisionMode && tensionLevel > 0.5f && Random.value < 0.4f)
        {
            yield return new WaitForSeconds(Random.Range(0.1f, 0.4f));
        }
        
        isClicking = true;
        clickCount++;
        
        // Visual click feedback
        yield return StartCoroutine(ClickAnimation());
        
        // Determine click type and invoke events
        if (isDoubleClick)
        {
            OnCursorDoubleClick?.Invoke(currentPosition);
        }
        else if (isRightClick)
        {
            OnCursorRightClick?.Invoke(currentPosition);
        }
        else
        {
            // Check for accidental double-click
            if (Time.time - lastClickTime < doubleClickWindow && Random.value < 0.1f)
            {
                OnCursorDoubleClick?.Invoke(currentPosition);
            }
            else
            {
                OnCursorClick?.Invoke(currentPosition);
            }
        }
        
        lastClickTime = Time.time;
        isClicking = false;
        
        // Track clicking patterns for fatigue
        if (clickCount > 20)
        {
            fatigueLevel += 0.05f;
            clickCount = 0;
        }
    }

    IEnumerator ClickAnimation()
    {
        Vector2 originalPos = currentPosition;
        Vector2 clickPos = originalPos + new Vector2(1, -1);
        
        // Down movement
        float downTime = 0.05f;
        float elapsed = 0f;
        
        while (elapsed < downTime)
        {
            currentPosition = Vector2.Lerp(originalPos, clickPos, elapsed / downTime);
            UpdateCursorPosition();
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        yield return new WaitForSeconds(0.03f);
        
        // Up movement
        elapsed = 0f;
        while (elapsed < downTime)
        {
            currentPosition = Vector2.Lerp(clickPos, originalPos, elapsed / downTime);
            UpdateCursorPosition();
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        currentPosition = originalPos;
        UpdateCursorPosition();
    }

    #endregion

    #region Idle Behavior

    public void StartIdleMovement()
    {
        if (idleMovementRoutine != null)
            StopCoroutine(idleMovementRoutine);
        idleMovementRoutine = StartCoroutine(IdleMovementLoop());
    }

    public void StopIdleMovement()
    {
        if (idleMovementRoutine != null)
        {
            StopCoroutine(idleMovementRoutine);
            idleMovementRoutine = null;
        }
    }

    IEnumerator IdleMovementLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(idleMovementInterval + Random.Range(-1f, 2f));
            
            if (!isMoving && !isClicking && ShouldDoIdleMovement())
            {
                yield return StartCoroutine(PerformIdleMovement());
            }
        }
    }

    bool ShouldDoIdleMovement()
    {
        string activity = GetCurrentActivity();
        if (!activityProfiles.ContainsKey(activity))
            return true;
            
        return activityProfiles[activity].allowsIdleMovement;
    }

    IEnumerator PerformIdleMovement()
    {
        Vector2 idleTarget = currentPosition + new Vector2(
            Random.Range(-idleMovementRadius, idleMovementRadius),
            Random.Range(-idleMovementRadius, idleMovementRadius)
        );
        
        // Keep idle movement on screen
        idleTarget.x = Mathf.Clamp(idleTarget.x, 20, Screen.width - 20);
        idleTarget.y = Mathf.Clamp(idleTarget.y, 20, Screen.height - 20);
        
        yield return StartCoroutine(QuickMoveTo(idleTarget));
    }

    void HandleIdleMovements()
    {
        // Micro-movements when stationary
        if (!isMoving && Time.time > nextMicroMovementTime)
        {
            nextMicroMovementTime = Time.time + Random.Range(2f, 8f);
            
            if (Random.value < 0.3f)
            {
                StartCoroutine(MicroMovement());
            }
        }
    }

    IEnumerator MicroMovement()
    {
        Vector2 originalPos = currentPosition;
        Vector2 microTarget = originalPos + new Vector2(
            Random.Range(-3f, 3f),
            Random.Range(-3f, 3f)
        );
        
        float microTime = Random.Range(0.3f, 0.8f);
        float elapsed = 0f;
        
        while (elapsed < microTime)
        {
            float progress = elapsed / microTime;
            currentPosition = Vector2.Lerp(originalPos, microTarget, 
                Mathf.Sin(progress * Mathf.PI));
            UpdateCursorPosition();
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        currentPosition = originalPos;
        UpdateCursorPosition();
    }

    #endregion

    #region Fatigue and Rest System

    void UpdateFatigue()
    {
        // Natural fatigue recovery over time
        if (!isMoving && !needsRest)
        {
            fatigueLevel -= Time.deltaTime * 0.01f;
            fatigueLevel = Mathf.Max(0f, fatigueLevel);
        }
    }

    IEnumerator TakeBreak()
    {
        Debug.Log("Orion's cursor needs a break due to fatigue");
        
        // Stop all movement temporarily
        StopCurrentMovement();
        
        // Brief pause
        yield return new WaitForSeconds(Random.Range(2f, 5f));
        
        // Gradual recovery
        while (fatigueLevel > 0.3f)
        {
            fatigueLevel -= Time.deltaTime * 0.05f;
            yield return null;
        }
        
        needsRest = false;
        Debug.Log("Orion feels refreshed and ready to continue");
    }

    #endregion

    #region Utility Methods

    void StopCurrentMovement()
    {
        if (currentMovement != null)
        {
            StopCoroutine(currentMovement);
            currentMovement = null;
        }
    }

    void UpdateCursorPosition()
    {
        if (cursorObject != null)
        {
            cursorObject.position = currentPosition;
        }
    }

    void RecordMovement(Vector2 position)
    {
        movementHistory.Enqueue(position);
        while (movementHistory.Count > maxHistoryLength)
        {
            movementHistory.Dequeue();
        }
        
        lastMovementTime = Time.time;
        consecutiveMovements++;
    }

    void HandleDebugInput()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButton(0))
        {
            Vector2 mousePos = Input.mousePosition;
            MoveTo(mousePos, true);
        }
    }

    #endregion

    #region Public API

    public Vector2 GetCurrentPosition() => currentPosition;
    public bool IsMoving() => isMoving;
    public bool IsClicking() => isClicking;
    
    public void SetTensionLevel(float tension)
    {
        tensionLevel = Mathf.Clamp01(tension);
    }
    
    public void SetConfidenceLevel(float confidence)
    {
        confidenceLevel = Mathf.Clamp01(confidence);
    }
    
    public void SetPrecisionMode(bool enabled)
    {
        isPrecisionMode = enabled;
        currentState = enabled ? CursorState.Precision : CursorState.Arrow;
        UpdateCursorAppearance();
    }
    
    public void SetContextMode(ContextMode mode)
    {
        currentContext = mode;
        ApplyContextModifiers();
    }
    
    public void AddFatigue(float amount)
    {
        fatigueLevel = Mathf.Clamp01(fatigueLevel + amount);
    }
    
    public void ResetFatigue()
    {
        fatigueLevel = 0f;
        needsRest = false;
    }

    void UpdateCursorAppearance()
    {
        if (cursorObject != null && cursorTextures != null && (int)currentState < cursorTextures.Length)
        {
            var image = cursorObject.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                var texture = cursorTextures[(int)currentState];
                var sprite = Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0, 1));
                image.sprite = sprite;
            }
        }
    }

    public string GetDebugInfo()
    {
        return $"Position: {currentPosition}, Moving: {isMoving}, " +
               $"Tension: {tensionLevel:F2}, Fatigue: {fatigueLevel:F2}, " +
               $"Precision: {isPrecisionMode}, Context: {currentContext}, " +
               $"Consecutive Moves: {consecutiveMovements}, Needs Rest: {needsRest}";
    }

    #endregion

    void OnDestroy()
    {
        // Restore system cursor
        Cursor.visible = true;
        
        if (Instance == this)
            Instance = null;
    }
}