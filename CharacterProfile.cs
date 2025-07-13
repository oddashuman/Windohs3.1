using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Mood
{
    Neutral, Curious, Suspicious, Paranoid, Playful, Frustrated, Inspired, Scared
}

public class RelationshipData
{
    public float trust = 0.5f;
    public float respect = 0.5f;
    public float intimacy = 0.3f;
    public float tension = 0.1f;
    public List<string> sharedMemories = new List<string>();
    public float lastInteractionTime;
    public int conversationCount = 0;
    public float emotionalBond = 0.0f;
    public List<string> conflictHistory = new List<string>();
    public List<string> supportHistory = new List<string>();
}

public class CharacterProfile
{
    public string name;
    public CharacterType type;
    public Mood mood = Mood.Neutral;

    // Big Five Personality Model (OCEAN)
    public float openness = 0.5f;           // Intellectual curiosity, creativity
    public float conscientiousness = 0.5f;  // Organization, discipline, reliability
    public float extraversion = 0.5f;       // Social energy, assertiveness
    public float agreeableness = 0.5f;      // Cooperation, trust, empathy
    public float neuroticism = 0.5f;        // Emotional instability, anxiety

    // Enhanced relationship tracking
    public Dictionary<string, RelationshipData> relationships = new Dictionary<string, RelationshipData>();

    // Social state (legacy compatibility)
    public Dictionary<string, float> trustLevels = new Dictionary<string, float>();
    public HashSet<string> friends = new HashSet<string>();
    public HashSet<string> rivals = new HashSet<string>();

    // Beliefs/memories
    public HashSet<string> believedTopics = new HashSet<string>();
    public HashSet<string> doubtedTopics = new HashSet<string>();
    public List<string> personalRumors = new List<string>();
    public List<string> sessionMemory = new List<string>();

    // Enhanced psychological state
    public float curiosity = 0.6f;
    public float suspicion = 0.15f;
    public float paranoia = 0.08f;
    public float playfulness = 0.2f;
    public float fear = 0.0f;
    public float trustOrion = 0.5f;

    // Personal background elements for authentic dialogue
    public string currentLocation = "unknown";
    public string occupation = "unknown";
    public List<string> personalExperiences = new List<string>();
    public List<string> phobias = new List<string>();
    public List<string> interests = new List<string>();

    // Typing behavior patterns
    public float typingSpeed = 1.0f;        // Base typing speed multiplier
    public float typoFrequency = 0.03f;     // How often they make typos
    public float hesitationChance = 0.1f;   // Likelihood to pause while typing
    public bool deletesAndRetypes = false;  // Tendency to delete and retype sensitive content

    // Active conversation
    public Topic currentTopic = null;
    public string lastSpokenTo = null;
    public string lastHeardFrom = null;

    // Character seed for procedural variety
    private System.Random rng;

    public CharacterProfile(string name, CharacterType type)
    {
        this.name = name;
        this.type = type;
        rng = new System.Random(name.GetHashCode() ^ DateTime.Now.Millisecond);
        
        InitializePersonalityBasedTraits();
        InitializePersonalBackground();
    }

    private void InitializePersonalityBasedTraits()
    {
        // Set personality traits based on character
        switch (name)
        {
            case "Orion":
                openness = 0.9f;
                conscientiousness = 0.8f;
                extraversion = 0.4f;
                agreeableness = 0.6f;
                neuroticism = 0.3f;
                
                typingSpeed = 1.1f;
                typoFrequency = 0.02f;
                hesitationChance = 0.15f;
                deletesAndRetypes = true;
                
                currentLocation = "research facility basement";
                occupation = "data analyst";
                interests.AddRange(new[] { "pattern recognition", "system architecture", "probability theory" });
                break;

            case "Nova":
                openness = 0.3f;
                conscientiousness = 0.7f;
                extraversion = 0.8f;
                agreeableness = 0.2f;
                neuroticism = 0.4f;
                
                typingSpeed = 1.3f;
                typoFrequency = 0.05f;
                hesitationChance = 0.05f;
                deletesAndRetypes = false;
                
                currentLocation = "urban apartment complex";
                occupation = "network security";
                interests.AddRange(new[] { "cybersecurity", "urban exploration", "skepticism" });
                break;

            case "Echo":
                openness = 0.6f;
                conscientiousness = 0.4f;
                extraversion = 0.2f;
                agreeableness = 0.8f;
                neuroticism = 0.9f;
                
                typingSpeed = 0.7f;
                typoFrequency = 0.08f;
                hesitationChance = 0.35f;
                deletesAndRetypes = true;
                
                currentLocation = "small town library";
                occupation = "night shift worker";
                phobias.AddRange(new[] { "surveillance", "loud noises", "crowds" });
                interests.AddRange(new[] { "old books", "quiet spaces", "protective rituals" });
                break;

            case "Lumen":
                openness = 0.95f;
                conscientiousness = 0.3f;
                extraversion = 0.6f;
                agreeableness = 0.7f;
                neuroticism = 0.2f;
                
                typingSpeed = 0.9f;
                typoFrequency = 0.03f;
                hesitationChance = 0.20f;
                deletesAndRetypes = false;
                
                currentLocation = "converted warehouse studio";
                occupation = "digital artist / philosopher";
                interests.AddRange(new[] { "consciousness studies", "digital art", "meditation", "quantum theory" });
                break;
        }
    }

    private void InitializePersonalBackground()
    {
        // Add some personal experiences for authentic dialogue
        switch (name)
        {
            case "Orion":
                personalExperiences.AddRange(new[] {
                    "worked late nights in server rooms",
                    "noticed patterns in system logs",
                    "had recurring dreams about data streams",
                    "found anomalies in network traffic"
                });
                break;

            case "Nova":
                personalExperiences.AddRange(new[] {
                    "investigated security breaches",
                    "tracked down hackers",
                    "worked with federal agencies",
                    "lived in three different cities"
                });
                break;

            case "Echo":
                personalExperiences.AddRange(new[] {
                    "worked alone during night shifts",
                    "heard strange sounds in empty buildings",
                    "noticed cameras in unexpected places",
                    "kept detailed journals of unusual events"
                });
                break;

            case "Lumen":
                personalExperiences.AddRange(new[] {
                    "studied consciousness at university",
                    "experimented with digital meditation",
                    "created art that seemed to move on its own",
                    "experienced time differently during creative sessions"
                });
                break;
        }
    }

    public void UpdateMood()
    {
        // Enhanced mood calculation based on Big Five traits
        float stressLevel = (neuroticism * paranoia) + (fear * 0.5f);
        float socialEnergy = extraversion * (1.0f - neuroticism * 0.3f);
        float intellectualEngagement = openness * curiosity;

        if (stressLevel > 0.7f)
            mood = Mood.Paranoid;
        else if (fear > 0.5f && neuroticism > 0.6f)
            mood = Mood.Scared;
        else if (suspicion > 0.6f && agreeableness < 0.4f)
            mood = Mood.Suspicious;
        else if (intellectualEngagement > 0.7f)
            mood = Mood.Curious;
        else if (socialEnergy > 0.7f && playfulness > 0.5f)
            mood = Mood.Playful;
        else if (conscientiousness < 0.3f && neuroticism > 0.5f)
            mood = Mood.Frustrated;
        else if (openness > 0.8f && curiosity > 0.6f)
            mood = Mood.Inspired;
        else
            mood = Mood.Neutral;
    }

    public void HearTopic(Topic topic, string fromWho)
    {
        currentTopic = topic;
        lastHeardFrom = fromWho;
        
        // Personality-based reaction to new information
        curiosity += openness * 0.02f;
        
        if (neuroticism > 0.7f && topic.status == TopicStatus.Controversial)
        {
            fear += 0.05f;
            paranoia += 0.03f;
        }
        
        sessionMemory.Add($"Heard about {topic.GetDisplayName()} from {fromWho}");
        UpdateRelationship(fromWho, "shared_information", topic.GetDisplayName());
    }

    public void SpeakTo(string who)
    {
        lastSpokenTo = who;
        
        // Update relationship data
        UpdateRelationship(who, "conversation", null);
        
        // Legacy compatibility
        trustLevels.TryGetValue(who, out float trust);
        trust += agreeableness * 0.01f; // More agreeable characters build trust faster
        trustLevels[who] = Mathf.Clamp01(trust);
    }

    public void UpdateRelationship(string otherCharacter, string interactionType, string context = null)
    {
        if (!relationships.ContainsKey(otherCharacter))
        {
            relationships[otherCharacter] = new RelationshipData();
        }

        var relationship = relationships[otherCharacter];
        relationship.lastInteractionTime = Time.time;
        relationship.conversationCount++;

        // Update relationship based on interaction type and personality
        switch (interactionType)
        {
            case "conversation":
                relationship.intimacy += (extraversion + agreeableness) * 0.005f;
                relationship.trust += conscientiousness * 0.003f;
                break;

            case "disagreement":
                relationship.tension += (1.0f - agreeableness) * 0.1f;
                relationship.trust -= neuroticism * 0.05f;
                if (context != null)
                    relationship.conflictHistory.Add(context);
                break;

            case "support":
                relationship.trust += agreeableness * 0.08f;
                relationship.emotionalBond += (agreeableness + extraversion) * 0.05f;
                if (context != null)
                    relationship.supportHistory.Add(context);
                break;

            case "shared_information":
                relationship.intimacy += openness * 0.01f;
                if (context != null && !relationship.sharedMemories.Contains(context))
                    relationship.sharedMemories.Add(context);
                break;
        }

        // Clamp values
        relationship.trust = Mathf.Clamp01(relationship.trust);
        relationship.respect = Mathf.Clamp01(relationship.respect);
        relationship.intimacy = Mathf.Clamp01(relationship.intimacy);
        relationship.tension = Mathf.Clamp01(relationship.tension);
        relationship.emotionalBond = Mathf.Clamp01(relationship.emotionalBond);
    }

    public RelationshipData GetRelationship(string otherCharacter)
    {
        return relationships.GetValueOrDefault(otherCharacter, new RelationshipData());
    }

    public float GetPersonalityCompatibility(CharacterProfile other)
    {
        if (other == null) return 0.5f;

        // Calculate personality compatibility
        float compatibility = 0f;
        
        // Similar openness creates intellectual connection
        compatibility += 1.0f - Mathf.Abs(openness - other.openness);
        
        // Complementary extraversion (one can be social, one can listen)
        compatibility += 1.0f - Mathf.Abs(extraversion - (1.0f - other.extraversion)) * 0.5f;
        
        // High agreeableness generally improves compatibility
        compatibility += (agreeableness + other.agreeableness) * 0.5f;
        
        // Low neuroticism helps stability
        compatibility += (2.0f - neuroticism - other.neuroticism) * 0.3f;
        
        // Conscientiousness difference can be complementary
        compatibility += 1.0f - Mathf.Abs(conscientiousness - other.conscientiousness) * 0.7f;
        
        return Mathf.Clamp01(compatibility / 4.3f); // Normalize
    }

    public string GetPersonalExperience()
    {
        if (personalExperiences.Count == 0) return null;
        return personalExperiences[rng.Next(personalExperiences.Count)];
    }

    public bool ShouldHesitateOnTopic(string topicText)
    {
        if (string.IsNullOrEmpty(topicText)) return false;
        
        string lowerTopic = topicText.ToLower();
        
        // High neuroticism characters hesitate on sensitive topics
        if (neuroticism > 0.7f)
        {
            string[] sensitiveWords = { "overseer", "watching", "monitored", "surveillance", "real", "escape" };
            if (sensitiveWords.Any(word => lowerTopic.Contains(word)))
                return true;
        }
        
        // Check personal phobias
        foreach (string phobia in phobias)
        {
            if (lowerTopic.Contains(phobia.ToLower()))
                return true;
        }
        
        return false;
    }

    public float GetTypingSpeedMultiplier(string messageContent)
    {
        float multiplier = typingSpeed;
        
        // Hesitation on sensitive topics
        if (ShouldHesitateOnTopic(messageContent))
            multiplier *= 0.6f;
        
        // Excitement speeds up typing for high openness characters
        if (openness > 0.8f && messageContent.Contains("!"))
            multiplier *= 1.2f;
        
        // Conscientiousness affects careful typing
        multiplier *= (1.0f + conscientiousness * 0.2f);
        
        return multiplier;
    }

    // More procedural memory/belief logic can go here...
}