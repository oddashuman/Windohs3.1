using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// **Complete Code: DialogueEngine**
/// Vision: The central narrative director. This engine manages conversation flow,
/// intelligently selects speakers based on personality, relationships, and narrative
/// context, and uses the ProceduralReplyEngine to generate dynamic, character-driven
/// dialogue that feels authentic and unscripted.
/// </summary>
public class DialogueEngine : MonoBehaviour
{
    public static DialogueEngine Instance { get; private set; }

    // Core Components
    public Dictionary<string, CharacterProfile> allCharacters = new Dictionary<string, CharacterProfile>();
    public ProceduralReplyEngine replyEngine;
    public TopicManager topicManager;
    public ConversationThreadManager threadManager;
    private DialogueState dialogueState;

    // Conversation State
    private ConversationThread currentThread;
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
        
        // Link to other singletons
        topicManager = TopicManager.Instance;
        threadManager = ConversationThreadManager.Instance;
        dialogueState = DialogueState.Instance;

        replyEngine = new ProceduralReplyEngine();

        RegisterCharacters();

        // Wait for all dependencies to be ready
        yield return new WaitUntil(() => topicManager != null && threadManager != null && dialogueState != null);
        
        isReady = true;
        Debug.Log("DIALOGUE_ENGINE: Ready.");
    }

    private void RegisterCharacters()
    {
        var characterNames = new[] { "Orion", "Nova", "Echo", "Lumen" };
        foreach (var name in characterNames)
        {
            allCharacters[name] = new CharacterProfile(name);
        }
    }

    /// <summary>
    /// The main public method to get the next line of dialogue for the terminal.
    /// </summary>
    public DialogueMessage GetNextMessage()
    {
        if (!isReady) return null;

        // 1. Manage Conversation Thread
        if (ShouldStartNewThread())
        {
            currentThread = StartNewThread();
            if (currentThread == null) return null; // Could not start a thread
        }

        // 2. Determine Next Speaker
        CharacterProfile speaker = DetermineNextSpeaker();
        if (speaker == null) return null;

        // 3. Update Speaker's State
        speaker.UpdateMood();
        
        // 4. Generate Reply
        string replyText = replyEngine.BuildLine(speaker, currentThread, dialogueState);
        if (string.IsNullOrEmpty(replyText)) return null;

        // 5. Create and Register Message
        var message = new DialogueMessage(speaker.name, replyText);
        currentThread.RegisterMessage(message);

        return message;
    }

    private bool ShouldStartNewThread()
    {
        return currentThread == null || currentThread.IsStale() || currentThread.turnCount > 30;
    }

    private ConversationThread StartNewThread()
    {
        // For now, we'll create a simple thread with 2-3 participants
        var participants = new List<CharacterProfile> { allCharacters["Orion"] };
        var others = allCharacters.Values.Where(p => p.name != "Orion").ToList();
        participants.Add(others[Random.Range(0, others.Count)]);
        if (Random.value > 0.5f)
        {
             participants.Add(others.Where(p => !participants.Contains(p)).ToList()[0]);
        }
        
        Topic topic = topicManager.GetRandomTopic(); // Get a starting topic
        return threadManager.StartThread(participants, topic);
    }

    private CharacterProfile DetermineNextSpeaker()
    {
        if (currentThread == null || currentThread.participants.Count == 0) return null;

        // Weighted selection logic
        Dictionary<CharacterProfile, float> speakerWeights = new Dictionary<CharacterProfile, float>();

        foreach (var participant in currentThread.participants)
        {
            float weight = 1.0f;
            var profile = allCharacters[participant.name];

            // Don't let the same person speak twice in a row (usually)
            if (participant.name == currentThread.lastSpeaker)
            {
                weight *= 0.1f;
            }

            // Personality influence
            weight *= (1.0f + profile.extraversion); // Extroverts speak more
            if (profile.mood == CharacterProfile.Mood.Frustrated || profile.mood == CharacterProfile.Inspired)
            {
                weight *= 1.5f; // Strong emotions lead to speaking out
            }

            // Narrative context
            if (dialogueState.globalTension > 0.7f && profile.neuroticism > 0.7f) // Anxious characters speak up when tense
            {
                 weight *= 1.8f;
            }

            speakerWeights[profile] = weight;
        }

        // Select speaker based on weights
        float totalWeight = speakerWeights.Values.Sum();
        float randomValue = Random.Range(0, totalWeight);

        foreach (var entry in speakerWeights)
        {
            if (randomValue < entry.Value)
            {
                return entry.Key;
            }
            randomValue -= entry.Value;
        }

        return allCharacters[currentThread.participants[0]]; // Fallback
    }
    
    public bool IsReady() => isReady;
}