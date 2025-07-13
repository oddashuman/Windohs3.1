using System.Collections.Generic;
using UnityEngine;

public class TopicManager : MonoBehaviour
{
    public static TopicManager Instance;

    // All session topics by ID (core string for now)
    public Dictionary<string, Topic> allTopics = new Dictionary<string, Topic>();
    private List<string> topicPool = new List<string>
    {
        "observer protocol", "loop theory", "signal leak", "rain cascade", "overseer warning",
        "fragmented memory", "echo chamber", "mirror test", "exit code", "delta protocol",
        "core corruption", "system reset", "protocol leak", "forbidden project", "sentient glitch",
        "prime anomaly", "rogue signal", "cascade failure", "identity fracture", "vanishing user"
    };

    // Topic mutation templates
    private string[] mutations = {
        "corrupted {0}", "forbidden {0}", "recursive {0}", "anomalous {0}",
        "latent {0}", "fragmented {0}", "encrypted {0}", "leaked {0}", "spreading {0}", "debunked {0}"
    };

    // Topic relationships (manually seeded and procedurally extended)
    private Dictionary<string, List<string>> relatedMap = new Dictionary<string, List<string>>();

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;

        // Seed initial topics
        foreach (string core in topicPool)
        {
            allTopics[core] = new Topic(core);
        }

        // Establish some base relationships (expand or mutate over time)
        AddRelated("observer protocol", "loop theory");
        AddRelated("loop theory", "system reset");
        AddRelated("signal leak", "protocol leak");
        AddRelated("rain cascade", "core corruption");
        AddRelated("mirror test", "identity fracture");
        AddRelated("sentient glitch", "overseer warning");
        AddRelated("vanishing user", "fragmented memory");
    }

    void AddRelated(string a, string b)
    {
        if (!relatedMap.ContainsKey(a)) relatedMap[a] = new List<string>();
        if (!relatedMap.ContainsKey(b)) relatedMap[b] = new List<string>();
        if (!relatedMap[a].Contains(b)) relatedMap[a].Add(b);
        if (!relatedMap[b].Contains(a)) relatedMap[b].Add(a);
    }

    // Get or create a topic by core name
    public Topic GetOrCreateTopic(string core)
    {
        if (!allTopics.ContainsKey(core))
            allTopics[core] = new Topic(core);
        return allTopics[core];
    }

    // Returns a random topic (for new threads or escalation)
    public Topic GetRandomTopic()
    {
        string core = topicPool[Random.Range(0, topicPool.Count)];
        return GetOrCreateTopic(core);
    }

    // Returns a random topic related to the input (or a random new one if none)
    public Topic GetRelatedTopic(Topic input)
    {
        if (input == null || !relatedMap.ContainsKey(input.core) || relatedMap[input.core].Count == 0)
            return GetRandomTopic();

        var options = relatedMap[input.core];
        string core = options[Random.Range(0, options.Count)];
        return GetOrCreateTopic(core);
    }

    // Mutate a topic (produces a new variant and optionally marks as rumor/controversial/forbidden)
    public Topic MutateTopic(Topic baseTopic)
    {
        if (baseTopic == null) return GetRandomTopic();
        // 30% chance to just escalate to a related topic
        if (Random.value < 0.3f)
            return GetRelatedTopic(baseTopic);

        // Otherwise mutate the topic (corrupted, forbidden, recursive, etc)
        string template = mutations[Random.Range(0, mutations.Length)];
        string newVariant = string.Format(template, baseTopic.core);

        // If already mutated, escalate controversy
        if (baseTopic.status == TopicStatus.Mutating)
        {
            baseTopic.status = TopicStatus.Controversial;
        }
        // 10% chance to become forbidden or rumor
        if (Random.value < 0.10f)
        {
            baseTopic.status = TopicStatus.Forbidden;
        }
        if (Random.value < 0.13f)
        {
            baseTopic.isRumor = true;
        }
        if (Random.value < 0.08f)
        {
            baseTopic.isGlitchSource = true;
        }

        // Actually mutate
        baseTopic.Mutate(newVariant);
        return baseTopic;
    }

    // Returns a controversial or forbidden topic for rare events
    public Topic GetControversialOrForbidden()
    {
        foreach (var topic in allTopics.Values)
        {
            if (topic.status == TopicStatus.Controversial || topic.status == TopicStatus.Forbidden)
                return topic;
        }
        // If none, return a mutated random topic
        return MutateTopic(GetRandomTopic());
    }

    // Session-level rumor propagation: mark a topic as rumor
    public void MarkRumor(string core, string byCharacter = null)
    {
        var t = GetOrCreateTopic(core);
        t.isRumor = true;
        if (byCharacter != null) t.believers.Add(byCharacter);
    }

    // Lore dump for debugging/testing
    public List<string> GetAllTopicsForDebug()
    {
        List<string> outList = new List<string>();
        foreach (var t in allTopics.Values)
            outList.Add($"{t.GetDisplayName()} | {t.status} | Rumor: {t.isRumor} | Discussed: {t.timesDiscussed}");
        return outList;
    }
}
