using System.Collections.Generic;
using UnityEngine;

public enum ThreadStatus
{
    Active,
    Stale,
    Closed,
    Escalating,
    Interrupted
}

public enum ConversationPhase
{
    Introduction,    // Characters establish topic
    Development,     // Explore implications  
    Complication,    // Introduce conflict/doubt
    Climax,         // Major revelation/argument
    Resolution      // Aftermath/new questions
}

public enum SessionType 
{
    Discovery,       // Characters find something new
    Investigation,   // Dig deeper into mysteries  
    Crisis,         // System threatens them
    Revelation,     // Major breakthrough
    Reset           // Everything falls apart
}

public class ConversationThread
{
    public string id;
    public List<string> participants = new List<string>();
    public Dictionary<string, CharacterProfile> participantProfiles = new Dictionary<string, CharacterProfile>();
    public Topic rootTopic;
    public List<Topic> branchTopics = new List<Topic>();
    public string lastSpeaker;
    public string lastMessage;
    public int turnCount = 0;
    public ThreadStatus status = ThreadStatus.Active;
    public float createdTime;
    public float lastActivity;
    public List<string> history = new List<string>();

    // Enhanced narrative structure
    public ConversationPhase currentPhase = ConversationPhase.Introduction;
    public SessionType sessionType = SessionType.Discovery;
    public float tensionLevel = 0f;
    public float awarenessLevel = 0f;
    public int messagesInPhase = 0;
    public bool hasClimax = false;
    public bool isViewerAware = false;
    
    // Phase transition thresholds
    private int minMessagesPerPhase = 2;
    private int maxMessagesPerPhase = 8;

    // Intent flow tracking to prevent loops
    private Queue<DialogueIntent> recentIntents = new Queue<DialogueIntent>();
    private Dictionary<string, int> speakerMessageCount = new Dictionary<string, int>();

    public ConversationThread(string id, List<CharacterProfile> profiles, Topic topic)
    {
        this.id = id;
        foreach (var p in profiles)
        {
            participants.Add(p.name);
            participantProfiles[p.name] = p;
            speakerMessageCount[p.name] = 0;
        }
        rootTopic = topic;
        branchTopics.Add(topic);
        lastSpeaker = null;
        lastMessage = null;
        createdTime = Time.time;
        lastActivity = Time.time;
        status = ThreadStatus.Active;
        
        DetermineSessionType();
    }

    public string GetNextSpeaker()
    {
        // Prevent same speaker domination
        var availableSpeakers = new List<string>();
        
        foreach (string participant in participants)
        {
            if (participant != lastSpeaker)
                availableSpeakers.Add(participant);
        }
        
        if (availableSpeakers.Count == 0)
            availableSpeakers = participants;
        
        // Balance speaker participation
        string nextSpeaker = null;
        int minMessages = int.MaxValue;
        
        foreach (string speaker in availableSpeakers)
        {
            int messageCount = speakerMessageCount.GetValueOrDefault(speaker, 0);
            if (messageCount < minMessages)
            {
                minMessages = messageCount;
                nextSpeaker = speaker;
            }
        }
        
        return nextSpeaker ?? availableSpeakers[0];
    }

    public void RegisterMessage(string speaker, string message)
    {
        lastSpeaker = speaker;
        lastMessage = message;
        history.Add($"{speaker}: {message}");
        turnCount++;
        messagesInPhase++;
        lastActivity = Time.time;
        
        speakerMessageCount[speaker] = speakerMessageCount.GetValueOrDefault(speaker, 0) + 1;
        
        UpdateDynamicsFromMessage(message);
        CheckPhaseTransition();
    }

    public void AddBranchTopic(Topic topic)
    {
        if (!branchTopics.Contains(topic))
            branchTopics.Add(topic);
    }

    private void DetermineSessionType()
    {
        var state = DialogueState.Instance;
        if (state == null)
        {
            sessionType = SessionType.Discovery;
            return;
        }

        if (state.overseerWarnings > 2 || state.rareRedGlitchOccurred)
            sessionType = SessionType.Crisis;
        else if (state.metaAwareness > 0.7f)
            sessionType = SessionType.Revelation;
        else if (state.glitchCount > 3)
            sessionType = SessionType.Investigation;
        else if (state.systemIntegrityCompromised)
            sessionType = SessionType.Reset;
        else
            sessionType = SessionType.Discovery;
    }

    private void UpdateDynamicsFromMessage(string message)
    {
        string lowerMessage = message.ToLower();
        
        // Increase tension for conflict/disagreement
        if (lowerMessage.Contains("wrong") || lowerMessage.Contains("disagree") || 
            lowerMessage.Contains("bullshit") || lowerMessage.Contains("nonsense"))
        {
            tensionLevel += 0.15f;
        }
        
        // Increase awareness for meta content
        if (lowerMessage.Contains("simulation") || lowerMessage.Contains("watching") || 
            lowerMessage.Contains("observers") || lowerMessage.Contains("screen") ||
            lowerMessage.Contains("real") || lowerMessage.Contains("code"))
        {
            awarenessLevel += 0.1f;
            if (awarenessLevel > 0.6f)
                isViewerAware = true;
        }
        
        // Dramatic content escalation
        if (lowerMessage.Contains("!") || lowerMessage.Contains("urgent") || 
            lowerMessage.Contains("emergency"))
        {
            tensionLevel += 0.1f;
        }
        
        // Clamp values
        tensionLevel = Mathf.Clamp01(tensionLevel);
        awarenessLevel = Mathf.Clamp01(awarenessLevel);
    }

    private void CheckPhaseTransition()
    {
        if (messagesInPhase < minMessagesPerPhase)
            return;
            
        bool shouldTransition = false;
        
        if (messagesInPhase >= maxMessagesPerPhase)
            shouldTransition = true;
            
        if (messagesInPhase >= minMessagesPerPhase + 1)
        {
            switch (currentPhase)
            {
                case ConversationPhase.Introduction:
                    if (tensionLevel > 0.3f || awarenessLevel > 0.2f)
                        shouldTransition = true;
                    break;
                    
                case ConversationPhase.Development:
                    if (tensionLevel > 0.6f || awarenessLevel > 0.5f)
                        shouldTransition = true;
                    break;
                    
                case ConversationPhase.Complication:
                    if (tensionLevel > 0.8f || awarenessLevel > 0.7f || sessionType == SessionType.Crisis)
                        shouldTransition = true;
                    break;
                    
                case ConversationPhase.Climax:
                    if (messagesInPhase >= 3)
                        shouldTransition = true;
                    break;
                    
                case ConversationPhase.Resolution:
                    if (messagesInPhase >= 2)
                    {
                        status = ThreadStatus.Stale;
                        return;
                    }
                    break;
            }
        }
        
        if (shouldTransition)
            TransitionToNextPhase();
    }

    private void TransitionToNextPhase()
    {
        messagesInPhase = 0;
        
        switch (currentPhase)
        {
            case ConversationPhase.Introduction:
                currentPhase = ConversationPhase.Development;
                break;
                
            case ConversationPhase.Development:
                currentPhase = ConversationPhase.Complication;
                break;
                
            case ConversationPhase.Complication:
                currentPhase = ConversationPhase.Climax;
                hasClimax = true;
                break;
                
            case ConversationPhase.Climax:
                currentPhase = ConversationPhase.Resolution;
                break;
                
            case ConversationPhase.Resolution:
                status = ThreadStatus.Stale;
                break;
        }
        
        AdjustForNewPhase();
    }

    private void AdjustForNewPhase()
    {
        var state = DialogueState.Instance;
        if (state == null) return;
        
        switch (currentPhase)
        {
            case ConversationPhase.Development:
                state.globalTension += 0.05f;
                break;
                
            case ConversationPhase.Complication:
                state.globalTension += 0.1f;
                state.paranoia += 0.05f;
                break;
                
            case ConversationPhase.Climax:
                state.globalTension += 0.15f;
                state.metaAwareness += 0.1f;
                if (sessionType == SessionType.Revelation)
                    state.charactersSuspectSimulation = true;
                break;
                
            case ConversationPhase.Resolution:
                state.globalTension *= 0.8f;
                break;
        }
    }

    public float GetPhaseProgress()
    {
        return Mathf.Clamp01((float)messagesInPhase / maxMessagesPerPhase);
    }

    public string GetPhaseDescription()
    {
        switch (currentPhase)
        {
            case ConversationPhase.Introduction: return "Establishing topic and context";
            case ConversationPhase.Development: return "Exploring implications and details";
            case ConversationPhase.Complication: return "Introducing conflict and doubt";
            case ConversationPhase.Climax: return "Peak tension and revelation";
            case ConversationPhase.Resolution: return "Aftermath and new questions";
            default: return "Unknown phase";
        }
    }

    public bool ShouldForceUrgentPacing()
    {
        return sessionType == SessionType.Crisis || 
               currentPhase == ConversationPhase.Climax ||
               tensionLevel > 0.8f;
    }

    public bool ShouldAllowInterruptions()
    {
        return currentPhase == ConversationPhase.Complication || 
               currentPhase == ConversationPhase.Climax ||
               sessionType == SessionType.Crisis;
    }

    public DialogueIntent GetPhaseAppropriateIntent(string characterName)
    {
        // Prevent question loops by heavily favoring statements and responses
        var questionIntents = new DialogueIntent[] { }; // No question intents
        
        switch (currentPhase)
        {
            case ConversationPhase.Introduction:
                return Random.value < 0.8f ? DialogueIntent.Reply : DialogueIntent.Statement;
                
            case ConversationPhase.Development:
                if (characterName == "Orion")
                    return Random.value < 0.6f ? DialogueIntent.Theory : DialogueIntent.Statement;
                return Random.value < 0.5f ? DialogueIntent.Reply : DialogueIntent.Observation;
                
            case ConversationPhase.Complication:
                if (characterName == "Nova")
                    return Random.value < 0.7f ? DialogueIntent.Challenge : DialogueIntent.Statement;
                if (characterName == "Echo")
                    return Random.value < 0.6f ? DialogueIntent.Fear : DialogueIntent.Personal;
                return Random.value < 0.4f ? DialogueIntent.Doubt : DialogueIntent.Challenge;
                
            case ConversationPhase.Climax:
                if (isViewerAware && Random.value < 0.4f)
                    return DialogueIntent.Meta;
                return Random.value < 0.5f ? DialogueIntent.Statement : DialogueIntent.Challenge;
                
            case ConversationPhase.Resolution:
                return Random.value < 0.6f ? DialogueIntent.Support : DialogueIntent.Statement;
                
            default:
                return DialogueIntent.Reply;
        }
    }

    // Enhanced speaker balancing
    public bool IsSpeakerDominating(string speakerName)
    {
        if (speakerMessageCount.Count == 0) return false;
        
        int speakerMessages = speakerMessageCount.GetValueOrDefault(speakerName, 0);
        int totalMessages = 0;
        foreach (var count in speakerMessageCount.Values)
            totalMessages += count;
        
        if (totalMessages < 4) return false; // Too early to judge
        
        float speakerPercentage = (float)speakerMessages / totalMessages;
        return speakerPercentage > 0.6f; // More than 60% is dominating
    }

    public string GetLeastActiveSpeaker()
    {
        string leastActive = null;
        int minMessages = int.MaxValue;
        
        foreach (string participant in participants)
        {
            int messageCount = speakerMessageCount.GetValueOrDefault(participant, 0);
            if (messageCount < minMessages)
            {
                minMessages = messageCount;
                leastActive = participant;
            }
        }
        
        return leastActive;
    }

    public void RegisterIntent(DialogueIntent intent)
    {
        recentIntents.Enqueue(intent);
        while (recentIntents.Count > 6)
        {
            recentIntents.Dequeue();
        }
    }

    public bool HasTooManyRecentIntents(DialogueIntent intent)
    {
        int count = 0;
        foreach (var recentIntent in recentIntents)
        {
            if (recentIntent == intent)
                count++;
        }
        return count >= 3; // Max 3 of same intent in recent history
    }

    public Dictionary<string, int> GetSpeakerStats()
    {
        return new Dictionary<string, int>(speakerMessageCount);
    }

    public string GetConversationSummary()
    {
        return $"Thread {id}: {currentPhase} phase, {turnCount} messages, " +
               $"Tension: {tensionLevel:F1}, Awareness: {awarenessLevel:F1}, " +
               $"Type: {sessionType}, Topic: {rootTopic?.GetDisplayName() ?? "Unknown"}";
    }
}