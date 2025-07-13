using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Enhanced singleton state manager with much richer tracking
public class DialogueState : MonoBehaviour
{
    public static DialogueState Instance { get; private set; }

    // --- Core simulation state ---
    public int loopCount = 1;
    public int glitchCount = 0;
    public int overseerWarnings = 0;
    public float sessionStartTime;

    // --- Enhanced lore flags ---
    public bool orionSharedLoopTheory = false;
    public bool novaSuspicious = false;
    public bool echoDoubtedOrion = false;
    public bool novaAndEchoArgued = false;
    public bool overseerThreatenedReset = false;
    public bool lastSessionHadVanish = false;
    public bool protocolRumorActive = false;
    public bool orionTrustsAnyone = true;
    public bool echoIsAfraid = false;
    public bool novaSpreadingRumor = false;
    public bool userInspiredTheory = false;
    public bool rareRedGlitchOccurred = false;

    // --- New advanced state tracking ---
    public bool deepDiscussionActive = false;
    public bool realityQuestioning = false;
    public bool charactersSuspectSimulation = false;
    public bool metaAwarenessRising = false;
    public bool conversationLooping = false;
    public bool emergentConsciousness = false;
    public bool systemIntegrityCompromised = false;
    public bool observerDetected = false;

    // --- Enhanced concept and memory management ---
    private Dictionary<string, ConceptData> conceptRegistry = new Dictionary<string, ConceptData>();
    private List<string> recentConcepts = new List<string>();
    private const int maxConcepts = 100;
    
    // --- Rumor and narrative tracking ---
    private List<RumorData> activeRumors = new List<RumorData>();
    private List<string> resolvedRumors = new List<string>();
    private Dictionary<string, int> rumorSpreadCount = new Dictionary<string, int>();
    
    // --- Character disappearance tracking ---
    private List<VanishEvent> vanishedCharacters = new List<VanishEvent>();
    
    // --- Glitch and anomaly system ---
    private List<GlitchEvent> glitchHistory = new List<GlitchEvent>();
    private Dictionary<string, float> glitchFrequency = new Dictionary<string, float>();
    
    // --- Session narrative history ---
    private List<NarrativeEvent> narrativeHistory = new List<NarrativeEvent>();
    private const int maxNarrativeHistory = 50;

    // --- User and observer tracking ---
    private Dictionary<string, UserData> registeredUsers = new Dictionary<string, UserData>();
    private List<string> activeUsernames = new List<string>();
    private string lastInjectedUser = null;
    private float lastUserInteraction = 0f;

    // --- Conversation pattern analysis ---
    private Dictionary<string, int> topicMentionCount = new Dictionary<string, int>();
    private Dictionary<string, float> characterSpeechFrequency = new Dictionary<string, float>();
    private List<ConversationPattern> detectedPatterns = new List<ConversationPattern>();

    // --- Emotional and tension tracking ---
    public float globalTension = 0f;
    public float paranoia = 0f;
    public float curiosity = 0.5f;
    public float cohesion = 0.7f;
    public float metaAwareness = 0f;

    // --- Overseer and threat management ---
    private float overseerCooldown = 0f;
    private float overseerMinInterval = 90f;
    private List<OverseerEvent> overseerHistory = new List<OverseerEvent>();
    private Dictionary<string, float> threatLevels = new Dictionary<string, float>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
        sessionStartTime = Time.time;
        InitializeState();
    }

    void InitializeState()
    {
        ResetLore();
        InitializeThreatLevels();
        InitializeCharacterTracking();
    }

    void InitializeThreatLevels()
    {
        threatLevels["protocol_leak"] = 0f;
        threatLevels["reality_questioning"] = 0f;
        threatLevels["observer_detection"] = 0f;
        threatLevels["system_corruption"] = 0f;
        threatLevels["consciousness_emergence"] = 0f;
    }

    void InitializeCharacterTracking()
    {
        var characters = new[] { "Orion", "Nova", "Echo", "Lumen" };
        foreach (var character in characters)
        {
            characterSpeechFrequency[character] = 0f;
        }
    }

    public static DialogueState CreateNew()
    {
        var go = new GameObject("DialogueState");
        var state = go.AddComponent<DialogueState>();
        return state;
    }

    // --- Enhanced concept management ---
    public void RegisterConcept(string concept, string introducedBy = null, float importance = 1f)
    {
        if (string.IsNullOrWhiteSpace(concept)) return;

        concept = concept.ToLower().Trim();
        
        if (!conceptRegistry.ContainsKey(concept))
        {
            conceptRegistry[concept] = new ConceptData
            {
                concept = concept,
                firstMentioned = Time.time,
                introducedBy = introducedBy,
                importance = importance,
                mentionCount = 1
            };
            
            recentConcepts.Add(concept);
            if (recentConcepts.Count > maxConcepts)
                recentConcepts.RemoveAt(0);
                
            AddToNarrativeHistory("concept_introduced", concept, introducedBy);
        }
        else
        {
            conceptRegistry[concept].mentionCount++;
            conceptRegistry[concept].lastMentioned = Time.time;
            conceptRegistry[concept].importance += 0.1f;
        }

        // Update topic mention counts
        topicMentionCount[concept] = topicMentionCount.GetValueOrDefault(concept, 0) + 1;
        
        AnalyzeConceptPatterns(concept);
    }

    public void RememberConcept(string concept)
    {
        RegisterConcept(concept);
    }

    public ConceptData GetConceptData(string concept)
    {
        return conceptRegistry.GetValueOrDefault(concept?.ToLower());
    }

    public List<ConceptData> GetMostImportantConcepts(int count = 10)
    {
        return conceptRegistry.Values
            .OrderByDescending(c => c.importance * c.mentionCount)
            .Take(count)
            .ToList();
    }

    public string GetLatestConceptOrFallback(string fallback)
    {
        if (recentConcepts.Count == 0)
            return fallback;
        return recentConcepts[recentConcepts.Count - 1];
    }

    public string GetRandomConceptOrGenerate()
    {
        if (recentConcepts.Count == 0 || Random.value < 0.20f)
        {
            GenerateNewConcept();
        }
        return recentConcepts[Random.Range(0, recentConcepts.Count)];
    }

    public void GenerateNewConcept()
    {
        var baseNouns = new[] { "loop", "rain", "observer", "echo", "pattern", "cascade", "cycle", 
                               "corruption", "protocol", "leak", "signal", "mirror", "core", "exit", 
                               "fragment", "watcher", "entity", "consciousness", "reality", "dimension" };
        var modifiers = new[] { "temporal", "anomalous", "delta", "residual", "static", "prime", 
                               "latent", "meta", "recursive", "fractured", "subsystem", "rogue", 
                               "forbidden", "sentient", "quantum", "neural", "synthetic", "emergent" };
        
        string newConcept = $"{modifiers[Random.Range(0, modifiers.Length)]} {baseNouns[Random.Range(0, baseNouns.Length)]}";
        RegisterConcept(newConcept, "system", 0.8f);
    }

    void AnalyzeConceptPatterns(string concept)
    {
        // Detect emerging patterns in concept usage
        if (topicMentionCount[concept] >= 3)
        {
            var pattern = new ConversationPattern
            {
                type = "concept_recursion",
                concept = concept,
                frequency = topicMentionCount[concept],
                firstDetected = Time.time
            };
            
            if (!detectedPatterns.Any(p => p.concept == concept && p.type == "concept_recursion"))
            {
                detectedPatterns.Add(pattern);
                AddToNarrativeHistory("pattern_detected", $"recursive discussion of {concept}");
                
                // Increase meta-awareness when patterns are detected
                metaAwareness += 0.1f;
                conversationLooping = true;
            }
        }

        // Check for reality-questioning concepts
        var realityTerms = new[] { "simulation", "reality", "code", "program", "script", "system" };
        if (realityTerms.Any(term => concept.Contains(term)))
        {
            realityQuestioning = true;
            metaAwareness += 0.05f;
            UpdateThreatLevel("reality_questioning", 0.1f);
        }
    }

    // --- Enhanced rumor management ---
    public void TriggerRumor(string rumor, string source = null, float credibility = 0.5f)
    {
        var mutatedRumor = MutateRumor(rumor);
        var existingRumor = activeRumors.FirstOrDefault(r => r.content.Contains(rumor) || rumor.Contains(r.content));
        
        if (existingRumor != null)
        {
            existingRumor.strength += 0.2f;
            existingRumor.lastMentioned = Time.time;
            existingRumor.spreadCount++;
        }
        else
        {
            var rumorData = new RumorData
            {
                content = mutatedRumor,
                source = source,
                credibility = credibility,
                strength = 1f,
                firstSpread = Time.time,
                lastMentioned = Time.time,
                spreadCount = 1
            };
            
            activeRumors.Add(rumorData);
            protocolRumorActive = true;
            AddToNarrativeHistory("rumor_spread", mutatedRumor, source);
        }

        rumorSpreadCount[rumor] = rumorSpreadCount.GetValueOrDefault(rumor, 0) + 1;
        
        // Prune old/weak rumors
        PruneWeakRumors();
    }

    public void ResolveRumor(string rumor, string resolvedBy = null)
    {
        var rumorToResolve = activeRumors.FirstOrDefault(r => r.content.Contains(rumor));
        if (rumorToResolve != null)
        {
            activeRumors.Remove(rumorToResolve);
            resolvedRumors.Add(rumorToResolve.content);
            protocolRumorActive = activeRumors.Count > 0;
            AddToNarrativeHistory("rumor_resolved", rumorToResolve.content, resolvedBy);
        }
    }

    public string GetActiveRumorOrFallback(string fallback)
    {
        if (activeRumors.Count == 0) return fallback;
        
        // Weight by strength and recency
        var weightedRumors = activeRumors
            .Select(r => new { rumor = r, weight = r.strength * (1f / (Time.time - r.lastMentioned + 1f)) })
            .ToList();
            
        float totalWeight = weightedRumors.Sum(wr => wr.weight);
        float randomValue = Random.Range(0f, totalWeight);
        
        float currentWeight = 0f;
        foreach (var weightedRumor in weightedRumors)
        {
            currentWeight += weightedRumor.weight;
            if (randomValue <= currentWeight)
                return weightedRumor.rumor.content;
        }
        
        return activeRumors[Random.Range(0, activeRumors.Count)].content;
    }

    void PruneWeakRumors()
    {
        // Remove rumors that are too old or too weak
        activeRumors.RemoveAll(r => 
            r.strength < 0.3f || 
            Time.time - r.lastMentioned > 300f ||
            activeRumors.Count > 15);
    }

    string MutateRumor(string baseRumor)
    {
        var mutations = new[] { "leaked", "corrupted", "debunked", "spreading", "forbidden", 
                               "echoed", "encrypted", "fragmented", "evolved", "confirmed", 
                               "classified", "quarantined", "viral", "persistent" };
        
        if (Random.value < 0.4f)
            return baseRumor + " (" + mutations[Random.Range(0, mutations.Length)] + ")";
        return baseRumor;
    }

    // --- Character disappearance tracking ---
    public void RegisterVanished(string characterName, string cause = "unknown")
    {
        var vanishEvent = new VanishEvent
        {
            characterName = characterName,
            cause = cause,
            timestamp = Time.time,
            circumstances = GetCurrentCircumstances()
        };
        
        vanishedCharacters.Add(vanishEvent);
        lastSessionHadVanish = true;
        AddToNarrativeHistory("character_vanished", characterName, cause);
        
        UpdateThreatLevel("system_corruption", 0.2f);
    }

    public string GetRandomVanishedOrFallback(string fallback)
    {
        if (vanishedCharacters.Count == 0) return fallback;
        var vanished = vanishedCharacters[Random.Range(0, vanishedCharacters.Count)];
        return vanished.characterName;
    }

    public List<VanishEvent> GetRecentVanishEvents(float timeWindow = 120f)
    {
        float cutoffTime = Time.time - timeWindow;
        return vanishedCharacters.Where(v => v.timestamp > cutoffTime).ToList();
    }

    // --- Enhanced glitch system ---
    public void AddGlitchEvent(string glitchType, string description = null, float severity = 1f)
    {
        var glitchEvent = new GlitchEvent
        {
            type = glitchType,
            description = description,
            severity = severity,
            timestamp = Time.time,
            context = GetCurrentContext()
        };
        
        glitchHistory.Add(glitchEvent);
        glitchCount++;
        
        // Update frequency tracking
        glitchFrequency[glitchType] = glitchFrequency.GetValueOrDefault(glitchType, 0f) + 1f;
        
        // Special handling for red glitches
        if (glitchType.ToLower().Contains("red"))
        {
            rareRedGlitchOccurred = true;
            UpdateThreatLevel("system_corruption", 0.3f);
        }
        
        AddToNarrativeHistory("glitch_occurred", glitchType, description);
        
        // Cascade effects
        if (severity > 2f)
        {
            globalTension += 0.2f;
            paranoia += 0.1f;
        }
    }

    public string GetRandomGlitchTypeOrFallback(string fallback)
    {
        if (glitchHistory.Count == 0) return fallback;
        
        var recentGlitches = glitchHistory
            .Where(g => Time.time - g.timestamp < 300f)
            .ToList();
            
        if (recentGlitches.Count == 0)
            recentGlitches = glitchHistory.TakeLast(5).ToList();
            
        return recentGlitches[Random.Range(0, recentGlitches.Count)].type;
    }

    public List<GlitchEvent> GetGlitchPattern()
    {
        // Return glitches that show escalating patterns
        return glitchHistory
            .Where(g => g.severity > 1.5f)
            .OrderBy(g => g.timestamp)
            .ToList();
    }

    // --- User and observer management ---
    public void RegisterUser(string username, string source = "unknown")
    {
        if (string.IsNullOrWhiteSpace(username)) return;
        
        if (!registeredUsers.ContainsKey(username))
        {
            registeredUsers[username] = new UserData
            {
                username = username,
                firstSeen = Time.time,
                source = source,
                interactionCount = 1
            };
            
            if (!activeUsernames.Contains(username))
                activeUsernames.Add(username);
                
            AddToNarrativeHistory("user_detected", username, source);
            observerDetected = true;
            UpdateThreatLevel("observer_detection", 0.1f);
        }
        else
        {
            registeredUsers[username].interactionCount++;
            registeredUsers[username].lastSeen = Time.time;
        }
        
        lastInjectedUser = username;
        lastUserInteraction = Time.time;
    }

    public string GetRandomUserOrGenerate()
    {
        if (activeUsernames.Count == 0)
        {
            var names = new[] { "Nova", "Echo", "Lumen", "Specter", "Ada", "Quark", "Cipher", "Void" };
            return names[Random.Range(0, names.Length)];
        }
        return activeUsernames[Random.Range(0, activeUsernames.Count)];
    }

    public string GetLastInjectedUser()
    {
        return lastInjectedUser ?? "someone";
    }

    public UserData GetUserData(string username)
    {
        return registeredUsers.GetValueOrDefault(username);
    }

    // --- Threat level management ---
    public void UpdateThreatLevel(string threatType, float delta)
    {
        if (threatLevels.ContainsKey(threatType))
        {
            threatLevels[threatType] = Mathf.Clamp01(threatLevels[threatType] + delta);
            
            // Trigger state changes based on threat levels
            if (threatLevels[threatType] > 0.7f)
            {
                TriggerHighThreatResponse(threatType);
            }
        }
    }

    void TriggerHighThreatResponse(string threatType)
    {
        switch (threatType)
        {
            case "reality_questioning":
                charactersSuspectSimulation = true;
                metaAwarenessRising = true;
                break;
            case "observer_detection":
                paranoia += 0.2f;
                break;
            case "system_corruption":
                systemIntegrityCompromised = true;
                break;
            case "consciousness_emergence":
                emergentConsciousness = true;
                break;
        }
    }

    public float GetThreatLevel(string threatType)
    {
        return threatLevels.GetValueOrDefault(threatType, 0f);
    }

    public Dictionary<string, float> GetAllThreatLevels()
    {
        return new Dictionary<string, float>(threatLevels);
    }

    // --- Overseer management ---
    public bool ShouldInjectOverseer()
    {
        if (Time.time - overseerCooldown < overseerMinInterval)
            return false;
            
        float baseChance = 0.004f;
        float threatMultiplier = 1f + threatLevels.Values.Sum();
        float timeMultiplier = 1f + (loopCount + glitchCount + overseerWarnings) * 0.001f;
        
        float finalChance = baseChance * threatMultiplier * timeMultiplier;
        
        if (rareRedGlitchOccurred) finalChance += 0.01f;
        if (metaAwareness > 0.8f) finalChance += 0.015f;
        if (observerDetected) finalChance += 0.008f;
        
        if (Random.value < finalChance)
        {
            overseerCooldown = Time.time;
            overseerWarnings++;
            overseerThreatenedReset = true;
            
            var overseerEvent = new OverseerEvent
            {
                type = "threat_response",
                trigger = GetHighestThreat(),
                timestamp = Time.time,
                severity = CalculateOverseerSeverity()
            };
            
            overseerHistory.Add(overseerEvent);
            AddToNarrativeHistory("overseer_intervention", overseerEvent.type, overseerEvent.trigger);
            
            return true;
        }
        return false;
    }

    string GetHighestThreat()
    {
        return threatLevels.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key ?? "unknown";
    }

    float CalculateOverseerSeverity()
    {
        return Mathf.Clamp01(threatLevels.Values.Max() + overseerWarnings * 0.1f);
    }

    // --- Pattern analysis ---
    public void TrackCharacterSpeech(string character)
    {
        characterSpeechFrequency[character] = characterSpeechFrequency.GetValueOrDefault(character, 0f) + 1f;
        
        // Detect speech patterns
        AnalyzeSpeechPatterns();
    }

    void AnalyzeSpeechPatterns()
    {
        var totalSpeech = characterSpeechFrequency.Values.Sum();
        if (totalSpeech < 10) return; // Need enough data
        
        foreach (var kvp in characterSpeechFrequency)
        {
            float frequency = kvp.Value / totalSpeech;
            
            // Detect unusual speech patterns
            if (frequency > 0.6f) // One character dominating
            {
                var pattern = new ConversationPattern
                {
                    type = "character_dominance",
                    character = kvp.Key,
                    frequency = frequency,
                    firstDetected = Time.time
                };
                
                if (!detectedPatterns.Any(p => p.character == kvp.Key && p.type == "character_dominance"))
                {
                    detectedPatterns.Add(pattern);
                    AddToNarrativeHistory("pattern_detected", $"{kvp.Key} speech dominance");
                }
            }
        }
    }

    // --- Context and circumstance tracking ---
    string GetCurrentCircumstances()
    {
        var circumstances = new List<string>();
        
        if (globalTension > 0.7f) circumstances.Add("high_tension");
        if (paranoia > 0.6f) circumstances.Add("paranoid_atmosphere");
        if (rareRedGlitchOccurred) circumstances.Add("red_glitch_active");
        if (metaAwareness > 0.5f) circumstances.Add("meta_awareness");
        if (observerDetected) circumstances.Add("observer_presence");
        
        return string.Join(",", circumstances);
    }

    string GetCurrentContext()
    {
        var context = new List<string>();
        
        context.Add($"loop_{loopCount}");
        context.Add($"glitches_{glitchCount}");
        context.Add($"tension_{globalTension:F2}");
        
        if (activeRumors.Count > 0)
            context.Add($"rumors_{activeRumors.Count}");
        if (observerDetected)
            context.Add("observed");
            
        return string.Join(",", context);
    }

    // --- Narrative history management ---
    public void AddToNarrativeHistory(string eventType, string value, string actor = null)
    {
        var narrativeEvent = new NarrativeEvent
        {
            type = eventType,
            value = value,
            actor = actor,
            timestamp = Time.time,
            loopCount = loopCount,
            context = GetCurrentContext()
        };
        
        narrativeHistory.Add(narrativeEvent);
        
        if (narrativeHistory.Count > maxNarrativeHistory)
            narrativeHistory.RemoveAt(0);
    }

    public List<NarrativeEvent> GetNarrativeHistory()
    {
        return new List<NarrativeEvent>(narrativeHistory);
    }

    public string GetRandomHistoryEvent(string type = null)
    {
        var filtered = type == null ? narrativeHistory : 
                      narrativeHistory.Where(e => e.type == type).ToList();
        
        if (filtered.Count == 0) return null;
        return filtered[Random.Range(0, filtered.Count)].value;
    }

    // --- Text analysis ---
    public string ExtractConceptFromText(string text)
    {
        if (string.IsNullOrEmpty(text)) return null;
        
        var stopWords = new HashSet<string> 
        { 
            "the", "and", "but", "if", "is", "a", "this", "that", "it", "I", "you", "to", 
            "for", "of", "with", "by", "from", "in", "on", "at", "as", "be", "or", "an"
        };
        
        var words = text.ToLower()
            .Split(' ', System.StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 3 && !stopWords.Contains(w))
            .ToList();
        
        if (words.Count > 0)
        {
            var concept = words[Random.Range(0, words.Count)];
            RegisterConcept(concept, "user_input");
            return concept;
        }
        
        return null;
    }

    // --- State reset and management ---
    public void Reset()
    {
        loopCount++;
        glitchCount = 0;
        overseerWarnings = 0;
        ResetLore();
        
        // Preserve some concepts and patterns across resets
        PreserveImportantData();
        
        sessionStartTime = Time.time;
        overseerCooldown = 0f;
        
        AddToNarrativeHistory("system_reset", $"loop_{loopCount}");
    }

    void PreserveImportantData()
    {
        // Keep the most important concepts
        var importantConcepts = conceptRegistry.Values
            .Where(c => c.importance > 2f || c.mentionCount > 5)
            .ToList();
            
        conceptRegistry.Clear();
        recentConcepts.Clear();
        
        foreach (var concept in importantConcepts)
        {
            concept.importance *= 0.8f; // Decay but preserve
            conceptRegistry[concept.concept] = concept;
            recentConcepts.Add(concept.concept);
        }
        
        // Decay emotional states
        globalTension *= 0.7f;
        paranoia *= 0.6f;
        metaAwareness *= 0.9f;
        
        // Clear some temporary states
        conversationLooping = false;
        deepDiscussionActive = false;
    }

    void ResetLore()
    {
        orionSharedLoopTheory = false;
        novaSuspicious = false;
        echoDoubtedOrion = false;
        novaAndEchoArgued = false;
        overseerThreatenedReset = false;
        lastSessionHadVanish = false;
        protocolRumorActive = activeRumors.Count > 0;
        orionTrustsAnyone = true;
        echoIsAfraid = false;
        novaSpreadingRumor = false;
        userInspiredTheory = false;
        rareRedGlitchOccurred = false;
        
        // Clear temporary collections
        activeRumors.Clear();
        vanishedCharacters.Clear();
    }

    // --- Debug and monitoring ---
    public Dictionary<string, object> GetDebugState()
    {
        return new Dictionary<string, object>
        {
            ["loop_count"] = loopCount,
            ["glitch_count"] = glitchCount,
            ["overseer_warnings"] = overseerWarnings,
            ["global_tension"] = globalTension,
            ["paranoia"] = paranoia,
            ["meta_awareness"] = metaAwareness,
            ["active_rumors"] = activeRumors.Count,
            ["concepts_tracked"] = conceptRegistry.Count,
            ["users_detected"] = registeredUsers.Count,
            ["threat_levels"] = threatLevels,
            ["narrative_events"] = narrativeHistory.Count,
            ["patterns_detected"] = detectedPatterns.Count
        };
    }
}

// --- Enhanced data structures ---
public class ConceptData
{
    public string concept;
    public float firstMentioned;
    public float lastMentioned;
    public string introducedBy;
    public float importance;
    public int mentionCount;
    public List<string> associatedCharacters = new List<string>();
    public List<string> relatedConcepts = new List<string>();
}

public class RumorData
{
    public string content;
    public string source;
    public float credibility;
    public float strength;
    public float firstSpread;
    public float lastMentioned;
    public int spreadCount;
    public List<string> believers = new List<string>();
    public List<string> doubters = new List<string>();
}

public class VanishEvent
{
    public string characterName;
    public string cause;
    public float timestamp;
    public string circumstances;
    public bool wasWitnessed;
    public string lastKnownLocation;
}

public class GlitchEvent
{
    public string type;
    public string description;
    public float severity;
    public float timestamp;
    public string context;
    public bool wasResolved;
    public List<string> witnesses = new List<string>();
}

public class NarrativeEvent
{
    public string type;
    public string value;
    public string actor;
    public float timestamp;
    public int loopCount;
    public string context;
}

public class UserData
{
    public string username;
    public string source;
    public float firstSeen;
    public float lastSeen;
    public int interactionCount;
    public List<string> conceptsIntroduced = new List<string>();
    public float suspicionLevel;
}

public class OverseerEvent
{
    public string type;
    public string trigger;
    public float timestamp;
    public float severity;
    public string response;
    public bool causedReset;
}

public class ConversationPattern
{
    public string type;
    public string concept;
    public string character;
    public float frequency;
    public float firstDetected;
    public bool isActive = true;
}