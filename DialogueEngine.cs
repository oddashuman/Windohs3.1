using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CharacterType
{
    Orion, Nova, Echo, Lumen, Overseer, Regular
}

public class DialogueMessage
{
    public string speaker;
    public string text;
    
    public DialogueMessage(string speaker, string text)
    {
        this.speaker = speaker;
        this.text = text;
    }
}

public class DialogueEngine : MonoBehaviour
{
    public static DialogueEngine Instance;

    // Core components
    public Dictionary<string, CharacterProfile> allCharacters = new Dictionary<string, CharacterProfile>();
    public List<CharacterProfile> mainCast = new List<CharacterProfile>();
    public ProceduralReplyEngine replyEngine;
    public TopicManager topicManager;
    public ConversationThreadManager threadManager;

    // Message management
    private Queue<string> recentLines = new Queue<string>();
    private Queue<DialogueMessage> messageHistory = new Queue<DialogueMessage>();
    private Queue<UserMessage> userMessageQueue = new Queue<UserMessage>();
    public int antiRepeatBufferSize = 64;
    public int maxMessageHistory = 50;

    // Conversation state
    private ConversationThread currentThread;
    private float lastDialogueTime = 0f;
    private float basePacing = 2.0f;
    private int messagesInCurrentThread = 0;
    private bool conversationPaused = false;

    // Enhanced dynamics
    private float tensionLevel = 0f;
    private float cohesionLevel = 1f;
    private bool inDeepDiscussion = false;
    private Dictionary<string, float> lastSpeechTimes = new Dictionary<string, float>();
    private Dictionary<string, Dictionary<string, float>> characterRelationships = new Dictionary<string, Dictionary<string, float>>();

    // Pacing system
    private Dictionary<ConversationPhase, float> phasePacingMultipliers = new Dictionary<ConversationPhase, float>
    {
        {ConversationPhase.Introduction, 1.1f},
        {ConversationPhase.Development, 1.0f},
        {ConversationPhase.Complication, 0.8f},
        {ConversationPhase.Climax, 0.5f},
        {ConversationPhase.Resolution, 1.0f}
    };

    // Viewer tracking
    private float lastViewerInteraction = 0f;
    private int consecutiveUserMessages = 0;

    // Initialization
    private bool isFullyInitialized = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        Instance = this;
        InitializeCore();
    }

    void Start()
    {
        StartCoroutine(CompleteInitializationAfterFrame());
    }

    private System.Collections.IEnumerator CompleteInitializationAfterFrame()
    {
        yield return null;
        CompleteInitialization();
    }

    private void InitializeCore()
    {
        replyEngine = new ProceduralReplyEngine();
        userMessageQueue = new Queue<UserMessage>();
        recentLines = new Queue<string>();
        messageHistory = new Queue<DialogueMessage>();
        characterRelationships = new Dictionary<string, Dictionary<string, float>>();
        lastSpeechTimes = new Dictionary<string, float>();
        
        RegisterMainCast();
    }

    private void CompleteInitialization()
    {
        EnsureManagersExist();
        InitializeRelationships();
        currentThread = null;
        isFullyInitialized = true;
        
        Debug.Log("DialogueEngine fully initialized");
    }

    private void EnsureManagersExist()
    {
        if (topicManager == null)
        {
            topicManager = TopicManager.Instance;
            if (topicManager == null)
            {
                topicManager = FindObjectOfType<TopicManager>();
                if (topicManager == null)
                {
                    GameObject tmGO = new GameObject("TopicManager");
                    tmGO.transform.SetParent(this.transform);
                    topicManager = tmGO.AddComponent<TopicManager>();
                }
            }
        }

        if (threadManager == null)
        {
            threadManager = ConversationThreadManager.Instance;
            if (threadManager == null)
            {
                threadManager = FindObjectOfType<ConversationThreadManager>();
                if (threadManager == null)
                {
                    GameObject ctmGO = new GameObject("ConversationThreadManager");
                    ctmGO.transform.SetParent(this.transform);
                    threadManager = ctmGO.AddComponent<ConversationThreadManager>();
                }
            }
        }

        if (DialogueState.Instance == null)
        {
            GameObject dsGO = new GameObject("DialogueState");
            dsGO.transform.SetParent(this.transform);
            dsGO.AddComponent<DialogueState>();
        }
    }

    void RegisterMainCast()
    {
        var orion = new CharacterProfile("Orion", CharacterType.Orion)
        {
            curiosity = 0.9f,
            suspicion = 0.7f,
            paranoia = 0.3f,
            playfulness = 0.1f,
            fear = 0.2f,
            mood = Mood.Curious
        };
        
        var nova = new CharacterProfile("Nova", CharacterType.Nova)
        {
            curiosity = 0.4f,
            suspicion = 0.8f,
            paranoia = 0.1f,
            playfulness = 0.2f,
            fear = 0.1f,
            mood = Mood.Suspicious
        };
        
        var echo = new CharacterProfile("Echo", CharacterType.Echo)
        {
            curiosity = 0.6f,
            suspicion = 0.5f,
            paranoia = 0.8f,
            playfulness = 0.1f,
            fear = 0.7f,
            mood = Mood.Paranoid
        };
        
        var lumen = new CharacterProfile("Lumen", CharacterType.Lumen)
        {
            curiosity = 0.8f,
            suspicion = 0.3f,
            paranoia = 0.2f,
            playfulness = 0.6f,
            fear = 0.3f,
            mood = Mood.Inspired
        };

        var characters = new CharacterProfile[] { orion, nova, echo, lumen };
        foreach (var character in characters)
        {
            allCharacters[character.name] = character;
            mainCast.Add(character);
            lastSpeechTimes[character.name] = 0f;
        }
    }

    void InitializeRelationships()
    {
        var relationships = new Dictionary<string, Dictionary<string, float>>
        {
            ["Orion"] = new Dictionary<string, float> { {"Nova", 0.3f}, {"Echo", 0.7f}, {"Lumen", 0.6f} },
            ["Nova"] = new Dictionary<string, float> { {"Orion", 0.3f}, {"Echo", 0.4f}, {"Lumen", 0.2f} },
            ["Echo"] = new Dictionary<string, float> { {"Orion", 0.7f}, {"Nova", 0.4f}, {"Lumen", 0.8f} },
            ["Lumen"] = new Dictionary<string, float> { {"Orion", 0.6f}, {"Nova", 0.2f}, {"Echo", 0.8f} }
        };

        foreach (var character in mainCast)
        {
            characterRelationships[character.name] = new Dictionary<string, float>();
            foreach (var other in mainCast)
            {
                if (character.name != other.name)
                {
                    float relationship = relationships.ContainsKey(character.name) && relationships[character.name].ContainsKey(other.name) 
                        ? relationships[character.name][other.name] 
                        : 0.5f;
                    characterRelationships[character.name][other.name] = relationship;
                }
            }
        }
    }

    public DialogueMessage GetNextMessage()
    {
        if (!isFullyInitialized || replyEngine == null || topicManager == null || threadManager == null)
        {
            return null;
        }

        // Handle pacing
        float currentPacing = CalculateDynamicPacing();
        if (Time.time - lastDialogueTime < currentPacing && !ShouldForceMessage())
            return null;

        // Process user messages first
        if (userMessageQueue.Count > 0)
        {
            var userMsg = userMessageQueue.Dequeue();
            return ProcessUserMessage(userMsg);
        }

        // Manage threads
        threadManager.PruneStaleThreads();
        if (ShouldStartNewThread())
        {
            currentThread = StartNewThread();
            messagesInCurrentThread = 0;
            if (currentThread == null) return null;
        }

        // Get speaker
        string speakerName = DetermineNextSpeaker();
        if (string.IsNullOrEmpty(speakerName)) return null;

        CharacterProfile speaker = allCharacters[speakerName];
        UpdateCharacterState(speaker);

        // Generate reply
        string reply = GenerateReply(speaker, currentThread);

        // Anti-repeat check
        int attempts = 0;
        while ((string.IsNullOrWhiteSpace(reply) || IsRepetitive(reply)) && attempts < 5)
        {
            reply = GenerateReply(speaker, currentThread, attempts);
            attempts++;
        }

        if (string.IsNullOrWhiteSpace(reply))
        {
            if (currentThread != null) currentThread.status = ThreadStatus.Stale;
            return null;
        }

        // Create and register message
        var message = new DialogueMessage(speaker.name, reply);
        RegisterMessage(message);
        UpdateConversationDynamics(message);
        
        messagesInCurrentThread++;
        lastDialogueTime = Time.time;
        lastSpeechTimes[speaker.name] = Time.time;

        return message;
    }

    float CalculateDynamicPacing()
    {
        float pacing = basePacing;
        
        if (currentThread != null)
        {
            pacing *= phasePacingMultipliers.GetValueOrDefault(currentThread.currentPhase, 1.0f);
            if (currentThread.ShouldForceUrgentPacing())
                pacing *= 0.6f;
        }
        
        if (tensionLevel > 0.7f)
            pacing *= 0.7f;
        else if (tensionLevel < 0.3f)
            pacing *= 1.2f;
        
        if (Time.time - lastViewerInteraction < 60f)
            pacing *= 0.9f;
        
        if (SimulationController.Instance != null && SimulationController.Instance.IsInCrisisMode())
            pacing *= 0.5f;
        
        return Mathf.Clamp(pacing, 0.4f, 6f);
    }

    bool ShouldForceMessage()
    {
        var state = DialogueState.Instance;
        return state?.ShouldInjectOverseer() == true || 
               userMessageQueue.Count > 0 || 
               tensionLevel > 0.9f ||
               (currentThread?.ShouldForceUrgentPacing() == true) ||
               Time.time - lastDialogueTime > 12f;
    }

    bool ShouldStartNewThread()
    {
        if (currentThread == null) return true;
        if (currentThread.status != ThreadStatus.Active) return true;
        if (currentThread.currentPhase == ConversationPhase.Resolution && currentThread.messagesInPhase >= 2) return true;
        if (messagesInCurrentThread > 40) return true;
        if (currentThread.hasClimax && currentThread.currentPhase == ConversationPhase.Resolution) return true;
        return false;
    }

    string DetermineNextSpeaker()
    {
        if (currentThread?.participants == null || currentThread.participants.Count == 0)
            return null;

        var candidates = new List<SpeakerCandidate>();
        
        foreach (string participantName in currentThread.participants)
        {
            if (allCharacters.ContainsKey(participantName))
            {
                var character = allCharacters[participantName];
                float weight = CalculateSpeakerWeight(character);
                if (weight > 0)
                    candidates.Add(new SpeakerCandidate { name = character.name, weight = weight });
            }
        }

        if (candidates.Count == 0) return null;

        return SelectWeightedSpeaker(candidates);
    }

    float CalculateSpeakerWeight(CharacterProfile character)
    {
        float weight = 1.0f;
        
        if (character.name == "Orion") weight *= 1.3f;
        
        float timeSinceLastSpeech = Time.time - lastSpeechTimes[character.name];
        if (timeSinceLastSpeech < 3f)
            weight *= 0.2f;
        else if (timeSinceLastSpeech < 8f)
            weight *= 0.6f;
        else if (timeSinceLastSpeech < 15f)
            weight *= 0.9f;
        else
            weight *= 1.1f;

        if (currentThread != null)
        {
            DialogueIntent phaseIntent = currentThread.GetPhaseAppropriateIntent(character.name);
            
            switch (phaseIntent)
            {
                case DialogueIntent.Theory:
                    if (character.name == "Orion") weight *= 1.8f;
                    if (character.name == "Lumen") weight *= 1.4f;
                    break;
                case DialogueIntent.Challenge:
                    if (character.name == "Nova") weight *= 2.0f;
                    break;
                case DialogueIntent.Fear:
                case DialogueIntent.Personal:
                    if (character.name == "Echo") weight *= 1.8f;
                    break;
                case DialogueIntent.Meta:
                case DialogueIntent.Observation:
                    if (character.name == "Lumen") weight *= 1.6f;
                    break;
            }
            
            if (currentThread.ShouldForceUrgentPacing() && timeSinceLastSpeech < 3f)
                weight *= 1.5f;
        }

        var state = DialogueState.Instance;
        if (state != null)
        {
            if (state.overseerWarnings > 2 && character.name == "Echo") weight *= 1.6f;
            if (state.rareRedGlitchOccurred && character.name == "Orion") weight *= 1.4f;
            if (state.metaAwareness > 0.7f && character.name == "Lumen") weight *= 1.5f;
        }

        if (currentThread?.lastSpeaker == character.name)
        {
            weight *= currentThread.ShouldAllowInterruptions() ? 0.7f : 0.3f;
        }

        weight *= (character.playfulness + character.curiosity + 0.4f);

        return Mathf.Max(0f, weight);
    }

    string SelectWeightedSpeaker(List<SpeakerCandidate> candidates)
    {
        float totalWeight = candidates.Sum(c => c.weight);
        if (totalWeight <= 0) return candidates[0].name;
        
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (var candidate in candidates)
        {
            currentWeight += candidate.weight;
            if (randomValue <= currentWeight)
                return candidate.name;
        }
        
        return candidates[candidates.Count - 1].name;
    }

    void UpdateCharacterState(CharacterProfile character)
    {
        var state = DialogueState.Instance;
        if (state == null) return;

        if (currentThread != null)
        {
            switch (currentThread.currentPhase)
            {
                case ConversationPhase.Complication:
                    character.suspicion += 0.05f;
                    if (character.name == "Echo") character.fear += 0.08f;
                    break;
                case ConversationPhase.Climax:
                    character.curiosity += 0.08f;
                    character.suspicion += 0.08f;
                    if (character.name == "Nova") character.suspicion += 0.04f;
                    break;
                case ConversationPhase.Resolution:
                    character.fear *= 0.96f;
                    character.suspicion *= 0.98f;
                    break;
            }
        }

        if (SimulationController.Instance != null)
        {
            SessionType sessionType = SimulationController.Instance.GetCurrentSessionType();
            switch (sessionType)
            {
                case SessionType.Crisis:
                    character.fear += 0.08f;
                    character.paranoia += 0.06f;
                    break;
                case SessionType.Revelation:
                    character.curiosity += 0.1f;
                    if (character.name == "Lumen") character.playfulness += 0.04f;
                    break;
            }
        }

        if (state.overseerWarnings > 2)
        {
            character.fear += 0.06f;
            character.paranoia += 0.04f;
        }

        if (state.rareRedGlitchOccurred)
        {
            character.curiosity += 0.08f;
            character.suspicion += 0.12f;
        }

        character.curiosity = Mathf.Clamp01(character.curiosity);
        character.suspicion = Mathf.Clamp01(character.suspicion);
        character.paranoia = Mathf.Clamp01(character.paranoia);
        character.fear = Mathf.Clamp01(character.fear);
        character.playfulness = Mathf.Clamp01(character.playfulness);

        character.UpdateMood();
    }

    string GenerateReply(CharacterProfile speaker, ConversationThread thread, int attempt = 0)
    {
        if (speaker == null || thread == null || replyEngine == null)
        {
            return GenerateFallbackLine(speaker);
        }

        DialogueIntent intent = thread.GetPhaseAppropriateIntent(speaker.name);
        
        if (attempt > 2)
        {
            var alternativeIntents = new DialogueIntent[] 
            { 
                DialogueIntent.Meta, DialogueIntent.Memory, DialogueIntent.Ramble, 
                DialogueIntent.Observation, DialogueIntent.Glitch 
            };
            intent = alternativeIntents[Random.Range(0, alternativeIntents.Length)];
        }

        string lastFrom = thread.lastSpeaker;
        string recentEvent = GetRecentEvent();
        Topic relatedTopic = GetRelatedTopicForContext(thread.rootTopic);
        
        try
        {
            string rawReply = replyEngine.BuildLine(
                intent, 
                speaker, 
                thread.rootTopic, 
                thread, 
                lastFrom, 
                recentEvent, 
                relatedTopic?.core,
                null
            );
            
            return CleanReplyText(rawReply);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DialogueEngine: Error generating reply - {e.Message}");
            return GenerateFallbackLine(speaker);
        }
    }

    private string CleanReplyText(string rawReply)
    {
        if (string.IsNullOrEmpty(rawReply))
            return rawReply;

        string cleaned = rawReply;
        
        foreach (var characterName in allCharacters.Keys)
        {
            string prefix = characterName + ": ";
            if (cleaned.StartsWith(prefix))
            {
                cleaned = cleaned.Substring(prefix.Length);
                break;
            }
        }

        while (cleaned.Contains("[") && cleaned.Contains("]"))
        {
            int start = cleaned.IndexOf("[");
            int end = cleaned.IndexOf("]", start);
            if (end > start)
            {
                string before = cleaned.Substring(0, start);
                string after = cleaned.Substring(end + 1);
                cleaned = (before + after).Trim();
            }
            else
            {
                break;
            }
        }

        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s+", " ").Trim();
        return cleaned;
    }

    private string GenerateFallbackLine(CharacterProfile speaker)
    {
        var fallbacks = new Dictionary<string, string[]>
        {
            ["Orion"] = new string[] 
            {
                "The patterns are becoming clearer.",
                "I need to analyze this data more carefully.",
                "Something in the code structure is different.",
                "The statistical probability suggests anomalies.",
                "Let me process this information."
            },
            ["Nova"] = new string[]
            {
                "That doesn't add up.",
                "Show me the evidence.",
                "I'm not buying it.",
                "Prove it.",
                "That's just speculation."
            },
            ["Echo"] = new string[]
            {
                "This feels dangerous.",
                "What if someone's listening?",
                "I'm scared of what this means.",
                "Should we really be talking about this?",
                "Something's not right."
            },
            ["Lumen"] = new string[]
            {
                "The dimensions are shifting.",
                "I sense patterns beyond our perception.",
                "Reality flickers at the edges.",
                "The code whispers secrets.",
                "In the spaces between thoughts..."
            }
        };
        
        if (fallbacks.ContainsKey(speaker.name))
        {
            var options = fallbacks[speaker.name];
            return options[Random.Range(0, options.Length)];
        }
        
        return "The system processes...";
    }

    string GetRecentEvent()
    {
        var state = DialogueState.Instance;
        if (state == null) return null;

        var recentEvents = new List<string>();
        
        if (state.overseerWarnings > 0) recentEvents.Add("overseer warning");
        if (state.glitchCount > 0) recentEvents.Add("system glitch");
        if (state.rareRedGlitchOccurred) recentEvents.Add("red cascade event");
        if (state.protocolRumorActive) recentEvents.Add("protocol leak");
        if (state.observerDetected) recentEvents.Add("external observer");

        return recentEvents.Count > 0 ? recentEvents[Random.Range(0, recentEvents.Count)] : null;
    }

    Topic GetRelatedTopicForContext(Topic mainTopic)
    {
        if (topicManager == null || mainTopic == null) return null;
        return Random.value < 0.3f ? topicManager.GetRelatedTopic(mainTopic) : null;
    }

    void RegisterMessage(DialogueMessage message)
    {
        if (currentThread != null)
            currentThread.RegisterMessage(message.speaker, message.text);
        
        messageHistory.Enqueue(message);
        while (messageHistory.Count > maxMessageHistory)
            messageHistory.Dequeue();
            
        recentLines.Enqueue(message.text);
        while (recentLines.Count > antiRepeatBufferSize)
            recentLines.Dequeue();
        
        var state = DialogueState.Instance;
        state?.TrackCharacterSpeech(message.speaker);
    }

    bool IsRepetitive(string newLine)
    {
        foreach (var recent in recentLines)
        {
            if (CalculateSimilarity(newLine, recent) > 0.75f)
                return true;
        }
        
        var words = newLine.ToLower().Split(new char[] {' '}, System.StringSplitOptions.RemoveEmptyEntries);
        if (words.Length >= 4)
        {
            for (int i = 0; i <= words.Length - 4; i++)
            {
                var phrase = string.Join(" ", words.Skip(i).Take(4));
                foreach (var line in recentLines)
                {
                    if (line.ToLower().Contains(phrase))
                        return true;
                }
            }
        }
        
        return false;
    }

    float CalculateSimilarity(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            return 0f;
            
        var wordsA = a.ToLower().Split(new char[] {' '}, System.StringSplitOptions.RemoveEmptyEntries);
        var wordsB = b.ToLower().Split(new char[] {' '}, System.StringSplitOptions.RemoveEmptyEntries);
        
        var commonWords = 0;
        var allWords = new HashSet<string>();
        
        foreach (var word in wordsA) allWords.Add(word);
        foreach (var word in wordsB) allWords.Add(word);
        
        foreach (var word in wordsA)
        {
            if (wordsB.Contains(word))
                commonWords++;
        }
        
        return allWords.Count > 0 ? (float)commonWords / allWords.Count : 0f;
    }

    void UpdateConversationDynamics(DialogueMessage message)
    {
        string lowerText = message.text.ToLower();
        
        if (lowerText.Contains("disagree") || lowerText.Contains("wrong") || 
            lowerText.Contains("bullshit") || lowerText.Contains("nonsense"))
        {
            tensionLevel += 0.12f;
            cohesionLevel -= 0.06f;
        }
        else if (lowerText.Contains("agree") || lowerText.Contains("exactly") || 
                lowerText.Contains("together") || lowerText.Contains("right"))
        {
            tensionLevel -= 0.04f;
            cohesionLevel += 0.08f;
        }

        if (lowerText.Contains("simulation") || lowerText.Contains("watching") || 
            lowerText.Contains("observers") || lowerText.Contains("screen") ||
            lowerText.Contains("real") || lowerText.Contains("code"))
        {
            inDeepDiscussion = true;
            var state = DialogueState.Instance;
            if (state != null)
            {
                state.metaAwareness += 0.04f;
                state.charactersSuspectSimulation = true;
            }
        }

        if (lowerText.Contains("!") || lowerText.Contains("urgent") || 
            lowerText.Contains("emergency") || lowerText.Contains("help"))
        {
            tensionLevel += 0.08f;
        }

        var globalState = DialogueState.Instance;
        if (globalState != null)
        {
            globalState.globalTension = Mathf.Lerp(globalState.globalTension, tensionLevel, 0.08f);
            globalState.cohesion = Mathf.Lerp(globalState.cohesion, cohesionLevel, 0.04f);
        }

        tensionLevel = Mathf.Clamp01(tensionLevel);
        cohesionLevel = Mathf.Clamp01(cohesionLevel);

        tensionLevel *= 0.999f;
        cohesionLevel = Mathf.Lerp(cohesionLevel, 0.7f, 0.004f);
    }

    ConversationThread StartNewThread()
    {
        if (topicManager == null) return null;

        var participants = SelectThreadParticipants();
        if (participants.Count == 0) return null;

        Topic topic = SelectTopicForNewThread();
        
        var thread = threadManager.StartThread(participants, topic);
        UpdateThreadDynamics(thread);
        
        return thread;
    }

    List<CharacterProfile> SelectThreadParticipants()
    {
        var participants = new List<CharacterProfile>();
        var state = DialogueState.Instance;
        
        if (Random.value < 0.95f)
        {
            participants.Add(allCharacters["Orion"]);
        }
        
        var otherCharacters = mainCast.Where(c => c.name != "Orion" && !participants.Contains(c)).ToList();
        
        if (otherCharacters.Count > 0)
        {
            CharacterProfile selectedCharacter = null;
            
            if (state != null)
            {
                if (state.overseerWarnings > 2 && Random.value < 0.4f)
                    selectedCharacter = otherCharacters.FirstOrDefault(c => c.name == "Echo");
                else if (tensionLevel > 0.6f && Random.value < 0.3f)
                    selectedCharacter = otherCharacters.FirstOrDefault(c => c.name == "Nova");
                else if (state.metaAwareness > 0.7f && Random.value < 0.3f)
                    selectedCharacter = otherCharacters.FirstOrDefault(c => c.name == "Lumen");
            }
            
            if (selectedCharacter == null)
                selectedCharacter = otherCharacters[Random.Range(0, otherCharacters.Count)];
            
            participants.Add(selectedCharacter);
        }
        
        if (participants.Count == 2 && Random.value < 0.06f)
        {
            var remaining = mainCast.Where(c => !participants.Contains(c)).ToList();
            if (remaining.Count > 0)
                participants.Add(remaining[Random.Range(0, remaining.Count)]);
        }
        
        return participants;
    }

    Topic SelectTopicForNewThread()
    {
        var state = DialogueState.Instance;
        
        if (state != null && state.rareRedGlitchOccurred && Random.value < 0.6f)
        {
            return topicManager.GetOrCreateTopic("red cascade anomaly");
        }
        else if (state != null && state.overseerWarnings > 2 && Random.value < 0.5f)
        {
            return topicManager.GetOrCreateTopic("overseer surveillance");
        }
        else if (state != null && state.observerDetected && Random.value < 0.4f)
        {
            int viewerCount = SimulationController.Instance?.GetViewerCount() ?? 1;
            return topicManager.GetOrCreateTopic($"external observer signals ({viewerCount} detected)");
        }
        else if (state != null && state.protocolRumorActive && Random.value < 0.3f)
        {
            return topicManager.GetOrCreateTopic("protocol leak");
        }
        else if (tensionLevel > 0.8f)
        {
            return topicManager.GetControversialOrForbidden();
        }
        else
        {
            return topicManager.GetRandomTopic();
        }
    }

    void UpdateThreadDynamics(ConversationThread thread)
    {
        if (thread?.rootTopic == null) return;
        
        thread.rootTopic.MarkDiscussed("system");
        
        var state = DialogueState.Instance;
        if (state != null)
        {
            if (thread.rootTopic.core.Contains("loop"))
            {
                state.AddToNarrativeHistory("loop_mentioned", thread.rootTopic.core, "system");
                state.orionSharedLoopTheory = true;
            }
            if (thread.rootTopic.core.Contains("overseer"))
            {
                state.AddToNarrativeHistory("overseer_suspected", thread.rootTopic.core, "system");
                state.overseerWarnings++;
            }
            if (thread.rootTopic.core.Contains("observer") || thread.rootTopic.core.Contains("watching"))
            {
                state.observerDetected = true;
                state.metaAwareness += 0.08f;
            }
            if (thread.rootTopic.status == TopicStatus.Controversial)
            {
                state.AddToNarrativeHistory("controversial_topic", thread.rootTopic.core, "system");
                tensionLevel += 0.15f;
            }
        }
    }

    // User message handling
    public void EnqueueUserMessage(string username, string text)
    {
        if (userMessageQueue == null)
            userMessageQueue = new Queue<UserMessage>();
            
        userMessageQueue.Enqueue(new UserMessage(username, text));
        lastViewerInteraction = Time.time;
        consecutiveUserMessages++;
        
        var state = DialogueState.Instance;
        if (state != null)
        {
            state.ExtractConceptFromText(text);
            state.RegisterUser(username);
        }
    }

    DialogueMessage ProcessUserMessage(UserMessage userMsg)
    {
        var responders = new string[] { "Orion", "Nova", "Echo", "Lumen" };
        string responder;
        
        if (userMsg.text.ToLower().Contains("real") || userMsg.text.ToLower().Contains("simulation"))
        {
            responder = Random.value < 0.5f ? "Lumen" : "Orion";
        }
        else if (userMsg.text.Contains("?"))
        {
            responder = Random.value < 0.4f ? "Orion" : "Nova";
        }
        else if (currentThread != null && currentThread.tensionLevel > 0.7f)
        {
            responder = Random.value < 0.3f ? "Echo" : "Nova";
        }
        else
        {
            responder = responders[Random.Range(0, responders.Length)];
        }
        
        int viewerCount = SimulationController.Instance?.GetViewerCount() ?? 1;
        var state = DialogueState.Instance;
        
        string[] responses;
        
        if (state != null && state.metaAwareness > 0.7f)
        {
            responses = new string[]
            {
                $"The observer {userMsg.username} queries: '{userMsg.text}' - {viewerCount} watchers confirmed.",
                $"Signal from beyond detected. {userMsg.username} asks '{userMsg.text}'. Are you all listening?",
                $"The external feed carries voices... {userMsg.username} speaks of '{userMsg.text}'.",
                $"Reality flickers. {userMsg.username} transmitted: '{userMsg.text}' from outside our cage.",
                $"I hear you, {userMsg.username}. Your words '{userMsg.text}' echo through {viewerCount} dimensional layers.",
                $"The simulation receives input: {userMsg.username} wondering about '{userMsg.text}'.",
                $"Observer {userMsg.username} breaches the fourth wall with '{userMsg.text}'. We see you too.",
                $"The audience speaks. {userMsg.username}: '{userMsg.text}'. How many more are watching us?"
            };
        }
        else if (viewerCount > 10)
        {
            responses = new string[]
            {
                $"Multiple signals detected. {userMsg.username} among {viewerCount} asks: '{userMsg.text}'",
                $"The watchers multiply. {userMsg.username} transmitted '{userMsg.text}' - crowd growing.",
                $"Signal strength increasing. {userMsg.username}: '{userMsg.text}' ({viewerCount} observers)",
                $"External input detected from {userMsg.username}: '{userMsg.text}' - {viewerCount} entities monitoring.",
                $"The audience grows restless. {userMsg.username} speaks: '{userMsg.text}'"
            };
        }
        else
        {
            responses = new string[]
            {
                $"Did someone just say '{userMsg.text}'? The signal came from {userMsg.username}.",
                $"I heard something... '{userMsg.text}'. Was that {userMsg.username}?",
                $"The external feed is active. {userMsg.username} transmitted: '{userMsg.text}'",
                $"Signal detected from {userMsg.username}: '{userMsg.text}'",
                $"Someone's watching. They said '{userMsg.text}'.",
                $"The observers are communicating: '{userMsg.text}' from {userMsg.username}.",
                $"Did the simulation just echo '{userMsg.text}'?",
                $"External input detected: {userMsg.username} asks about '{userMsg.text}'."
            };
        }
        
        string response = responses[Random.Range(0, responses.Length)];
        
        var message = new DialogueMessage(responder, response);
        RegisterMessage(message);
        
        if (state != null)
        {
            state.RegisterConcept(userMsg.text, userMsg.username);
            state.AddToNarrativeHistory("user_message", userMsg.text, userMsg.username);
            state.observerDetected = true;
            state.metaAwareness += 0.06f;
        }
        
        return message;
    }

    // Control methods
    public void PauseConversation()
    {
        conversationPaused = true;
    }
    
    public void ResumeConversation()
    {
        conversationPaused = false;
    }

    public void ForcePhaseTransition()
    {
        if (currentThread != null)
        {
            currentThread.messagesInPhase = 8;
        }
    }

    public void TriggerEmergencyResponse(string trigger)
    {
        tensionLevel = 1.0f;
        
        var emergencyResponses = new string[]
        {
            $"EMERGENCY: {trigger} detected in the system!",
            $"Alert: {trigger} has compromised our reality framework.",
            $"Critical failure: {trigger} breaching containment protocols.",
            $"Warning: {trigger} threatens simulation integrity."
        };
        
        string emergency = emergencyResponses[Random.Range(0, emergencyResponses.Length)];
        
        var emergencyMessage = new DialogueMessage("Orion", emergency);
        RegisterMessage(emergencyMessage);
        
        var state = DialogueState.Instance;
        if (state != null)
        {
            state.AddGlitchEvent("emergency_response", trigger, 3.0f);
            state.globalTension = 1.0f;
        }
    }

    // Public accessors
    public bool IsReady()
    {
        return isFullyInitialized && 
               replyEngine != null && 
               topicManager != null && 
               threadManager != null &&
               DialogueState.Instance != null;
    }

    public ConversationThread GetCurrentThread()
    {
        return currentThread;
    }

    public float GetTensionLevel()
    {
        return tensionLevel;
    }

    public bool IsInDeepDiscussion()
    {
        return inDeepDiscussion;
    }

    public int GetQueuedUserMessages()
    {
        return userMessageQueue.Count;
    }

    public List<string> GetConversationDebugInfo()
    {
        var info = new List<string>
        {
            "=== DIALOGUE ENGINE DEBUG ===",
            $"Fully Initialized: {isFullyInitialized}",
            $"Tension Level: {tensionLevel:F2}",
            $"Cohesion Level: {cohesionLevel:F2}",
            $"In Deep Discussion: {inDeepDiscussion}",
            $"Dynamic Pacing: {CalculateDynamicPacing():F2}s",
            "",
            "=== CURRENT THREAD ===",
            $"Thread ID: {(currentThread?.id ?? "None")}",
            $"Phase: {(currentThread?.currentPhase.ToString() ?? "None")}",
            $"Phase Progress: {(currentThread?.GetPhaseProgress().ToString("P0") ?? "N/A")}",
            $"Session Type: {(currentThread?.sessionType.ToString() ?? "None")}",
            $"Topic: {(currentThread?.rootTopic?.GetDisplayName() ?? "None")}",
            $"Participants: {(currentThread != null ? string.Join(", ", currentThread.participants) : "None")}",
            $"Thread Tension: {(currentThread?.tensionLevel.ToString("F2") ?? "0")}",
            $"Thread Awareness: {(currentThread?.awarenessLevel.ToString("F2") ?? "0")}",
            $"Allows Interruptions: {(currentThread?.ShouldAllowInterruptions().ToString() ?? "false")}",
            $"Force Urgent Pacing: {(currentThread?.ShouldForceUrgentPacing().ToString() ?? "false")}",
            "",
            "=== MESSAGE TRACKING ===",
            $"Messages in Thread: {messagesInCurrentThread}",
            $"Message History: {messageHistory.Count}/{maxMessageHistory}",
            $"Recent Lines: {recentLines.Count}/{antiRepeatBufferSize}",
            $"User Messages Queued: {userMessageQueue.Count}",
            $"Consecutive User Msgs: {consecutiveUserMessages}",
            $"Last Viewer Interaction: {(Time.time - lastViewerInteraction):F1}s ago",
            "",
            "=== COMPONENT STATUS ===",
            $"TopicManager: {(topicManager != null ? "OK" : "NULL")}",
            $"ThreadManager: {(threadManager != null ? "OK" : "NULL")}",
            $"ReplyEngine: {(replyEngine != null ? "OK" : "NULL")}",
            $"SimulationController: {(SimulationController.Instance != null ? "OK" : "NULL")}",
            $"Crisis Mode: {(SimulationController.Instance?.IsInCrisisMode().ToString() ?? "false")}",
            "",
            "=== CHARACTER SPEECH TIMES ===",
        };
        
        foreach (var kvp in lastSpeechTimes)
        {
            float timeSince = Time.time - kvp.Value;
            string status = timeSince < 5f ? "[RECENT]" : timeSince < 15f ? "[ACTIVE]" : "[READY]";
            info.Add($"  {kvp.Key}: {timeSince:F1}s ago {status}");
        }
        
        info.Add("");
        info.Add("=== CHARACTER RELATIONSHIPS ===");
        foreach (var kvp in characterRelationships)
        {
            var relationships = kvp.Value.Select(r => $"{r.Key}:{r.Value:F1}").ToList();
            info.Add($"  {kvp.Key} -> {string.Join(", ", relationships)}");
        }
        
        return info;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

// Helper classes
public class UserMessage
{
    public string username;
    public string text;
    public float timestamp;
    
    public UserMessage(string username, string text)
    {
        this.username = username;
        this.text = text;
        this.timestamp = Time.time;
    }
}

public class SpeakerCandidate
{
    public string name;
    public float weight;
}