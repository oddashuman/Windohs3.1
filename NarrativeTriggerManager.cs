using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// **New Script: NarrativeTriggerManager**
/// Vision: The central hub for cause-and-effect storytelling. This manager
/// listens for events from all other systems (viewer commands, high tension,
/// specific dialogue) and triggers appropriate consequences, such as glitches,
/// environmental changes, or new AI behaviors, making the narrative feel emergent.
/// </summary>
public class NarrativeTriggerManager : MonoBehaviour
{
    public static NarrativeTriggerManager Instance { get; private set; }

    private Dictionary<string, System.Action<string>> eventRegistry;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        InitializeEventRegistry();
    }

    private void InitializeEventRegistry()
    {
        eventRegistry = new Dictionary<string, System.Action<string>>
        {
            // Events triggered by viewers
            ["ViewerGlitchRequest"] = (source) => GlitchManager.Instance?.TriggerRandomGlitch(),
            ["ViewerTensionUp"] = (source) => DialogueState.Instance.globalTension += 0.2f,
            ["ViewerObserve"] = (source) => {
                DialogueState.Instance.observerDetected = true;
                EnvironmentManager.Instance.UpdateWallpaperBasedOnMood(); // Force an environment check
            },

            // Events triggered by the simulation state
            ["HighTension"] = (source) => GlitchManager.Instance?.TriggerRandomGlitch(),
            ["HighAwareness"] = (source) => EnvironmentManager.Instance.UpdateWallpaperBasedOnMood(),
        };
    }
    
    /// <summary>
    /// Public entry point for any system to trigger a narrative event.
    /// </summary>
    public void TriggerEvent(string eventName, string sourceInfo)
    {
        Debug.Log($"NARRATIVE_TRIGGER: Event '{eventName}' triggered by '{sourceInfo}'.");
        if (eventRegistry.ContainsKey(eventName))
        {
            eventRegistry[eventName].Invoke(sourceInfo);
        }
    }

    void Update()
    {
        // Continuously check the simulation state for potential triggers
        if (Time.frameCount % 150 == 0) // Check every ~2.5 seconds
        {
            CheckSimulationStateTriggers();
        }
    }

    private void CheckSimulationStateTriggers()
    {
        var state = DialogueState.Instance;
        if (state.globalTension > 0.8f)
        {
            TriggerEvent("HighTension", "GlobalTensionCheck");
        }
        if (state.metaAwareness > 0.7f)
        {
            TriggerEvent("HighAwareness", "MetaAwarenessCheck");
        }
    }
}