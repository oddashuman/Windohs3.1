using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// **New Script: Solitaire**
/// Vision: Implements the Solitaire game as a form of behavioral storytelling.
/// The AI doesn't just play to win; it plays in a way that reflects Orion's
/// current mental state—fast and confident, slow and contemplative, or erratic
/// and distracted when under pressure.
/// </summary>
public class Solitaire : MonoBehaviour
{
    public Window parentWindow;
    private Coroutine gameCoroutine;

    // Simplified game state
    private List<string> foundationPiles = new List<string> { "A♠", "A♥", "A♦", "A♣" };
    private string stock = "K♦ Q♣ J♥...";

    public void Initialize()
    {
        // For a real implementation, you would create card UI elements here.
        // For our narrative purpose, we just need to simulate the game's progress.
        Debug.Log("SOLITAIRE: Game started.");
        gameCoroutine = StartCoroutine(PlayGame());
    }

    private IEnumerator PlayGame()
    {
        while (true)
        {
            // The "thinking" delay is the most important part for character storytelling.
            float thinkingTime = GetThinkingTime();
            yield return new WaitForSeconds(thinkingTime);

            // Simulate making a move
            MakeMove();
        }
    }

    private float GetThinkingTime()
    {
        DialogueState state = DialogueState.Instance;
        float baseTime = Random.Range(2.0f, 5.0f);

        // Adjust timing based on Orion's mental state
        if (state.globalTension > 0.7f)
        {
            baseTime *= 1.5f; // Hesitates more when tense
        }
        if (state.metaAwareness > 0.6f)
        {
            baseTime *= 0.7f; // More focused and quicker when deep in thought
        }
        if (state.glitchCount > 5)
        {
            baseTime += Random.Range(3.0f, 6.0f); // Distracted by system instability
        }

        return baseTime;
    }

    private void MakeMove()
    {
        // In a full game, you'd move actual card objects.
        // Here, we just log the action to show the AI is "playing".
        Windows31DesktopManager.Instance.PlaySound("card");
        Debug.Log("SOLITAIRE: Orion made a move.");
    }

    void OnDestroy()
    {
        if (gameCoroutine != null)
        {
            StopCoroutine(gameCoroutine);
        }
    }
}