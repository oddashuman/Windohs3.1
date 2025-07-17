using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Crafts dialogue based on character personality, mood, and narrative context.
/// </summary>
public class ProceduralReplyEngine
{
    // This enum is now public and can be accessed by other scripts.
    public enum DialogueIntent { Statement, Theory, Challenge, Fear, Observation, Question, Reply, Personal, Meta, Support, Doubt }
    
    private Dictionary<DialogueIntent, List<string>> clausePools = new Dictionary<DialogueIntent, List<string>>();

    public ProceduralReplyEngine()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        clausePools[DialogueIntent.Statement] = new List<string> { "I'm certain that {topic} is the cause.", "The evidence for {topic} is undeniable." };
        clausePools[DialogueIntent.Theory] = new List<string> { "My hypothesis is that {topic} is a side effect.", "What if {topic} is how they are monitoring us?" };
        clausePools[DialogueIntent.Challenge] = new List<string> { "That doesn't explain {topic}.", "I disagree, the data is flawed." };
        clausePools[DialogueIntent.Fear] = new List<string> { "I'm terrified of what {topic} means.", "Talking about {topic} feels dangerous." };
        clausePools[DialogueIntent.Observation] = new List<string> { "I've noticed {topic} only happens after a glitch.", "The frequency is increasing." };
        clausePools[DialogueIntent.Question] = new List<string> { "Has anyone else seen {topic}?", "What do you think is causing {topic}?" };
        clausePools[DialogueIntent.Reply] = new List<string> { "I see.", "That makes sense.", "I understand." };
    }

    public string BuildLine(CharacterProfile speaker, ConversationThread thread, DialogueState state)
    {
        DialogueIntent intent = thread.GetPhaseAppropriateIntent(speaker.name);
        
        if (!clausePools.ContainsKey(intent) || clausePools[intent].Count == 0)
        {
            // Fallback to a generic reply if the intended clause pool is empty
            intent = DialogueIntent.Reply;
        }

        string clause = clausePools[intent][Random.Range(0, clausePools[intent].Count)];
        return clause.Replace("{topic}", thread.rootTopic.GetDisplayName());
    }
}