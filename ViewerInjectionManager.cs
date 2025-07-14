using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// **New Script: ViewerInjectionManager**
/// Vision: Implements the "viewer" interaction layer. This script simulates
/// messages and chat commands from an audience, allowing them to directly
/// influence the simulation's state by triggering glitches, increasing tension,
/// or asking questions, making the world feel responsive and alive.
/// </summary>
public class ViewerInjectionManager : MonoBehaviour
{
    public static ViewerInjectionManager Instance { get; private set; }

    [Header("Viewer Simulation")]
    public bool enableAutoChat = true;
    public float minChatInterval = 30f;
    public float maxChatInterval = 120f;

    private Dictionary<string, System.Action<string>> chatCommands;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        InitializeChatCommands();
        if (enableAutoChat)
        {
            StartCoroutine(SimulateViewerChat());
        }
    }

    private void InitializeChatCommands()
    {
        chatCommands = new Dictionary<string, System.Action<string>>
        {
            ["!glitch"] = (user) => NarrativeTriggerManager.Instance?.TriggerEvent("ViewerGlitchRequest", user),
            ["!tension"] = (user) => NarrativeTriggerManager.Instance?.TriggerEvent("ViewerTensionUp", user),
            ["!observe"] = (user) => NarrativeTriggerManager.Instance?.TriggerEvent("ViewerObserve", user),
            ["!question"] = (user) => InjectViewerMessage(user, "Are you really real?"),
        };
    }

    private IEnumerator SimulateViewerChat()
    {
        while (true)
        {
            float waitTime = Random.Range(minChatInterval, maxChatInterval);
            yield return new WaitForSeconds(waitTime);

            // Generate a random viewer name and message
            string viewerName = "Observer" + Random.Range(100, 999);
            string message = GetRandomViewerMessage();

            // Use a command a small percentage of the time
            if (Random.value < 0.2f)
            {
                message = "!glitch";
            }
            
            ProcessIncomingMessage(viewerName, message);
        }
    }

    /// <summary>
    /// Public entry point for external systems (like a real Twitch chat) to send messages.
    /// </summary>
    public void ProcessIncomingMessage(string username, string message)
    {
        Debug.Log($"VIEWER_INJECT: Received message from {username}: {message}");

        if (message.StartsWith("!") && chatCommands.ContainsKey(message))
        {
            // It's a command, execute the action
            chatCommands[message].Invoke(username);
        }
        else
        {
            // It's a regular message, enqueue it in the dialogue engine
            InjectViewerMessage(username, message);
        }
    }

    private void InjectViewerMessage(string username, string message)
    {
        DialogueState.Instance.observerDetected = true;
        DialogueEngine.Instance.EnqueueUserMessage(username, message);
    }
    
    private string GetRandomViewerMessage()
    {
        string[] messages = { "What is this place?", "Can he see us?", "Look at the files!", "He seems nervous.", "Try running solitaire." };
        return messages[Random.Range(0, messages.Length)];
    }
}