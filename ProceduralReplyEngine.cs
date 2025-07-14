using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// **Complete Code: ProceduralReplyEngine**
/// Vision: A sophisticated text generator that crafts dialogue based on a character's
/// personality, mood, and the current narrative context. It selects a specific
/// "intent" for each line, ensuring the generated text feels purposeful and authentic.
/// </summary>
public class ProceduralReplyEngine
{
    private enum DialogueIntent { Statement, Theory, Challenge, Fear, Observation, Question }
    private Dictionary<DialogueIntent, List<string>> clausePools = new Dictionary<DialogueIntent, List<string>>();

    public ProceduralReplyEngine()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        // Populate with a wide variety of sentence fragments for each intent
        clausePools[DialogueIntent.Statement] = new List<string> { "I'm certain that {topic} is the cause.", "The evidence for {topic} is undeniable.", "We can't ignore {topic} any longer." };
        clausePools[DialogueIntent.Theory] = new List<string> { "My hypothesis is that {topic} is a side effect of the system resets.", "What if {topic} is how they are monitoring us?", "I believe {topic} is a form of communication." };
        clausePools[DialogueIntent.Challenge] = new List<string> { "That doesn't explain {topic}.", "I disagree, the data on {topic} is flawed.", "You have no proof that's related to {topic}." };
        clausePools[DialogueIntent.Fear] = new List<string> { "I'm terrified of what {topic} means.", "Talking about {topic} feels dangerous.", "What if {topic} finds us?" };
        clausePools[DialogueIntent.Observation] = new List<string> { "I've noticed {topic} only happens after a glitch.", "The frequency of {topic} is increasing.", "There's a pattern to {topic} that we're missing." };
        clausePools[DialogueIntent.Question] = new List<string> { "Has anyone else seen {topic}?", "What do you think is causing {topic}?", "How can we stop {topic}?" };
    }

    public string BuildLine(CharacterProfile speaker, ConversationThread thread, DialogueState state)
    {
        DialogueIntent intent = DetermineIntent(speaker, state);
        
        if (!clausePools.ContainsKey(intent) || clausePools[intent].Count == 0) return "";

        string clause = clausePools[intent][Random.Range(0, clausePools[intent].Count)];
        
        // Replace tokens
        clause = clause.Replace("{topic}", thread.rootTopic.GetDisplayName());

        return clause;
    }

    private DialogueIntent DetermineIntent(CharacterProfile speaker, DialogueState state)
    {
        // This is the core logic that makes characters feel unique.
        float rand = Random.value;

        // Mood-based intent
        switch (speaker.mood)
        {
            case CharacterProfile.Mood.Scared: return DialogueIntent.Fear;
            case CharacterProfile.Mood.Paranoid: return rand < 0.5f ? DialogueIntent.Fear : DialogueIntent.Challenge;
            case CharacterProfile.Mood.Curious: return rand < 0.6f ? DialogueIntent.Question : DialogueIntent.Theory;
        }

        // Personality-based intent
        if (speaker.agreeableness < 0.3f && rand < 0.4f) return DialogueIntent.Challenge;
        if (speaker.openness > 0.8f && rand < 0.5f) return DialogueIntent.Theory;
        if (speaker.neuroticism > 0.8f && rand < 0.4f) return DialogueIntent.Fear;
        
        // Default to a simple statement
        return DialogueIntent.Statement;
    }
}

// Minimalist placeholder for DialogueMessage if it's not defined elsewhere
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