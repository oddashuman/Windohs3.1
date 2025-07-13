using System.Collections.Generic;
using UnityEngine;

public class MemoryState : MonoBehaviour
{
    public static MemoryState Instance;

    // ====== LOOP CYCLE THREAD ======
    public int loopCycleCount = 0;
    public int orionLoopAwareness = 0;
    public float loopStartTime;

    // ====== OVERSEER THREAD ======
    public int overseerWarnings = 0;
    public bool overseerDirectPing = false;

    // ====== GENERAL LORE FLAGS ======
    public bool hasSeenGlitch = false;
    public bool mentionedTheLoop = false;
    public bool suspectsOverseer = false;
    public bool sawOtherSelf = false;
    public bool trustIsFading = false;
    public bool protocolLeaked = false;

    // ====== CONCEPT MEMORY ======
    public List<string> recentConcepts = new List<string>();
    private const int maxConcepts = 50;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        loopStartTime = Time.time;
    }

    // ====== CONCEPT TRACKING ======
    public void RememberConcept(string concept)
    {
        if (!recentConcepts.Contains(concept))
        {
            recentConcepts.Add(concept);
            if (recentConcepts.Count > maxConcepts)
                recentConcepts.RemoveAt(0);
        }
    }

    public bool HasConcept(string concept)
    {
        return recentConcepts.Contains(concept);
    }

    // ====== LOOP BEHAVIOR ======
    public void IncrementLoopSuspicion()
    {
        orionLoopAwareness++;
    }

    public float TimeSinceLoopStarted()
    {
        return Time.time - loopStartTime;
    }

    // ====== OVERSEER BEHAVIOR ======
    public void IncrementOverseerThreat()
    {
        overseerWarnings++;
        if (overseerWarnings > 4)
            overseerDirectPing = true;
    }

    // ====== FULL RESET ======
    public void ForgetEverything()
    {
        loopCycleCount = 0;
        orionLoopAwareness = 0;
        loopStartTime = Time.time;

        overseerWarnings = 0;
        overseerDirectPing = false;

        hasSeenGlitch = false;
        mentionedTheLoop = false;
        suspectsOverseer = false;
        sawOtherSelf = false;
        trustIsFading = false;
        protocolLeaked = false;

        recentConcepts.Clear();
    }
}