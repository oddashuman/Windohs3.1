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
    private const int maxNarrativeHistory = 100;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
        AddToNarrativeHistory("Glitch", $"{glitchType}: {description}", "GLITCH_MANAGER");

        globalTension = Mathf.Clamp01(globalTension + (severity * 0.05f));

        if (glitchType.ToLower().Contains("red") || severity > 2.5f)
        {
            rareRedGlitchOccurred = true;
            paranoia = Mathf.Clamp01(paranoia + 0.1f);
        }
    }

    /// <summary>
    /// Resets the state for a new loop cycle, preserving some meta-awareness.
    /// </summary>
    public void Reset()
    {
        AddToNarrativeHistory("System Reset", $"Ending Loop {loopCount}");
        loopCount++;
        AddToNarrativeHistory("System Reset", $"Beginning Loop {loopCount}");

        // Reset volatile states
        glitchCount = 0;
        overseerWarnings = 0;
        rareRedGlitchOccurred = false;
        systemIntegrityCompromised = false;
        observerDetected = false;

        // Decay emotional states, but don't erase them completely
        globalTension *= 0.3f; // Tension resets more significantly
        paranoia *= 0.5f;
        metaAwareness = Mathf.Clamp01(metaAwareness * 0.9f + 0.05f); // Retain a sense of deja vu and slightly increase it
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