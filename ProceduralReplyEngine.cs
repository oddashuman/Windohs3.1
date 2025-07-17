using System.Collections.Generic;
using UnityEngine;

public class ProceduralReplyEngine
{
    public enum DialogueIntent { Statement, Theory, Challenge, Fear, Observation, Question, Reply, Personal, Meta, Support, Doubt }

    private Dictionary<DialogueIntent, List<string>> clausePools = new Dictionary<DialogueIntent, List<string>>();

    public ProceduralReplyEngine()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        clausePools[DialogueIntent.Statement] = new List<string> { "I'm certain that {topic} is the cause.", "The evidence for {topic} is undeniable.", "We need to accept that {topic} is our reality." };
        clausePools[DialogueIntent.Theory] = new List<string> { "My hypothesis is that {topic} is a side effect.", "What if {topic} is how they are monitoring us?", "Could {topic} be a message?" };
        clausePools[DialogueIntent.Challenge] = new List<string> { "That doesn't explain {topic}.", "I disagree, the data on {topic} is flawed.", "But what if you're wrong about {topic}?" };
        clausePools[DialogueIntent.Fear] = new List<string> { "I'm terrified of what {topic} means.", "Talking about {topic} feels dangerous.", "The implications of {topic} are staggering." };
        clausePools[DialogueIntent.Observation] = new List<string> { "I've noticed {topic} only happens after a glitch.", "The frequency of {topic} is increasing.", "The logs show a correlation with {topic}." };
        clausePools[DialogueIntent.Question] = new List<string> { "Has anyone else seen {topic}?", "What do you think is causing {topic}?", "Can we do anything about {topic}?" };
        clausePools[DialogueIntent.Reply] = new List<string> { "I see.", "That makes sense.", "I understand.", "Agreed.", "Indeed." };
    }

    public string BuildLine(CharacterProfile speaker, ConversationThread thread, DialogueState state)
    {
        DialogueIntent intent = GetIntentForSpeaker(speaker, thread);

        if (!clausePools.ContainsKey(intent) || clausePools[intent].Count == 0)
        {
            intent = DialogueIntent.Reply;
        }

        string clause = clausePools[intent][Random.Range(0, clausePools[intent].Count)];
        return clause.Replace("{topic}", thread.rootTopic.GetDisplayName());
    }

    private DialogueIntent GetIntentForSpeaker(CharacterProfile speaker, ConversationThread thread)
    {
        // Character personality influences their preferred intent
        if (speaker.name == "Nova" && Random.value < 0.4f) return DialogueIntent.Challenge;
        if (speaker.name == "Echo" && Random.value < 0.5f) return DialogueIntent.Fear;
        if (speaker.name == "Lumen" && Random.value < 0.6f) return DialogueIntent.Theory;

        // Otherwise, use the phase-appropriate intent
        return thread.GetPhaseAppropriateIntent();
    }
}