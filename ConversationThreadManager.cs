using System.Collections.Generic;
using UnityEngine;

public class ConversationThreadManager : MonoBehaviour
{
    public static ConversationThreadManager Instance;

    private Dictionary<string, ConversationThread> activeThreads = new Dictionary<string, ConversationThread>();
    private int threadCounter = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;
    }

    public ConversationThread StartThread(List<CharacterProfile> participants, Topic topic)
    {
        string id = $"thread_{threadCounter++}";
        var thread = new ConversationThread(id, participants, topic);
        activeThreads[id] = thread;
        return thread;
    }

    public void CloseThread(string id)
    {
        if (activeThreads.ContainsKey(id))
            activeThreads[id].status = ThreadStatus.Closed;
    }

    public void PruneStaleThreads(float maxIdleTime = 120f)
    {
        var toClose = new List<string>();
        foreach (var kv in activeThreads)
        {
            if (kv.Value.status == ThreadStatus.Active && Time.time - kv.Value.lastActivity > maxIdleTime)
                toClose.Add(kv.Key);
        }
        foreach (var id in toClose)
            CloseThread(id);
    }

    public ConversationThread GetActiveThread()
    {
        // Prioritize escalating/active threads
        foreach (var thread in activeThreads.Values)
            if (thread.status == ThreadStatus.Active || thread.status == ThreadStatus.Escalating)
                return thread;
        return null;
    }

    public List<ConversationThread> GetAllActiveThreads()
    {
        var list = new List<ConversationThread>();
        foreach (var t in activeThreads.Values)
            if (t.status == ThreadStatus.Active)
                list.Add(t);
        return list;
    }
}
