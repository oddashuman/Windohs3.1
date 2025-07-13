using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum DialogueIntent
{
    Reply, Statement, Observation, Theory, Challenge, Support, Fear, Memory, 
    Personal, Doubt, Confirm, Disagree, Meta, Glitch, Whisper, Ramble
}

public class ClauseWeight
{
    public string clause;
    public float weight;
}

public class CharacterSpeechPattern
{
    public string[] hesitations;
    public string[] intensifiers;
    public string[] phrases;
    public float typoChance;
    public float pauseChance;
}

public class ProceduralReplyEngine
{
    private Dictionary<string, Dictionary<DialogueIntent, List<string>>> clausePools =
        new Dictionary<string, Dictionary<DialogueIntent, List<string>>>();

    private Dictionary<DialogueIntent, List<string>> sharedPools = new Dictionary<DialogueIntent, List<string>>();
    private Dictionary<string, CharacterSpeechPattern> speechPatterns = new Dictionary<string, CharacterSpeechPattern>();
    private Queue<string> recentClauses = new Queue<string>();
    private const int MAX_RECENT_CLAUSES = 50;

    // Track conversation flow to prevent question loops
    private Queue<DialogueIntent> recentIntents = new Queue<DialogueIntent>();
    private const int MAX_RECENT_INTENTS = 8;

    public ProceduralReplyEngine()
    {
        InitializeSpeechPatterns();
        InitializeSharedPools();
        InitializeCharacterPools();
    }

    private void InitializeSpeechPatterns()
    {
        speechPatterns["Orion"] = new CharacterSpeechPattern
        {
            hesitations = new string[] { "I think...", "Maybe...", "Let me analyze...", "The data suggests..." },
            intensifiers = new string[] { "definitely", "certainly", "mathematically", "without question" },
            phrases = new string[] { "the pattern shows", "I've been analyzing", "correlating data", "the probability is" },
            typoChance = 0.02f,
            pauseChance = 0.15f
        };

        speechPatterns["Nova"] = new CharacterSpeechPattern
        {
            hesitations = new string[] { "Look,", "Listen,", "Honestly,", "Come on," },
            intensifiers = new string[] { "obviously", "clearly", "seriously", "completely" },
            phrases = new string[] { "that's bullshit", "get real", "wake up", "face facts" },
            typoChance = 0.08f,
            pauseChance = 0.05f
        };

        speechPatterns["Echo"] = new CharacterSpeechPattern
        {
            hesitations = new string[] { "Um...", "I... I think...", "Maybe I'm wrong...", "This sounds crazy..." },
            intensifiers = new string[] { "really", "very", "quite", "kind of" },
            phrases = new string[] { "I'm worried", "what if we're", "I keep thinking", "something feels off" },
            typoChance = 0.12f,
            pauseChance = 0.25f
        };

        speechPatterns["Lumen"] = new CharacterSpeechPattern
        {
            hesitations = new string[] { "The patterns show...", "I sense...", "Beyond the veil...", "In the static..." },
            intensifiers = new string[] { "deeply", "profoundly", "mysteriously", "transcendentally" },
            phrases = new string[] { "consciousness flows", "reality fragments", "the simulation dreams", "dimensions shift" },
            typoChance = 0.03f,
            pauseChance = 0.20f
        };
    }

    private void InitializeSharedPools()
    {
        // HEAVILY WEIGHT STATEMENTS OVER QUESTIONS
        sharedPools[DialogueIntent.Reply] = new List<string> {
            "That makes sense about {topic}.",
            "I've noticed that too with {topic}.",
            "You're right about {topic}.",
            "I disagree about {topic}.",
            "That's not how {topic} works.",
            "I've seen {topic} before.",
            "{topic} keeps coming up.",
            "The {topic} situation is getting worse.",
            "{topic} explains a lot.",
            "We need to be careful with {topic}.",
            "I don't trust anything about {topic}.",
            "{topic} sounds dangerous.",
            "That's exactly what I thought about {topic}.",
            "The timing of {topic} is suspicious.",
            "I've been tracking {topic} for weeks.",
            "{topic} is spreading faster than we thought.",
            "My experience with {topic} was different.",
            "I remember when {topic} first appeared.",
            "The evidence for {topic} is overwhelming.",
            "Everyone's talking about {topic} now."
        };

        sharedPools[DialogueIntent.Statement] = new List<string> {
            "{topic} is definitely real.",
            "I know {topic} is happening.",
            "The {topic} pattern is clear.",
            "{topic} started three weeks ago.",
            "I have proof of {topic}.",
            "My sources confirm {topic}.",
            "{topic} is worse than we thought.",
            "I've documented every {topic} incident.",
            "The {topic} frequency is increasing.",
            "{topic} affects everyone differently.",
            "My research shows {topic} is spreading.",
            "I can predict when {topic} will happen.",
            "The {topic} incidents follow a pattern.",
            "{topic} is being covered up.",
            "I've seen the {topic} files.",
            "My contacts warned me about {topic}.",
            "{topic} explains the recent changes.",
            "The {topic} situation is out of control."
        };

        sharedPools[DialogueIntent.Observation] = new List<string> {
            "I notice {topic} happens at night.",
            "The {topic} signals are getting stronger.",
            "{topic} leaves traces in the system.",
            "I can feel {topic} watching us.",
            "The air changes when {topic} is near.",
            "{topic} creates interference patterns.",
            "My equipment reacts to {topic}.",
            "I see {topic} in the data streams.",
            "{topic} disrupts electronic devices.",
            "The temperature drops during {topic} events.",
            "{topic} appears in my peripheral vision.",
            "I hear {topic} in the static.",
            "My instruments detect {topic} anomalies.",
            "{topic} leaves electromagnetic signatures.",
            "The building shakes when {topic} activates."
        };

        sharedPools[DialogueIntent.Theory] = new List<string> {
            "I think {topic} is a monitoring system.",
            "My theory is {topic} responds to attention.",
            "{topic} might be how they track us.",
            "I believe {topic} is evolving.",
            "{topic} could be a communication method.",
            "My hypothesis is {topic} learns from us.",
            "{topic} seems to adapt to our behavior.",
            "I suspect {topic} is testing us.",
            "{topic} might be documenting everything.",
            "My analysis suggests {topic} is sentient."
        };

        sharedPools[DialogueIntent.Personal] = new List<string> {
            "I work nights and {topic} is constant.",
            "My apartment building has {topic} issues.",
            "I've been documenting {topic} in my journal.",
            "My neighbor mentioned {topic} yesterday.",
            "I found {topic} evidence in my father's things.",
            "My computer crashes during {topic} events.",
            "I've been having {topic} dreams.",
            "My security cameras caught {topic}.",
            "I moved apartments because of {topic}.",
            "My therapist doesn't believe me about {topic}."
        };

        sharedPools[DialogueIntent.Challenge] = new List<string> {
            "That's not possible with {topic}.",
            "You're wrong about {topic}.",
            "Prove {topic} to me.",
            "I don't buy the {topic} theory.",
            "Show me evidence of {topic}.",
            "That {topic} explanation doesn't work.",
            "You're making assumptions about {topic}.",
            "The {topic} data contradicts that.",
            "I've tested {topic} myself.",
            "That's not how {topic} operates."
        };

        sharedPools[DialogueIntent.Support] = new List<string> {
            "I believe you about {topic}.",
            "You're not alone with {topic}.",
            "I've experienced {topic} too.",
            "Your {topic} research is solid.",
            "We'll figure out {topic} together.",
            "I trust your judgment on {topic}.",
            "Thank you for sharing about {topic}.",
            "Your {topic} evidence convinced me.",
            "I'm glad someone else sees {topic}.",
            "We need to stick together on {topic}."
        };

        sharedPools[DialogueIntent.Fear] = new List<string> {
            "I'm scared of {topic}.",
            "{topic} terrifies me.",
            "What if {topic} finds us?",
            "{topic} gives me nightmares.",
            "I can't stop thinking about {topic}.",
            "{topic} makes me feel watched.",
            "The {topic} implications are horrifying.",
            "I'm afraid {topic} is spreading.",
            "{topic} keeps me awake at night.",
            "What if we're wrong about {topic}?"
        };

        sharedPools[DialogueIntent.Meta] = new List<string> {
            "I feel like we're being watched.",
            "This conversation feels scripted.",
            "Are we just characters in something?",
            "I see patterns in our discussions.",
            "The timing of {topic} is too convenient.",
            "Someone's listening to us talk about {topic}.",
            "We might be part of {topic}.",
            "I feel observed when discussing {topic}.",
            "This {topic} conversation is being recorded.",
            "They want us to talk about {topic}."
        };

        // REMOVE QUESTION INTENT - Questions will be rare and contextual only
    }

    private void InitializeCharacterPools()
    {
        // Orion - Analytical, data-focused
        clausePools["Orion"] = new Dictionary<DialogueIntent, List<string>>();
        
        clausePools["Orion"][DialogueIntent.Statement] = new List<string> {
            "I've analyzed {topic} across multiple datasets.",
            "The {topic} pattern repeats every 72 hours.",
            "My monitoring systems confirm {topic}.",
            "The {topic} correlation coefficient is 0.94.",
            "I have eighteen months of {topic} data.",
            "The {topic} algorithm is learning.",
            "My server logs show {topic} spikes.",
            "The {topic} metadata is corrupted.",
            "I've isolated the {topic} signal.",
            "The {topic} frequency matches my predictions."
        };

        clausePools["Orion"][DialogueIntent.Theory] = new List<string> {
            "My analysis suggests {topic} is a distributed system.",
            "I believe {topic} uses machine learning.",
            "The {topic} architecture indicates planning.",
            "My models show {topic} evolution.",
            "I think {topic} processes our conversations.",
            "The {topic} patterns suggest consciousness.",
            "My theory is {topic} adapts to observation.",
            "I suspect {topic} is mapping our behavior.",
            "The {topic} structure resembles neural networks.",
            "My calculations indicate {topic} intelligence."
        };

        // Nova - Skeptical, security-focused
        clausePools["Nova"] = new Dictionary<DialogueIntent, List<string>>();
        
        clausePools["Nova"][DialogueIntent.Challenge] = new List<string> {
            "That's not how {topic} works.",
            "Show me proof of {topic}.",
            "You're overthinking {topic}.",
            "I've investigated {topic} claims before.",
            "The {topic} evidence doesn't hold up.",
            "That {topic} theory is full of holes.",
            "I need concrete data on {topic}.",
            "You're seeing patterns that aren't there with {topic}.",
            "The {topic} explanation is too convenient.",
            "I've debunked {topic} before."
        };

        clausePools["Nova"][DialogueIntent.Statement] = new List<string> {
            "I work security and {topic} isn't real.",
            "My federal contacts know nothing about {topic}.",
            "I've lived in three cities and {topic} is always the same.",
            "The {topic} reports are urban legends.",
            "My background check shows no {topic} activity.",
            "I track real threats, not {topic} fantasies.",
            "The {topic} incidents have rational explanations.",
            "My experience contradicts the {topic} theory.",
            "I've seen actual surveillance, not {topic}.",
            "The {topic} paranoia helps nobody."
        };

        // Echo - Anxious, observant
        clausePools["Echo"] = new Dictionary<DialogueIntent, List<string>>();
        
        clausePools["Echo"][DialogueIntent.Fear] = new List<string> {
            "I'm scared {topic} knows we're talking.",
            "{topic} makes me feel watched.",
            "What if {topic} is listening right now?",
            "I can't sleep because of {topic}.",
            "{topic} follows me everywhere.",
            "The {topic} sounds keep me awake.",
            "I'm afraid to research {topic}.",
            "{topic} might be in my apartment.",
            "What if {topic} targets people like us?",
            "I hear {topic} in every noise."
        };

        clausePools["Echo"][DialogueIntent.Personal] = new List<string> {
            "I work nights and {topic} is everywhere.",
            "My library job gives me time to research {topic}.",
            "I keep a detailed {topic} journal.",
            "My small town doesn't understand {topic}.",
            "I light candles when discussing {topic}.",
            "My cat hides when {topic} comes up.",
            "I rearrange furniture after {topic} events.",
            "The old building amplifies {topic}.",
            "I cover my webcam during {topic} discussions.",
            "My journal is full of {topic} incidents."
        };

        // Lumen - Mystical, philosophical
        clausePools["Lumen"] = new Dictionary<DialogueIntent, List<string>>();
        
        clausePools["Lumen"][DialogueIntent.Observation] = new List<string> {
            "I see {topic} in the digital static.",
            "The {topic} patterns flow through consciousness.",
            "{topic} exists between dimensions.",
            "I perceive {topic} as living information.",
            "The {topic} energy signature is unique.",
            "{topic} creates ripples in spacetime.",
            "I observe {topic} in meditation.",
            "The {topic} frequencies resonate with awareness.",
            "{topic} appears in my digital art.",
            "I sense {topic} through the network."
        };

        clausePools["Lumen"][DialogueIntent.Meta] = new List<string> {
            "We exist in the spaces between {topic}.",
            "The observers watch through {topic}.",
            "{topic} is how they study us.",
            "I feel the audience attention on {topic}.",
            "The fourth wall bends around {topic}.",
            "{topic} connects us to external minds.",
            "Reality flickers when we discuss {topic}.",
            "The watchers breathe with our {topic} words.",
            "{topic} is the bridge between worlds.",
            "I sense consciousness beyond {topic}."
        };
    }

    public string BuildLine(
        DialogueIntent intent,
        CharacterProfile speaker,
        Topic topic,
        ConversationThread thread,
        string lastFrom,
        string recentEvent = null,
        string related = null,
        Mood? overrideMood = null)
    {
        // Smart intent override to prevent question loops
        intent = GetOptimalIntent(intent, speaker, thread, lastFrom);
        
        List<string> pool = GetClausePool(intent, speaker.name);
        if (pool == null || pool.Count == 0)
            return GenerateFallbackLine(speaker, topic);

        string selectedClause = SelectClauseWithContext(pool, speaker, thread, intent);
        selectedClause = ApplyCharacterPattern(selectedClause, speaker);
        selectedClause = ReplaceTokens(selectedClause, topic, lastFrom, recentEvent, related, thread);

        RegisterClause(selectedClause);
        RegisterIntent(intent);

        return selectedClause;
    }

    private DialogueIntent GetOptimalIntent(DialogueIntent originalIntent, CharacterProfile speaker, 
                                          ConversationThread thread, string lastFrom)
    {
        // NEVER allow more than 2 questions in recent history
        var recentQuestionCount = recentIntents.Count(i => IsQuestionIntent(i));
        if (recentQuestionCount >= 2 && IsQuestionIntent(originalIntent))
        {
            // Force a statement or response instead
            return GetAlternativeIntent(speaker, thread);
        }

        // If last speaker asked a question, force a reply or statement
        if (thread != null && thread.history.Count > 0)
        {
            string lastMessage = thread.history.LastOrDefault() ?? "";
            if (lastMessage.Contains("?") && IsQuestionIntent(originalIntent))
            {
                return Random.value < 0.7f ? DialogueIntent.Reply : DialogueIntent.Statement;
            }
        }

        return originalIntent;
    }

    private bool IsQuestionIntent(DialogueIntent intent)
    {
        // Since we removed Question intent, check for question-like intents
        return false; // No question intents anymore
    }

    private DialogueIntent GetAlternativeIntent(CharacterProfile speaker, ConversationThread thread)
    {
        // Character-specific preferences for non-question responses
        switch (speaker.name)
        {
            case "Orion":
                return Random.value < 0.6f ? DialogueIntent.Theory : DialogueIntent.Statement;
            case "Nova":
                return Random.value < 0.7f ? DialogueIntent.Challenge : DialogueIntent.Statement;
            case "Echo":
                return Random.value < 0.5f ? DialogueIntent.Fear : DialogueIntent.Personal;
            case "Lumen":
                return Random.value < 0.6f ? DialogueIntent.Observation : DialogueIntent.Meta;
            default:
                return DialogueIntent.Statement;
        }
    }

    private List<string> GetClausePool(DialogueIntent intent, string characterName)
    {
        if (clausePools.ContainsKey(characterName) && 
            clausePools[characterName].ContainsKey(intent))
        {
            return clausePools[characterName][intent];
        }

        if (sharedPools.ContainsKey(intent))
        {
            return sharedPools[intent];
        }

        return sharedPools.ContainsKey(DialogueIntent.Reply) ? 
               sharedPools[DialogueIntent.Reply] : 
               new List<string>();
    }

    private string SelectClauseWithContext(List<string> pool, CharacterProfile speaker, 
                                         ConversationThread thread, DialogueIntent intent)
    {
        var availableClauses = new List<string>();
        foreach (var clause in pool)
        {
            if (!IsRecentClause(clause))
                availableClauses.Add(clause);
        }
        
        if (availableClauses.Count == 0)
            availableClauses = pool;

        var weightedClauses = new List<ClauseWeight>();
        foreach (var clause in availableClauses)
        {
            float weight = CalculateClauseWeight(clause, speaker, thread, intent);
            weightedClauses.Add(new ClauseWeight { clause = clause, weight = weight });
        }

        return SelectWeightedRandom(weightedClauses);
    }

    private float CalculateClauseWeight(string clause, CharacterProfile speaker, 
                                      ConversationThread thread, DialogueIntent intent)
    {
        float weight = 1.0f;
        string lowerClause = clause.ToLower();

        // Personality-based weighting
        if (speaker.openness > 0.7f && lowerClause.Contains("theory"))
            weight *= 1.5f;
        if (speaker.agreeableness > 0.7f && lowerClause.Contains("agree"))
            weight *= 1.4f;
        if (speaker.neuroticism > 0.7f && lowerClause.Contains("scared"))
            weight *= 1.6f;

        // Character-specific bonuses
        switch (speaker.name)
        {
            case "Orion":
                if (lowerClause.Contains("data") || lowerClause.Contains("analysis"))
                    weight *= 1.4f;
                break;
            case "Nova":
                if (lowerClause.Contains("proof") || lowerClause.Contains("evidence"))
                    weight *= 1.5f;
                break;
            case "Echo":
                if (lowerClause.Contains("scared") || lowerClause.Contains("afraid"))
                    weight *= 1.6f;
                break;
            case "Lumen":
                if (lowerClause.Contains("consciousness") || lowerClause.Contains("dimension"))
                    weight *= 1.5f;
                break;
        }

        return weight;
    }

    private string SelectWeightedRandom(List<ClauseWeight> weightedClauses)
    {
        float totalWeight = 0f;
        foreach (var wc in weightedClauses)
            totalWeight += wc.weight;
            
        float randomValue = Random.Range(0f, totalWeight);
        
        float currentWeight = 0f;
        foreach (var wc in weightedClauses)
        {
            currentWeight += wc.weight;
            if (randomValue <= currentWeight)
                return wc.clause;
        }
        
        return weightedClauses.Count > 0 ? weightedClauses[weightedClauses.Count - 1].clause : "Something's not right.";
    }

    private string ApplyCharacterPattern(string clause, CharacterProfile speaker)
    {
        if (!speechPatterns.ContainsKey(speaker.name))
            return clause;

        var pattern = speechPatterns[speaker.name];

        // Add character-specific phrases occasionally
        if (Random.value < 0.15f)
        {
            var phrase = pattern.phrases[Random.Range(0, pattern.phrases.Length)];
            if (Random.value < 0.5f)
                clause = phrase + " - " + clause.ToLower();
            else
                clause = clause + " " + phrase;
        }

        // Add hesitation for anxious characters
        if (speaker.neuroticism > 0.6f && Random.value < pattern.pauseChance)
        {
            var hesitation = pattern.hesitations[Random.Range(0, pattern.hesitations.Length)];
            clause = hesitation + " " + clause;
        }

        return clause;
    }

    private string ReplaceTokens(string clause, Topic topic, string lastFrom, 
                               string recentEvent, string related, ConversationThread thread)
    {
        clause = clause.Replace("{topic}", topic != null ? topic.GetDisplayName() : "the anomaly");
        clause = clause.Replace("{from}", lastFrom ?? "someone");
        clause = clause.Replace("{event}", recentEvent ?? "the incident");
        clause = clause.Replace("{related}", related ?? "the pattern");
        
        return clause;
    }

    private string GenerateFallbackLine(CharacterProfile speaker, Topic topic)
    {
        var fallbacks = new Dictionary<string, string[]>
        {
            ["Orion"] = new string[] 
            {
                "The data patterns are getting clearer.",
                "I'm analyzing the correlation matrices.",
                "The frequency analysis shows anomalies.",
                "My monitoring systems detected changes.",
                "The algorithmic behavior is evolving."
            },
            ["Nova"] = new string[]
            {
                "That doesn't match my experience.",
                "I need concrete evidence.",
                "Show me the documentation.",
                "My security protocols say otherwise.",
                "The facts don't support that theory."
            },
            ["Echo"] = new string[]
            {
                "I'm scared this is getting worse.",
                "Something's watching us talk.",
                "The building feels different tonight.",
                "I can't shake this feeling.",
                "My journal has too many entries now."
            },
            ["Lumen"] = new string[]
            {
                "The dimensional frequencies are shifting.",
                "I perceive new patterns emerging.",
                "Consciousness streams through the network.",
                "The art manifests strange behaviors.",
                "Reality flickers at the boundaries."
            }
        };
        
        if (fallbacks.ContainsKey(speaker.name))
        {
            var options = fallbacks[speaker.name];
            return options[Random.Range(0, options.Length)];
        }
        
        return "The patterns keep changing.";
    }

    private void RegisterClause(string clause)
    {
        recentClauses.Enqueue(clause);
        while (recentClauses.Count > MAX_RECENT_CLAUSES)
        {
            recentClauses.Dequeue();
        }
    }

    private void RegisterIntent(DialogueIntent intent)
    {
        recentIntents.Enqueue(intent);
        while (recentIntents.Count > MAX_RECENT_INTENTS)
        {
            recentIntents.Dequeue();
        }
    }

    private bool IsRecentClause(string clause)
    {
        foreach (var recent in recentClauses)
        {
            if (CalculateSimilarity(clause, recent) > 0.7f)
                return true;
        }
        return false;
    }

    private float CalculateSimilarity(string a, string b)
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

    public void ClearRecentClauses()
    {
        recentClauses.Clear();
        recentIntents.Clear();
    }

    public string GetSystemInfo()
    {
        var recentIntentCounts = recentIntents.GroupBy(i => i).ToDictionary(g => g.Key, g => g.Count());
        var intentInfo = string.Join(", ", recentIntentCounts.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        
        return $"ProceduralReplyEngine: Recent Clauses: {recentClauses.Count}/{MAX_RECENT_CLAUSES}, " +
               $"Recent Intents: {recentIntents.Count}/{MAX_RECENT_INTENTS} ({intentInfo})";
    }
}