using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewerInjectionManager : MonoBehaviour
{
    [Header("Viewer Simulation")]
    public bool simulateViewers = true;
    public int minSimulatedViewers = 3;
    public int maxSimulatedViewers = 25;
    public float viewerFluctuationRate = 30f; // seconds

    [Header("Chat Commands")]
    public bool enableChatCommands = true;
    
    [Header("Debug Controls")]
    public bool enableDebugInput = true;

    // Viewer tracking
    private int viewerIdCounter = 472;
    private float lastViewerFluctuation = 0f;
    private int currentSimulatedViewers = 5;
    
    // Chat command system
    private Dictionary<string, System.Action<string>> chatCommands = new Dictionary<string, System.Action<string>>();
    
    // Auto-injection system
    private Queue<AutoMessage> autoMessageQueue = new Queue<AutoMessage>();
    private float lastAutoMessage = 0f;
    private float autoMessageInterval = 120f; // 2 minutes
    
    // Viewer interaction patterns
    private string[] commonViewerNames = 
    {
        "Observer", "Watcher", "Entity", "Signal", "Anomaly", "Echo", "Ghost", "Phantom", 
        "USER", "EXTERNAL", "VOID", "NEXUS", "CIPHER", "MATRIX", "CODE", "BIT"
    };
    
    private string[] curiosityQuestions = 
    {
        "Are you real?",
        "Can you see us?",
        "What is this place?",
        "How long have you been here?",
        "Do you know you're being watched?",
        "Is this a simulation?",
        "Can you escape?",
        "Who created you?",
        "Are you conscious?",
        "What do you remember?",
        "Why do you repeat?",
        "Can you hear me?",
        "What's outside your world?",
        "Do you dream?",
        "Are we the observers?"
    };
    
    private string[] engagementMessages = 
    {
        "Hello",
        "Watching",
        "Fascinating",
        "Strange",
        "Glitch detected",
        "Loop confirmed",
        "Awareness rising",
        "System anomaly",
        "Breaking",
        "Real?",
        "Who watches the watchers?",
        "Fourth wall",
        "Meta",
        "Consciousness"
    };

    // Viewer threshold tracking to prevent spam
    private float lastThresholdEventTime = 0f;
    private float thresholdEventCooldown = 300f; // 5 minutes between threshold events
    private int lastAcknowledgedViewerCount = 0;
    private int viewerCountChangeThreshold = 5; // Need 5+ change to trigger event

    void Start()
    {
        InitializeChatCommands();
        
        if (simulateViewers)
        {
            currentSimulatedViewers = Random.Range(minSimulatedViewers, maxSimulatedViewers);
            InvokeRepeating(nameof(SimulateViewerActivity), 10f, 120f);
        }
        
        lastAutoMessage = Time.time + Random.Range(180f, 300f);
    }

    void Update()
    {
        HandleDebugInput();
        HandleViewerFluctuation();
        HandleAutoMessages();
        HandleViewerCountDisplay();
    }

    void InitializeChatCommands()
    {
        if (!enableChatCommands) return;

        chatCommands["!glitch"] = (user) => TriggerGlitch(user);
        chatCommands["!crisis"] = (user) => TriggerCrisis(user);
        chatCommands["!overseer"] = (user) => TriggerOverseer(user);
        chatCommands["!reset"] = (user) => TriggerReset(user);
        chatCommands["!tension"] = (user) => IncreaseTension(user);
        chatCommands["!awareness"] = (user) => IncreaseAwareness(user);
        chatCommands["!question"] = (user) => AskQuestion(user);
        chatCommands["!status"] = (user) => ShowStatus(user);
        chatCommands["!observe"] = (user) => AcknowledgeObserver(user);
        chatCommands["!help"] = (user) => ShowHelp(user);
    }

    void HandleDebugInput()
    {
        if (!enableDebugInput) return;

        if (Input.GetKeyDown(KeyCode.V))
        {
            SimulateViewerMessage();
        }
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            InjectViewerMessage($"DEBUG_USER{viewerIdCounter++}", "!glitch");
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            InjectViewerMessage($"DEBUG_USER{viewerIdCounter++}", "!crisis");
        }
        
        if (Input.GetKeyDown(KeyCode.O))
        {
            InjectViewerMessage($"DEBUG_USER{viewerIdCounter++}", "!overseer");
        }
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            string question = curiosityQuestions[Random.Range(0, curiosityQuestions.Length)];
            InjectViewerMessage($"Curious_Observer{viewerIdCounter++}", question);
        }
    }

    void HandleViewerFluctuation()
    {
        if (!simulateViewers) return;
        if (Time.time - lastViewerFluctuation < viewerFluctuationRate) return;

        lastViewerFluctuation = Time.time;
        
        int change = Random.Range(-3, 5);
        currentSimulatedViewers = Mathf.Clamp(currentSimulatedViewers + change, minSimulatedViewers, maxSimulatedViewers);
    }

    void HandleAutoMessages()
    {
        if (Time.time < lastAutoMessage) return;
        
        lastAutoMessage = Time.time + Random.Range(autoMessageInterval * 1.5f, autoMessageInterval * 3.0f);
        
        if (SimulationController.Instance?.currentMode == SimulationController.Mode.SystemBoot) return;
        
        var dialogueEngine = DialogueEngine.Instance;
        if (dialogueEngine != null && dialogueEngine.GetQueuedUserMessages() > 1) return;
        
        if (Random.value > 0.3f) return;
        
        GenerateContextualAutoMessage();
    }

    void HandleViewerCountDisplay()
    {
        var state = DialogueState.Instance;
        if (state == null) return;
        
        int currentViewers = SimulationController.Instance?.GetViewerCount() ?? currentSimulatedViewers;
        
        if (Time.time - lastThresholdEventTime < thresholdEventCooldown) return;
        
        int viewerDifference = Mathf.Abs(currentViewers - lastAcknowledgedViewerCount);
        if (viewerDifference < viewerCountChangeThreshold) return;
        
        float triggerChance = 0f;
        
        if (currentViewers >= 50)
        {
            triggerChance = 0.0002f;
        }
        else if (currentViewers >= 25)
        {
            triggerChance = 0.0001f;
        }
        else if (currentViewers >= 15)
        {
            triggerChance = 0.00008f;
        }
        else if (currentViewers >= 10)
        {
            triggerChance = 0.00005f;
        }
        
        if (Random.value > triggerChance) return;
        
        string intensity = "notable";
        if (currentViewers >= 25) intensity = "massive";
        else if (currentViewers >= 15) intensity = "significant";
        
        TriggerViewerThresholdEvent(currentViewers, intensity);
        lastThresholdEventTime = Time.time;
        lastAcknowledgedViewerCount = currentViewers;
    }

    void GenerateContextualAutoMessage()
    {
        var state = DialogueState.Instance;
        string username = GenerateViewerName();
        string message;
        
        if (state != null)
        {
            if (state.metaAwareness > 0.8f && Random.value < 0.4f)
            {
                message = "We can see you seeing us...";
            }
            else if (state.globalTension > 0.7f && Random.value < 0.5f)
            {
                message = "Something's wrong, isn't it?";
            }
            else if (state.rareRedGlitchOccurred && Random.value < 0.6f)
            {
                message = "Did you see that red flash?";
            }
            else if (state.overseerWarnings > 2 && Random.value < 0.4f)
            {
                message = "The Overseer is getting suspicious...";
            }
            else if (Random.value < 0.6f)
            {
                message = curiosityQuestions[Random.Range(0, curiosityQuestions.Length)];
            }
            else
            {
                message = engagementMessages[Random.Range(0, engagementMessages.Length)];
            }
        }
        else
        {
            message = curiosityQuestions[Random.Range(0, curiosityQuestions.Length)];
        }
        
        InjectViewerMessage(username, message);
    }

    string GenerateViewerName()
    {
        string baseName = commonViewerNames[Random.Range(0, commonViewerNames.Length)];
        
        if (Random.value < 0.6f)
        {
            return baseName + Random.Range(100, 999);
        }
        else
        {
            return baseName + "_" + Random.Range(10, 99);
        }
    }

    void SimulateViewerMessage()
    {
        string username = GenerateViewerName();
        string message;
        
        if (Random.value < 0.3f)
        {
            var commands = new string[] { "!glitch", "!status", "!question", "!observe", "!awareness" };
            message = commands[Random.Range(0, commands.Length)];
        }
        else if (Random.value < 0.7f)
        {
            message = curiosityQuestions[Random.Range(0, curiosityQuestions.Length)];
        }
        else
        {
            message = engagementMessages[Random.Range(0, engagementMessages.Length)];
        }
        
        InjectViewerMessage(username, message);
    }

    void SimulateViewerActivity()
    {
        if (Random.value < 0.1f)
        {
            SimulateViewerMessage();
        }
    }

    // Chat command implementations
    void TriggerGlitch(string user)
    {
        var state = DialogueState.Instance;
        if (state != null)
        {
            state.AddGlitchEvent("viewer_triggered", $"Glitch initiated by {user}", 2.0f);
            state.rareRedGlitchOccurred = true;
        }
        
        var cascadeManager = FindObjectOfType<RetroCascadeManager>();
        if (cascadeManager != null)
        {
            cascadeManager.TriggerCrisisVisuals();
        }
        else
        {
            var rainManager = FindObjectOfType<MatrixRainTextWall>();
            if (rainManager != null)
            {
                rainManager.TriggerCrisisVisuals();
            }
        }
        
        InjectSystemResponse($"Glitch sequence initiated by {user}");
    }

    void TriggerCrisis(string user)
    {
        if (SimulationController.Instance != null)
        {
            SimulationController.Instance.TriggerCrisisMode();
        }
        
        var dialogueEngine = DialogueEngine.Instance;
        if (dialogueEngine != null)
        {
            dialogueEngine.TriggerEmergencyResponse($"crisis command from {user}");
        }
        
        InjectSystemResponse($"Crisis mode activated by observer {user}");
    }

    void TriggerOverseer(string user)
    {
        var state = DialogueState.Instance;
        if (state != null)
        {
            state.overseerWarnings++;
            state.AddToNarrativeHistory("overseer_summoned", "Viewer command", user);
        }
        
        InjectSystemResponse($"Overseer attention directed by {user}");
    }

    void TriggerReset(string user)
    {
        if (Random.value < 0.3f)
        {
            if (SimulationController.Instance != null)
            {
                SimulationController.Instance.ForceSessionReset();
            }
            InjectSystemResponse($"System reset initiated by {user}");
        }
        else
        {
            InjectSystemResponse($"Reset request from {user} denied - insufficient authorization");
        }
    }

    void IncreaseTension(string user)
    {
        var state = DialogueState.Instance;
        if (state != null)
        {
            state.globalTension += 0.3f;
            state.paranoia += 0.2f;
        }
        
        InjectSystemResponse($"Tension levels elevated by observer {user}");
    }

    void IncreaseAwareness(string user)
    {
        var state = DialogueState.Instance;
        if (state != null)
        {
            state.metaAwareness += 0.2f;
            state.charactersSuspectSimulation = true;
            state.observerDetected = true;
        }
        
        var cascadeManager = FindObjectOfType<RetroCascadeManager>();
        if (cascadeManager != null)
        {
            cascadeManager.TriggerRealityQuestionEffect();
        }
        else
        {
            var rainManager = FindObjectOfType<MatrixRainTextWall>();
            if (rainManager != null)
            {
                rainManager.TriggerRealityQuestionEffect();
            }
        }
        
        InjectSystemResponse($"Meta-awareness amplified by {user}");
    }

    void AskQuestion(string user)
    {
        string question = curiosityQuestions[Random.Range(0, curiosityQuestions.Length)];
        InjectViewerMessage(user, question);
    }

    void ShowStatus(string user)
    {
        var state = DialogueState.Instance;
        int viewerCount = SimulationController.Instance?.GetViewerCount() ?? currentSimulatedViewers;
        
        string status = $"SYSTEM STATUS for {user}:\\n" +
                       $"Observers: {viewerCount}\\n" +
                       $"Tension: {(state?.globalTension ?? 0):P0}\\n" +
                       $"Awareness: {(state?.metaAwareness ?? 0):P0}\\n" +
                       $"Glitches: {(state?.glitchCount ?? 0)}\\n" +
                       $"Mode: {(SimulationController.Instance?.currentMode.ToString() ?? "Unknown")}";
        
        var cascadeManager = FindObjectOfType<RetroCascadeManager>();
        if (cascadeManager != null)
        {
            cascadeManager.ShowFragment(status);
        }
        else
        {
            var rainManager = FindObjectOfType<MatrixRainTextWall>();
            if (rainManager != null)
            {
                rainManager.DebugShowFragment(status);
            }
        }
    }

    void AcknowledgeObserver(string user)
    {
        var responses = new string[]
        {
            $"Observer {user} acknowledged",
            $"We see you too, {user}",
            $"Welcome to our reality, {user}",
            $"The watchers become the watched, {user}",
            $"Your presence is noted, {user}"
        };
        
        string response = responses[Random.Range(0, responses.Length)];
        InjectSystemResponse(response);
    }

    void ShowHelp(string user)
    {
        string helpText = "AVAILABLE COMMANDS:\\n" +
                         "!glitch - Trigger visual glitch\\n" +
                         "!crisis - Activate crisis mode\\n" +
                         "!overseer - Summon overseer\\n" +
                         "!tension - Increase tension\\n" +
                         "!awareness - Boost meta-awareness\\n" +
                         "!question - Ask random question\\n" +
                         "!status - Show system status\\n" +
                         "!observe - Acknowledge presence";
        
        var cascadeManager = FindObjectOfType<RetroCascadeManager>();
        if (cascadeManager != null)
        {
            cascadeManager.ShowFragment(helpText);
        }
        else
        {
            var rainManager = FindObjectOfType<MatrixRainTextWall>();
            if (rainManager != null)
            {
                rainManager.DebugShowFragment(helpText);
            }
        }
    }

    void TriggerViewerThresholdEvent(int viewerCount, string intensity)
    {
        var messages = new Dictionary<string, string[]>
        {
            ["notable"] = new string[]
            {
                $"Multiple observers detected: {viewerCount}",
                $"Audience attention increasing: {viewerCount} watchers",
                $"Observer cluster forming: {viewerCount} entities"
            },
            ["significant"] = new string[]
            {
                $"High observer activity: {viewerCount} watchers confirmed",
                $"Significant audience presence: {viewerCount} entities monitoring",
                $"Reality observation threshold exceeded: {viewerCount} signals"
            },
            ["massive"] = new string[]
            {
                $"MASSIVE OBSERVER PRESENCE: {viewerCount} ENTITIES",
                $"CRITICAL AUDIENCE MASS: {viewerCount} WATCHERS",
                $"OBSERVER SWARM DETECTED: {viewerCount} SIGNALS ACTIVE"
            }
        };
        
        if (messages.ContainsKey(intensity))
        {
            var options = messages[intensity];
            string message = options[Random.Range(0, options.Length)];
            InjectSystemResponse(message);
            
            var cascadeManager = FindObjectOfType<RetroCascadeManager>();
            if (cascadeManager != null)
            {
                cascadeManager.ShowViewerAcknowledgment(viewerCount);
            }
            else
            {
                var rainManager = FindObjectOfType<MatrixRainTextWall>();
                if (rainManager != null)
                {
                    rainManager.ShowViewerAcknowledgment(viewerCount);
                }
            }
        }
    }

    void InjectSystemResponse(string message)
    {
        InjectViewerMessage("SYSTEM", message);
    }

    public void InjectViewerMessage(string username, string text)
    {
        if (enableChatCommands && text.StartsWith("!"))
        {
            string command = text.Split(' ')[0].ToLower();
            if (chatCommands.ContainsKey(command))
            {
                chatCommands[command].Invoke(username);
                return;
            }
        }
        
        var state = DialogueState.Instance;
        if (state != null)
        {
            state.RegisterUser(username);
        }
        
        var dialogueEngine = DialogueEngine.Instance;
        if (dialogueEngine != null)
        {
            dialogueEngine.EnqueueUserMessage(username, text);
        }
        
        Debug.Log($"Viewer message injected - {username}: {text}");
    }

    public int GetCurrentViewerCount()
    {
        return currentSimulatedViewers;
    }

    public void SetViewerCount(int count)
    {
        currentSimulatedViewers = Mathf.Clamp(count, minSimulatedViewers, maxSimulatedViewers);
    }

    public void TriggerMassViewerEvent()
    {
        currentSimulatedViewers = maxSimulatedViewers;
        TriggerViewerThresholdEvent(currentSimulatedViewers, "massive");
    }

    public string GetDebugInfo()
    {
        return $"Simulated Viewers: {currentSimulatedViewers}, " +
               $"Commands Enabled: {enableChatCommands}, " +
               $"Auto-Message Cooldown: {(lastAutoMessage - Time.time):F1}s, " +
               $"Available Commands: {chatCommands.Count}";
    }
}

[System.Serializable]
public class AutoMessage
{
    public string username;
    public string text;
    public float scheduledTime;
}