using System.Collections.Generic;
using UnityEngine;

// Enums are kept in this file as they are tightly coupled with the ConversationThread class.
public enum ThreadStatus { Active, Stale, Closed, Escalating, Interrupted }
public enum ConversationPhase { Introduction, Development, Complication, Climax, Resolution }
public enum SessionType { Discovery, Investigation, Crisis, Revelation, Reset }

public class ConversationThread
{
    public string id;
    public List<CharacterProfile> participants;
    public Topic rootTopic;
    public string lastSpeaker;
    public int turnCount = 0;
    public ThreadStatus status = ThreadStatus.Active;
    public float lastActivity;
    public ConversationPhase currentPhase = ConversationPhase.Introduction;
    
    private int messagesInPhase = 0;
    
    public ConversationThread(string id, List<CharacterProfile> profiles, Topic topic)
    {
        this.id = id;
        this.participants = profiles;
        this.rootTopic = topic;
        this.lastActivity = Time.time;
    }

    public void RegisterMessage(DialogueMessage message)
    {
        lastSpeaker = message.speaker;
        turnCount++;
        messagesInPhase++;
        lastActivity = Time.time;
        UpdatePhase();
    }
    
    private void UpdatePhase()
    {
        // Simplified phase transition logic for stability
        if (messagesInPhase > 5)
        {
            messagesInPhase = 0;
            if (currentPhase < ConversationPhase.Resolution)
            {
                currentPhase++;
                Debug.Log($"Thread {id} advanced to phase: {currentPhase}");
            }
            else
            {
                status = ThreadStatus.Stale;
            }
        }
    }

    public ProceduralReplyEngine.DialogueIntent GetPhaseAppropriateIntent(string characterName)
    {
        // This logic determines the "flavor" of the conversation
        switch (currentPhase)
        {
            case ConversationPhase.Introduction: return ProceduralReplyEngine.DialogueIntent.Question;
            case ConversationPhase.Development: return ProceduralReplyEngine.DialogueIntent.Theory;
            case ConversationPhase.Complication: return ProceduralReplyEngine.DialogueIntent.Challenge;
            case ConversationPhase.Climax: return ProceduralReplyEngine.DialogueIntent.Fear;
            case ConversationPhase.Resolution: return ProceduralReplyEngine.DialogueIntent.Statement;
            default: return ProceduralReplyEngine.DialogueIntent.Reply;
        }
    }
}