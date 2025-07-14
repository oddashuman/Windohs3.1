using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// **Complete Code: CursorController**
/// Vision: Provides extremely realistic human cursor movement. Includes personality
/// traits (hesitation, precision), fatigue simulation, and a high-level API for
/// the DesktopAI to direct Orion's actions with simple commands (e.g., "click this icon").
/// </summary>
public class CursorController : MonoBehaviour
{
    public static CursorController Instance { get; private set; }

    [Header("Cursor Visuals")]
    public Texture2D[] cursorTextures;
    private RectTransform cursorRect;
    private Image cursorImage;


    [Header("Movement Physics")]
    public float baseSpeed = 800f;
    public float hesitationChance = 0.15f;
    public AnimationCurve humanMovementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // State
    private Vector2 currentPosition;
    private bool isMoving = false;
    private Coroutine currentMovementCoroutine;


    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        InitializeCursor();
    }

    private void InitializeCursor()
    {
        GameObject cursorGO = new GameObject("OrionsCursor");
        cursorGO.transform.SetParent(transform, false);
        Canvas cursorCanvas = cursorGO.AddComponent<Canvas>();
        cursorCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        cursorCanvas.sortingOrder = 10000;

        cursorRect = cursorGO.AddComponent<RectTransform>();
        cursorRect.sizeDelta = new Vector2(32, 32);
        cursorRect.pivot = new Vector2(0, 1); // Top-left pivot for authentic feel

        cursorImage = cursorGO.AddComponent<Image>();
        if (cursorTextures != null && cursorTextures.Length > 0)
        {
            var sprite = Sprite.Create(cursorTextures[0], new Rect(0, 0, cursorTextures[0].width, cursorTextures[0].height), new Vector2(0, 1));
            cursorImage.sprite = sprite;
        }

        Cursor.visible = false;
        currentPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
        cursorRect.position = currentPosition;
    }

    #region Public AI API

    /// <summary>
    /// High-level command for the DesktopAI. Moves to a target and single-clicks.
    /// Mood affects speed and hesitation.
    /// </summary>
    public IEnumerator MoveToAndClick(Vector2 target, string mood = "casual")
    {
        isMoving = true;
        SetMood(mood);
        yield return StartCoroutine(ExecuteRealisticMovement(target));
        yield return StartCoroutine(PerformClick(false));
        isMoving = false;
    }

    /// <summary>
    /// High-level command for the DesktopAI. Moves to a target and double-clicks.
    /// </summary>
    public IEnumerator MoveToAndDoubleClick(Vector2 target, string mood = "casual")
    {
        isMoving = true;
        SetMood(mood);
        yield return StartCoroutine(ExecuteRealisticMovement(target));
        yield return StartCoroutine(PerformClick(true)); // First click
        yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
        yield return StartCoroutine(PerformClick(true)); // Second click
        isMoving = false;
    }

    /// <summary>
    /// High-level command for the DesktopAI. Moves the cursor to a UI element's handle
    /// and then drags it by a specified vector.
    /// </summary>
    public IEnumerator MoveAndDrag(Vector2 startHandle, Vector2 dragVector)
    {
        isMoving = true;
        // Move to the draggable handle first
        yield return StartCoroutine(ExecuteRealisticMovement(startHandle));

        // Simulate mouse down, drag, and mouse up
        Vector2 endPosition = startHandle + dragVector;
        yield return StartCoroutine(ExecuteRealisticMovement(endPosition, true)); // Pass true to signify dragging

        isMoving = false;
    }


    /// <summary>
    /// Sets the cursor's behavior based on a simple mood string from the AI.
    /// </summary>
    private void SetMood(string mood)
    {
        switch (mood.ToLower())
        {
            case "urgent":
                baseSpeed = 1200f;
                hesitationChance = 0.05f;
                break;
            case "cautious":
                baseSpeed = 600f;
                hesitationChance = 0.3f;
                break;
            case "casual":
            default:
                baseSpeed = 800f;
                hesitationChance = 0.15f;
                break;
        }
    }

    #endregion

    #region Core Movement Engine

    /// <summary>
    /// The main movement logic that simulates human-like motion.
    /// </summary>
    private IEnumerator ExecuteRealisticMovement(Vector2 target, bool isDragging = false)
    {
        // 1. Pre-movement hesitation
        if (Random.value < hesitationChance && !isDragging)
        {
            yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
        }

        // 2. Path Generation
        Vector2 startPosition = currentPosition;
        float distance = Vector2.Distance(startPosition, target);
        float moveTime = distance / baseSpeed;

        // Create a bezier control point for a natural arc
        Vector2 controlPoint = startPosition + (target - startPosition) / 2 + new Vector2(Random.Range(-distance / 4, distance / 4), Random.Range(-distance / 4, distance / 4));

        // 3. Move along the generated path
        float elapsedTime = 0;
        while (elapsedTime < moveTime)
        {
            float t = elapsedTime / moveTime;
            float curvedT = humanMovementCurve.Evaluate(t);

            // Quadratic Bezier curve for smooth, arcing motion
            currentPosition = (1 - curvedT) * (1 - curvedT) * startPosition + 2 * (1 - curvedT) * curvedT * controlPoint + curvedT * curvedT * target;
            
            cursorRect.position = currentPosition;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        currentPosition = target;
        cursorRect.position = currentPosition;
    }

    /// <summary>
    /// Simulates the physical action of a mouse click.
    /// </summary>
    private IEnumerator PerformClick(bool isDoubleClick)
    {
        // Use the sound system for click feedback
        Windows31DesktopManager.Instance.PlaySound(isDoubleClick ? "doubleclick" : "click");
        yield return new WaitForSeconds(0.1f);
    }
    
    // Direct move for glitch effects or instant repositions
    public void MoveTo(Vector2 target, bool immediate)
    {
         if(immediate)
         {
              currentPosition = target;
              cursorRect.position = currentPosition;
         }
         else
         {
              if(currentMovementCoroutine != null) StopCoroutine(currentMovementCoroutine);
              currentMovementCoroutine = StartCoroutine(ExecuteRealisticMovement(target));
         }
    }

    public Vector2 GetCurrentPosition() => currentPosition;

    #endregion
}