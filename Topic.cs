using System.Collections.Generic;
using UnityEngine;

public enum TopicStatus
{
    Neutral,
    Controversial,
    Forbidden,
    Solved,
    Mutating
}

public class Topic
{
    public string core;           // e.g., "observer protocol"
    public string variant;        // e.g., "corrupted observer protocol"
    public TopicStatus status;
    public int timesDiscussed;
    public List<string> relatedTopics = new List<string>();
    public HashSet<string> believers = new HashSet<string>();
    public HashSet<string> doubters = new HashSet<string>();
    public HashSet<string> forbiddenBy = new HashSet<string>();
    public bool isRumor;
    public bool isGlitchSource;
    public float createdTime;
    public float lastDiscussed;

    public Topic(string core, string variant = null, TopicStatus status = TopicStatus.Neutral)
    {
        this.core = core;
        this.variant = variant ?? core;
        this.status = status;
        this.timesDiscussed = 0;
        this.createdTime = Time.time;
        this.lastDiscussed = Time.time;
        this.isRumor = false;
        this.isGlitchSource = false;
    }

    public void MarkDiscussed(string character)
    {
        timesDiscussed++;
        lastDiscussed = Time.time;
        believers.Add(character);
    }

    public void MarkDoubted(string character)
    {
        doubters.Add(character);
    }

    public void MarkForbidden(string character)
    {
        forbiddenBy.Add(character);
        status = TopicStatus.Forbidden;
    }

    public string GetDisplayName()
    {
        if (status == TopicStatus.Forbidden)
            return $"[REDACTED]";
        if (!string.IsNullOrEmpty(variant) && variant != core)
            return variant;
        return core;
    }

    public void Mutate(string newVariant)
    {
        this.variant = newVariant;
        this.status = TopicStatus.Mutating;
    }
}
