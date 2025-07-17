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
        
        // This ensures other managers are ready before this one starts
        yield return new WaitUntil(() => TopicManager.Instance != null && ConversationThreadManager.Instance != null && DialogueState.Instance != null);
        
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
            var participants = new List<CharacterProfile> { allCharacters["Orion"], allCharacters["Nova"] };
            var topic = topicManager.GetRandomTopic();
            currentThread = threadManager.StartThread(participants, topic);
        }

        CharacterProfile speaker = DetermineNextSpeaker(currentThread);
        if (speaker == null) return null;

        string replyText = replyEngine.BuildLine(speaker, currentThread, dialogueState);
        if (string.IsNullOrEmpty(replyText)) return null;

        var message = new DialogueMessage(speaker.name, replyText);
        currentThread.RegisterMessage(message);
        return message;
    }

    private CharacterProfile DetermineNextSpeaker(ConversationThread thread)
    {
        var potentialSpeakers = thread.participants.Where(p => p.name != thread.lastSpeaker).ToList();
        if (potentialSpeakers.Count == 0)
        {
            potentialSpeakers = thread.participants;
        }
        return potentialSpeakers[Random.Range(0, potentialSpeakers.Count)];
    }

    public void EnqueueUserMessage(string username, string message)
    {
        // Placeholder for future implementation where characters can react to user messages
        Debug.Log($"User message from {username}: {message}");
    }
}