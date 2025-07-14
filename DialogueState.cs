using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// **Complete Code: DialogueState**
/// Vision: The central nervous system of the simulation's narrative. This script tracks
/// loop counts, glitches, character relationships, rumors, and overall meta-awareness,
/// providing a persistent memory that influences all character and system behavior.
/// </summary>
public class DialogueState : MonoBehaviour
{
    public static DialogueState Instance { get; private set; }

    [Header("Core Simulation State")]
    public int loopCount = 1;
    public int glitchCount = 0;
    public int overseerWarnings = 0;

    [Header("Lore & Narrative Flags")]
    public bool charactersSuspectSimulation = false;
    public bool rareRedGlitchOccurred = false;
    public bool observerDetected = false;
    public bool systemIntegrityCompromised = false;

    [Header("Emotional State")]
    public float globalTension = 0f;
    public float paranoia = 0f;
    public float metaAwareness = 0f;

    // Advanced State Tracking
    private List<NarrativeEvent> narrativeHistory = new List<NarrativeEvent>();
    private const int maxNarrativeHistory = 50;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Adds a significant event to the simulation's history.
    /// </summary>
    public void AddToNarrativeHistory(string eventType, string value, string actor = "SYSTEM")
    {
        var narrativeEvent = new NarrativeEvent
        {
            type = eventType,
            value = value,
            actor = actor,
            timestamp = Time.time,
            loopCount = this.loopCount
        };
        
        narrativeHistory.Add(narrativeEvent);
        if (narrativeHistory.Count > maxNarrativeHistory)
        {
            narrativeHistory.RemoveAt(0);
        }
    }

    /// <summary>
    /// Records a glitch event and updates relevant state flags.
    /// </summary>
    public void AddGlitchEvent(string glitchType, string description, float severity)
    {
        glitchCount++;
        AddToNarrativeHistory("Glitch", $"{glitchType}: {description}");

        if (severity > 2.0f)
        {
            globalTension += 0.1f;
        }
        if (glitchType.ToLower().Contains("red"))
        {
            rareRedGlitchOccurred = true;
        }
    }

    /// <summary>
    /// Resets the state for a new loop cycle, preserving some meta-awareness.
    /// </summary>
    public void Reset()
    {
        loopCount++;
        AddToNarrativeHistory("System Reset", $"Beginning Loop {loopCount}");

        // Reset volatile states
        glitchCount = 0;
        overseerWarnings = 0;
        rareRedGlitchOccurred = false;
        systemIntegrityCompromised = false;

        // Decay emotional states, but don't erase them completely
        globalTension *= 0.5f;
        paranoia *= 0.6f;
        metaAwareness *= 0.9f; // Characters retain a sense of deja vu
    }

    public List<NarrativeEvent> GetNarrativeHistory() => narrativeHistory;

    public struct NarrativeEvent
    {
        public string type;
        public string value;
        public string actor;
        public float timestamp;
        public int loopCount;
    }
}