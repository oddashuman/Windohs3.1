using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// **Complete Code: CharacterProfile**
/// Vision: Implements deep character personalities using the Big Five model.
/// This script defines who the characters are, how they feel, and how they relate
/// to one another. Their traits directly influence their dialogue, typing style,
/// and reactions to narrative events, making them feel like real, evolving entities.
/// </summary>
public class CharacterProfile
{
    public string name;
    public enum Mood { Neutral, Curious, Suspicious, Paranoid, Playful, Frustrated, Inspired, Scared }
    public Mood mood = Mood.Neutral;

    // --- Big Five Personality Model (OCEAN) ---
    public float openness;          // Curiosity, creativity
    public float conscientiousness; // Discipline, reliability
    public float extraversion;      // Social energy, assertiveness
    public float agreeableness;     // Cooperation, empathy
    public float neuroticism;       // Emotional instability, anxiety

    // --- Dynamic State ---
    public float curiosity = 0.5f;
    public float suspicion = 0.2f;
    public float fear = 0.1f;

    // --- Relationships ---
    public Dictionary<string, RelationshipData> relationships = new Dictionary<string, RelationshipData>();

    // --- Behavioral Traits ---
    public float typingSpeedMultiplier = 1.0f;
    public float typoFrequency = 0.03f;
    public float hesitationChance = 0.1f;
    public bool deletesAndRetypes = false;

    public CharacterProfile(string name)
    {
        this.name = name;
        InitializePersonality();
    }

    private void InitializePersonality()
    {
        switch (name)
        {
            case "Orion":
                openness = 0.9f; conscientiousness = 0.8f; extraversion = 0.4f; agreeableness = 0.6f; neuroticism = 0.3f;
                typingSpeedMultiplier = 1.1f; typoFrequency = 0.02f; hesitationChance = 0.15f; deletesAndRetypes = true;
                break;
            case "Nova":
                openness = 0.3f; conscientiousness = 0.7f; extraversion = 0.8f; agreeableness = 0.2f; neuroticism = 0.4f;
                typingSpeedMultiplier = 1.3f; typoFrequency = 0.05f; hesitationChance = 0.05f; deletesAndRetypes = false;
                break;
            case "Echo":
                openness = 0.6f; conscientiousness = 0.4f; extraversion = 0.2f; agreeableness = 0.8f; neuroticism = 0.9f;
                typingSpeedMultiplier = 0.7f; typoFrequency = 0.08f; hesitationChance = 0.35f; deletesAndRetypes = true;
                break;
            case "Lumen":
                openness = 0.95f; conscientiousness = 0.3f; extraversion = 0.6f; agreeableness = 0.7f; neuroticism = 0.2f;
                typingSpeedMultiplier = 0.9f; typoFrequency = 0.03f; hesitationChance = 0.20f; deletesAndRetypes = false;
                break;
        }
    }

    /// <summary>
    /// Updates the character's mood based on their personality and current state.
    /// </summary>
    public void UpdateMood()
    {
        if (fear > 0.7f && neuroticism > 0.6f) mood = Mood.Scared;
        else if (suspicion > 0.8f) mood = Mood.Paranoid;
        else if (suspicion > 0.6f) mood = Mood.Suspicious;
        else if (curiosity > 0.8f && openness > 0.7f) mood = Mood.Curious;
        else if (DialogueState.Instance.globalTension > 0.8f) mood = Mood.Frustrated;
        else mood = Mood.Neutral;
    }

    /// <summary>
    /// Checks if a character should hesitate before typing certain content,
    /// based on their personality and the topic's sensitivity.
    /// </summary>
    public bool ShouldHesitateOnTopic(string topicText)
    {
        if (string.IsNullOrEmpty(topicText)) return false;
        string lowerTopic = topicText.ToLower();
        
        // High neuroticism characters hesitate on sensitive topics
        if (neuroticism > 0.7f)
        {
            string[] sensitiveWords = { "overseer", "watching", "monitored", "escape", "real" };
            if (sensitiveWords.Any(word => lowerTopic.Contains(word)))
                return true;
        }
        return false;
    }
    
    public float GetTypingSpeedMultiplier(string messageContent)
    {
         float multiplier = typingSpeedMultiplier;
         if (ShouldHesitateOnTopic(messageContent)) multiplier *= 0.7f; // Slow down on sensitive topics
         if (mood == Mood.Frustrated || mood == Mood.Scared) multiplier *= 0.8f;
         if (mood == Mood.Inspired) multiplier *= 1.2f;
         return multiplier;
    }

    public class RelationshipData
    {
        public float trust = 0.5f;
        public float tension = 0.0f;
    }
}