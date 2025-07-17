using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DialogueEngine : MonoBehaviour
{
    public static DialogueEngine Instance { get; private set; }

    public Dictionary<string, CharacterProfile> allCharacters = new Dictionary<string, CharacterProfile>();
    private ProceduralReplyEngine replyEngine;
    private TopicManager topicManager;
    private ConversationThreadManager threadManager;
    private DialogueState dialogueState;
    private bool isReady = false;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(InitializeEngine());
    }

    private IEnumerator InitializeEngine()
    {
        Debug.Log("DIALOGUE_ENGINE: Initializing...");

        yield return new WaitUntil(() => TopicManager.Instance != null &&
                                       ConversationThreadManager.Instance != null &&
                                       DialogueState.Instance != null);

        topicManager = TopicManager.Instance;
        threadManager = ConversationThreadManager.Instance;
        dialogueState = DialogueState.Instance;
        replyEngine = new ProceduralReplyEngine();

        RegisterCharacters();
        isReady = true;
        Debug.Log("DIALOGUE_ENGINE: Ready.");
    }

    public bool IsReady() => isReady;

    private void RegisterCharacters()
    {
        allCharacters["Orion"] = new CharacterProfile("Orion");
        allCharacters["Nova"] = new CharacterProfile("Nova");
        allCharacters["Echo"] = new CharacterProfile("Echo");
        allCharacters["Lumen"] = new CharacterProfile("Lumen");
    }

    public DialogueMessage GetNextMessage()
    {
        if (!isReady) return null;

        ConversationThread currentThread = threadManager.GetActiveThread();
        if (currentThread == null)
        {
            var participants = new List<CharacterProfile> { allCharacters["Orion"], allCharacters["Nova"], allCharacters["Echo"], allCharacters["Lumen"] };
            var topic = dialogueState.globalTension > 0.6f ? topicManager.GetControversialOrForbidden() : topicManager.GetRandomTopic();
            currentThread = threadManager.StartThread(participants, topic);
        }

        CharacterProfile speaker = DetermineNextSpeaker(currentThread);
        if (speaker == null) return null;

        string replyText = replyEngine.BuildLine(speaker, currentThread, dialogueState);
        if (string.IsNullOrEmpty(replyText)) return null;

        var message = new DialogueMessage(speaker.name, replyText);
        currentThread.RegisterMessage(message);
        dialogueState.AddToNarrativeHistory("Dialogue", $"{speaker.name}: {replyText}");
        return message;
    }

    private CharacterProfile DetermineNextSpeaker(ConversationThread thread)
    {
        // Simple turn-based speaker selection for now
        int nextSpeakerIndex = (thread.participants.FindIndex(p => p.name == thread.lastSpeaker) + 1) % thread.participants.Count;
        return thread.participants[nextSpeakerIndex];
    }


    public void EnqueueUserMessage(string username, string message)
    {
        // This can now trigger narrative events
        Debug.Log($"User message from {username}: {message}");
        NarrativeTriggerManager.Instance.TriggerEvent("ViewerMessage", username);
    }
}