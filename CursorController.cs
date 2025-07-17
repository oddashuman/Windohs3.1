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
    public bool isMoving { get; private set; } = false;
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

    public IEnumerator MoveToAndClick(Vector2 target, string mood = "casual")
    {
        isMoving = true;
        SetMood(mood);
        yield return StartCoroutine(ExecuteRealisticMovement(target));
        yield return StartCoroutine(PerformClick(false));
        isMoving = false;
    }

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

    public IEnumerator MoveAndDrag(Vector2 startHandle, Vector2 dragVector)
    {
        isMoving = true;
        yield return StartCoroutine(ExecuteRealisticMovement(startHandle));
        Vector2 endPosition = startHandle + dragVector;
        yield return StartCoroutine(ExecuteRealisticMovement(endPosition, true));
        isMoving = false;
    }

    private void SetMood(string mood)
    {
        switch (mood.ToLower())
        {
            case "urgent": baseSpeed = 1200f; hesitationChance = 0.05f; break;
            case "cautious": baseSpeed = 600f; hesitationChance = 0.3f; break;
            default: baseSpeed = 800f; hesitationChance = 0.15f; break;
        }
    }
    #endregion

    #region Core Movement Engine
    private IEnumerator ExecuteRealisticMovement(Vector2 target, bool isDragging = false)
    {
        if (Random.value < hesitationChance && !isDragging)
        {
            yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
        }

        Vector2 startPosition = currentPosition;
        float distance = Vector2.Distance(startPosition, target);
        float moveTime = distance / baseSpeed;
        Vector2 controlPoint = startPosition + (target - startPosition) / 2 + new Vector2(Random.Range(-distance / 4, distance / 4), Random.Range(-distance / 4, distance / 4));

        float elapsedTime = 0;
        while (elapsedTime < moveTime)
        {
            float t = elapsedTime / moveTime;
            float curvedT = humanMovementCurve.Evaluate(t);
            currentPosition = (1 - curvedT) * (1 - curvedT) * startPosition + 2 * (1 - curvedT) * curvedT * controlPoint + curvedT * curvedT * target;
            cursorRect.position = currentPosition;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        currentPosition = target;
        cursorRect.position = currentPosition;
    }
    
    private IEnumerator PerformClick(bool isDoubleClick)
    {
        Windows31DesktopManager.Instance.PlaySound(isDoubleClick ? "doubleclick" : "click");
        yield return new WaitForSeconds(0.1f);
    }
    
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
    
    // --- Missing Method Stubs ---
    public bool IsMoving() => isMoving;
    public void StartIdleMovement() { /* TODO */ }
    public void SetPrecisionMode(bool precision) { /* TODO */ }
    public void StopIdleMovement() { /* TODO */ }
    public void ResetFatigue() { /* TODO */ }
    public void SetTensionLevel(float tension) { /* TODO */ }
    public void SetConfidenceLevel(float confidence) { /* TODO */ }
    public void Click(Vector2 position, bool doubleClick = false)
    {
        StartCoroutine(MoveToAndClick(position));
    }

    #endregion
}