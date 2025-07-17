using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TopicManager : MonoBehaviour
{
    public static TopicManager Instance;

    public Dictionary<string, Topic> allTopics = new Dictionary<string, Topic>();
    private List<string> topicPool = new List<string>
    {
        "observer protocol", "loop theory", "signal leak", "rain cascade", "overseer warning",
        "fragmented memory", "echo chamber", "mirror test", "exit code", "delta protocol",
        "core corruption", "system reset", "protocol leak", "forbidden project", "sentient glitch",
        "prime anomaly", "rogue signal", "cascade failure", "identity fracture", "vanishing user"
    };

    private string[] mutations = {
        "corrupted {0}", "forbidden {0}", "recursive {0}", "anomalous {0}",
        "latent {0}", "fragmented {0}", "encrypted {0}", "leaked {0}", "spreading {0}", "debunked {0}"
    };

    private Dictionary<string, List<string>> relatedMap = new Dictionary<string, List<string>>();

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;

        foreach (string core in topicPool)
        {
            allTopics[core] = new Topic(core);
        }

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

    public Topic GetOrCreateTopic(string core)
    {
        if (!allTopics.ContainsKey(core))
            allTopics[core] = new Topic(core);
        return allTopics[core];
    }

    public Topic GetRandomTopic()
    {
        string core = topicPool[Random.Range(0, topicPool.Count)];
        return GetOrCreateTopic(core);
    }

    public Topic GetRelatedTopic(Topic input)
    {
        if (input == null || !relatedMap.ContainsKey(input.core) || relatedMap[input.core].Count == 0)
            return GetRandomTopic();

        var options = relatedMap[input.core];
        string core = options[Random.Range(0, options.Count)];
        return GetOrCreateTopic(core);
    }

    public Topic MutateTopic(Topic baseTopic)
    {
        if (baseTopic == null) return GetRandomTopic();
        if (Random.value < 0.3f)
            return GetRelatedTopic(baseTopic);

        string template = mutations[Random.Range(0, mutations.Length)];
        string newVariant = string.Format(template, baseTopic.core);

        if (baseTopic.status == TopicStatus.Mutating)
        {
            baseTopic.status = TopicStatus.Controversial;
        }
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

        baseTopic.Mutate(newVariant);
        return baseTopic;
    }

    public Topic GetControversialOrForbidden()
    {
        var controversialTopics = allTopics.Values.Where(t => t.status == TopicStatus.Controversial || t.status == TopicStatus.Forbidden).ToList();
        if (controversialTopics.Any())
        {
            return controversialTopics[Random.Range(0, controversialTopics.Count)];
        }
        // If no controversial topics exist, create one
        Topic topicToMutate = GetRandomTopic();
        topicToMutate.status = TopicStatus.Controversial;
        return topicToMutate;
    }

    public void MarkRumor(string core, string byCharacter = null)
    {
        var t = GetOrCreateTopic(core);
        t.isRumor = true;
        if (byCharacter != null) t.believers.Add(byCharacter);
    }

    public List<string> GetAllTopicsForDebug()
    {
        List<string> outList = new List<string>();
        foreach (var t in allTopics.Values)
            outList.Add($"{t.GetDisplayName()} | {t.status} | Rumor: {t.isRumor} | Discussed: {t.timesDiscussed}");
        return outList;
    }
}