using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Production-grade terminal manager with realistic human typing simulation.
/// Features: Character-specific typing patterns, emotional state modifiers,
/// realistic error correction, and optimized text rendering.
/// </summary>
public class MatrixTerminalManager : MonoBehaviour
{
    public static MatrixTerminalManager Instance { get; private set; }

    [Header("UI Components")]
    private CanvasGroup terminalCanvasGroup;
    private TMP_Text terminalText;
    private ScrollRect scrollRect;
    private Image titleBar;
    private TMP_Text titleBarText;

    [Header("Typing Simulation - Advanced")]
    public string cursorSymbol = "_";
    [Range(0.1f, 2f)] public float blinkInterval = 0.5f;
    [Range(0.01f, 0.2f)] public float baseTypingSpeed = 0.08f;
    [Range(0f, 0.1f)] public float errorRate = 0.03f;
    [Range(0f, 1f)] public float backspaceChance = 0.7f;
    
    [Header("Performance Settings")]
    public bool enableRetroUI = true;
    public bool enableCRTEffects = true;
    [Range(100, 2000)] public int maxTextLength = 1000;
    public bool enableTextOptimization = true;

    // Advanced typing system
    private struct TypingState
    {
        public bool isTyping;
        public bool isDeleting;
        public float currentSpeed;
        public int charactersTyped;
        public float fatigueLevel;
        public string pendingText;
        public int deleteCount;
    }
    
    private TypingState typingState;
    private StringBuilder displayBuffer = new StringBuilder(2048);
    private Queue<char> typingQueue = new Queue<char>();
    
    // Character-specific behaviors
    private Dictionary<string, CharacterTypingProfile> typingProfiles;
    private CharacterTypingProfile currentProfile;
    
    // Performance optimization
    private Coroutine typingCoroutine;
    private Coroutine blinkCoroutine;
    private DialogueEngine dialogueEngine;
    private bool isInitialized = false;
    private float lastTextUpdate;
    private bool needsScrollUpdate;
    
    // Memory management
    private readonly StringBuilder tempBuilder = new StringBuilder(256);
    private float lastGCTime;
    private const float GC_INTERVAL = 45f;

    #region Character Typing Profiles

    [System.Serializable]
    public class CharacterTypingProfile
    {
        public string characterName;
        public float baseSpeed = 1f;           // Typing speed multiplier
        public float consistency = 0.8f;       // How consistent the timing is
        public float errorFrequency = 0.03f;   // How often they make mistakes
        public float correctionSpeed = 1.5f;   // How fast they correct errors
        public float hesitationChance = 0.1f;  // Probability of pausing
        public float fatigueRate = 0.001f;     // How quickly they get tired
        public bool deletesAndRetypes = false; // Tendency to delete and retype
        public string[] commonErrors;          // Common typing mistakes
        public string[] hesitationTriggers;    // Words that cause hesitation
        
        public CharacterTypingProfile(string name)
        {
            characterName = name;
            SetDefaultsForCharacter(name);
        }
        
        private void SetDefaultsForCharacter(string name)
        {
            switch (name.ToLower())
            {
                case "orion":
                    baseSpeed = 1.1f;
                    consistency = 0.85f;
                    errorFrequency = 0.02f;
                    correctionSpeed = 1.2f;
                    hesitationChance = 0.15f;
                    deletesAndRetypes = true;
                    hesitationTriggers = new string[] { "overseer", "watching", "simulation", "real" };
                    commonErrors = new string[] { "teh", "hte", "adn", "nad" };
                    break;
                    
                case "nova":
                    baseSpeed = 1.4f;
                    consistency = 0.9f;
                    errorFrequency = 0.05f;
                    correctionSpeed = 2.0f;
                    hesitationChance = 0.05f;
                    deletesAndRetypes = false;
                    hesitationTriggers = new string[] { "trust" };
                    commonErrors = new string[] { "thr", "nad", "waht" };
                    break;
                    
                case "echo":
                    baseSpeed = 0.7f;
                    consistency = 0.6f;
                    errorFrequency = 0.08f;
                    correctionSpeed = 0.8f;
                    hesitationChance = 0.35f;
                    deletesAndRetypes = true;
                    hesitationTriggers = new string[] { "fear", "danger", "watched", "monitored", "escape" };
                    commonErrors = new string[] { "teh", "adn", "jsut", "hten", "waht" };
                    break;
                    
                case "lumen":
                    baseSpeed = 0.9f;
                    consistency = 0.7f;
                    errorFrequency = 0.03f;
                    correctionSpeed = 1.0f;
                    hesitationChance = 0.25f;
                    deletesAndRetypes = false;
                    hesitationTriggers = new string[] { "reality", "existence", "consciousness" };
                    commonErrors = new string[] { "teh", "ot", "fo" };
                    break;
                    
                default:
                    // Generic profile
                    baseSpeed = 1f;
                    consistency = 0.8f;
                    errorFrequency = 0.04f;
                    correctionSpeed = 1.3f;
                    hesitationChance = 0.12f;
                    deletesAndRetypes = false;
                    hesitationTriggers = new string[] { };
                    commonErrors = new string[] { "teh", "adn" };
                    break;
            }
        }
    }

    #endregion

    #region Initialization

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(InitializeAndLink());
    }

    private IEnumerator InitializeAndLink()
    {
        InitializeTypingProfiles();
        
        yield return new WaitUntil(() => DialogueEngine.Instance != null && DialogueEngine.Instance.IsReady());
        
        dialogueEngine = DialogueEngine.Instance;
        isInitialized = true;
        
        StartCursorBlink();
        Debug.Log("TERMINAL: Enhanced typing system ready");
    }

    private void InitializeTypingProfiles()
    {
        typingProfiles = new Dictionary<string, CharacterTypingProfile>();
        
        // Create profiles for all known characters
        string[] characters = { "Orion", "Nova", "Echo", "Lumen", "Overseer" };
        foreach (string character in characters)
        {
            typingProfiles[character] = new CharacterTypingProfile(character);
        }
        
        // Set default profile
        currentProfile = typingProfiles["Orion"];
    }

    public void InitializeUI(CanvasGroup canvasGroup, TMP_Text textComponent, ScrollRect scrollComponent, Image bar, TMP_Text barText)
    {
        terminalCanvasGroup = canvasGroup;
        terminalText = textComponent;
        scrollRect = scrollComponent;
        titleBar = bar;
        titleBarText = barText;
        
        // Optimize text component for performance
        if (enableTextOptimization && terminalText != null)
        {
            terminalText.overflowMode = TextOverflowModes.Truncate;
            terminalText.enableWordWrapping = true;
            terminalText.richText = true;
        }
        
        DisableTerminal();
    }

    #endregion

    #region Core Typing Engine

    void Update()
    {
        if (!terminalCanvasGroup || !isInitialized || typingCoroutine != null) return;
        
        // Start typing next message if available
        if (!typingState.isTyping && dialogueEngine != null)
        {
            typingCoroutine = StartCoroutine(ProcessNextMessageAdvanced());
        }
        
        // Handle scroll updates
        if (needsScrollUpdate && scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
            needsScrollUpdate = false;
        }
        
        // Periodic memory cleanup
        OptimizedMemoryManagement();
    }

    private IEnumerator ProcessNextMessageAdvanced()
    {
        typingState.isTyping = true;
        
        // Get next message from dialogue engine
        DialogueMessage message = dialogueEngine.GetNextMessage();
        if (message == null || string.IsNullOrEmpty(message.text))
        {
            typingState.isTyping = false;
            typingCoroutine = null;
            yield break;
        }
        
        // Set character profile
        if (typingProfiles.ContainsKey(message.speaker))
        {
            currentProfile = typingProfiles[message.speaker];
        }
        
        // Add speaker prefix with color
        Color speakerColor = GetCharacterColor(message.speaker);
        string colorHex = ColorUtility.ToHtmlStringRGB(speakerColor);
        string prefix = $"\n<color=#{colorHex}>{message.speaker}:</color> ";
        
        displayBuffer.Append(prefix);
        UpdateTerminalDisplay();
        
        yield return new WaitForSeconds(Random.Range(0.5f, 1.2f)); // Pre-typing pause
        
        // Type the message with realistic behavior
        yield return StartCoroutine(TypeMessageRealistic(message.text));
        
        // Post-message pause
        yield return new WaitForSeconds(Random.Range(0.3f, 0.8f));
        
        // Report activity and reset state
        if (SimulationController.Instance != null)
            SimulationController.Instance.IncrementTerminalMessageCount();
            
        typingState.isTyping = false;
        typingCoroutine = null;
    }

    private IEnumerator TypeMessageRealistic(string messageText)
    {
        string processedText = PreprocessMessage(messageText);
        
        // Initialize typing state
        typingState.currentSpeed = baseTypingSpeed / currentProfile.baseSpeed;
        typingState.charactersTyped = 0;
        typingState.fatigueLevel = 0f;
        
        for (int i = 0; i < processedText.Length; i++)
        {
            char currentChar = processedText[i];
            
            // Check for hesitation triggers
            if (ShouldHesitate(processedText, i))
            {
                yield return StartCoroutine(HandleHesitation());
            }
            
            // Simulate typing error
            if (ShouldMakeError(currentChar))
            {
                yield return StartCoroutine(HandleTypingError(currentChar, processedText, i));
            }
            else
            {
                // Type character normally
                displayBuffer.Append(currentChar);
                UpdateTerminalDisplay();
                
                // Play appropriate sound
                PlayTypingSound(currentChar);
                
                // Calculate dynamic delay
                float delay = CalculateTypingDelay(currentChar);
                yield return new WaitForSeconds(delay);
            }
            
            typingState.charactersTyped++;
            UpdateFatigue();
            
            // Trim buffer if too long for performance
            if (enableTextOptimization && displayBuffer.Length > maxTextLength)
            {
                TrimDisplayBuffer();
            }
        }
        
        // Final formatting
        needsScrollUpdate = true;
    }

    #endregion

    #region Realistic Typing Behaviors

    private bool ShouldHesitate(string text, int position)
    {
        if (currentProfile.hesitationTriggers == null) return false;
        
        // Check if we're approaching a hesitation trigger word
        foreach (string trigger in currentProfile.hesitationTriggers)
        {
            int triggerStart = text.IndexOf(trigger, position, System.StringComparison.OrdinalIgnoreCase);
            if (triggerStart >= 0 && triggerStart <= position + 3)
            {
                return Random.value < currentProfile.hesitationChance * (1f + typingState.fatigueLevel);
            }
        }
        
        // Random hesitation
        return Random.value < currentProfile.hesitationChance * 0.3f;
    }

    private IEnumerator HandleHesitation()
    {
        float hesitationTime = Random.Range(0.3f, 1.5f) * (1f + typingState.fatigueLevel);
        
        // Sometimes show typing indicator during hesitation
        if (Random.value < 0.4f)
        {
            for (int i = 0; i < 3; i++)
            {
                displayBuffer.Append(".");
                UpdateTerminalDisplay();
                yield return new WaitForSeconds(hesitationTime / 6f);
                
                // Remove the dot
                if (displayBuffer.Length > 0)
                {
                    displayBuffer.Length--;
                    UpdateTerminalDisplay();
                }
                yield return new WaitForSeconds(hesitationTime / 6f);
            }
        }
        else
        {
            yield return new WaitForSeconds(hesitationTime);
        }
    }

    private bool ShouldMakeError(char character)
    {
        float errorChance = currentProfile.errorFrequency * (1f + typingState.fatigueLevel);
        
        // Increase error chance for difficult characters
        if (char.IsUpper(character) || char.IsPunctuation(character))
        {
            errorChance *= 1.5f;
        }
        
        return Random.value < errorChance;
    }

    private IEnumerator HandleTypingError(char correctChar, string fullText, int position)
    {
        // Generate error character
        char errorChar = GenerateErrorCharacter(correctChar);
        
        // Type the error
        displayBuffer.Append(errorChar);
        UpdateTerminalDisplay();
        PlayTypingSound(errorChar);
        
        float errorDelay = CalculateTypingDelay(errorChar);
        yield return new WaitForSeconds(errorDelay);
        
        // Decide whether to correct immediately or continue
        bool shouldCorrectImmediately = Random.value < backspaceChance;
        
        if (currentProfile.deletesAndRetypes && Random.value < 0.3f)
        {
            // Character deletes multiple characters and retypes
            yield return StartCoroutine(HandleBulkCorrection(fullText, position));
        }
        else if (shouldCorrectImmediately)
        {
            // Simple backspace correction
            yield return StartCoroutine(HandleSimpleCorrection(correctChar));
        }
        else
        {
            // Continue typing, might correct later
            typingState.pendingText += correctChar.ToString();
        }
    }

    private IEnumerator HandleSimpleCorrection(char correctChar)
    {
        // Pause before correction (realization delay)
        yield return new WaitForSeconds(Random.Range(0.1f, 0.4f));
        
        // Backspace
        if (displayBuffer.Length > 0)
        {
            displayBuffer.Length--;
            UpdateTerminalDisplay();
            PlayBackspaceSound();
            
            float backspaceDelay = CalculateTypingDelay(' ') * currentProfile.correctionSpeed;
            yield return new WaitForSeconds(backspaceDelay);
        }
        
        // Type correct character
        displayBuffer.Append(correctChar);
        UpdateTerminalDisplay();
        PlayTypingSound(correctChar);
        
        yield return new WaitForSeconds(CalculateTypingDelay(correctChar));
    }

    private IEnumerator HandleBulkCorrection(string fullText, int position)
    {
        // Pause for realization
        yield return new WaitForSeconds(Random.Range(0.2f, 0.8f));
        
        // Delete several characters
        int deleteCount = Random.Range(2, 6);
        for (int i = 0; i < deleteCount && displayBuffer.Length > 0; i++)
        {
            displayBuffer.Length--;
            UpdateTerminalDisplay();
            PlayBackspaceSound();
            
            float backspaceDelay = CalculateTypingDelay(' ') * currentProfile.correctionSpeed * 0.8f;
            yield return new WaitForSeconds(backspaceDelay);
        }
        
        // Retype the section correctly
        int retypeStart = Mathf.Max(0, position - deleteCount + 1);
        for (int i = retypeStart; i <= position && i < fullText.Length; i++)
        {
            displayBuffer.Append(fullText[i]);
            UpdateTerminalDisplay();
            PlayTypingSound(fullText[i]);
            
            yield return new WaitForSeconds(CalculateTypingDelay(fullText[i]));
        }
    }

    #endregion

    #region Typing Calculations

    private float CalculateTypingDelay(char character)
    {
        float baseDelay = typingState.currentSpeed;
        
        // Character-specific timing
        if (char.IsWhiteSpace(character))
        {
            baseDelay *= 0.5f; // Spaces are faster
        }
        else if (char.IsUpper(character))
        {
            baseDelay *= 1.3f; // Capitals take longer
        }
        else if (char.IsPunctuation(character))
        {
            baseDelay *= 1.2f; // Punctuation takes longer
        }
        
        // Apply consistency variation
        float variance = (1f - currentProfile.consistency) * 0.5f;
        float randomFactor = 1f + Random.Range(-variance, variance);
        
        // Apply fatigue
        float fatigueMultiplier = 1f + typingState.fatigueLevel * 0.3f;
        
        // Apply emotional state if available
        if (DialogueState.Instance != null)
        {
            float tension = DialogueState.Instance.globalTension;
            float tensionMultiplier = 1f + tension * 0.2f; // Tension makes typing slightly faster but less accurate
            randomFactor *= tensionMultiplier;
        }
        
        return baseDelay * randomFactor * fatigueMultiplier;
    }

    private char GenerateErrorCharacter(char correctChar)
    {
        // Use common typing errors specific to character
        if (currentProfile.commonErrors != null && Random.value < 0.4f)
        {
            string errorWord = currentProfile.commonErrors[Random.Range(0, currentProfile.commonErrors.Length)];
            if (errorWord.Length > 0)
            {
                return errorWord[Random.Range(0, errorWord.Length)];
            }
        }
        
        // Generate adjacent key error
        string qwertyLayout = "qwertyuiopasdfghjklzxcvbnm";
        int correctIndex = qwertyLayout.IndexOf(char.ToLower(correctChar));
        
        if (correctIndex >= 0)
        {
            // Generate adjacent key based on QWERTY layout
            List<char> adjacentKeys = new List<char>();
            
            // Add horizontally adjacent keys
            if (correctIndex > 0) adjacentKeys.Add(qwertyLayout[correctIndex - 1]);
            if (correctIndex < qwertyLayout.Length - 1) adjacentKeys.Add(qwertyLayout[correctIndex + 1]);
            
            if (adjacentKeys.Count > 0)
            {
                char errorChar = adjacentKeys[Random.Range(0, adjacentKeys.Count)];
                return char.IsUpper(correctChar) ? char.ToUpper(errorChar) : errorChar;
            }
        }
        
        // Fallback to random similar character
        char[] similarChars = { 'x', 'z', 'q', 'w' };
        return similarChars[Random.Range(0, similarChars.Length)];
    }

    private void UpdateFatigue()
    {
        typingState.fatigueLevel = Mathf.Min(1f, typingState.fatigueLevel + currentProfile.fatigueRate);
        
        // Gradually increase typing delay as fatigue builds
        typingState.currentSpeed = baseTypingSpeed / currentProfile.baseSpeed * (1f + typingState.fatigueLevel * 0.4f);
    }

    private string PreprocessMessage(string message)
    {
        // Add subtle character-specific text modifications
        switch (currentProfile.characterName.ToLower())
        {
            case "echo":
                // Echo sometimes adds ellipses for nervousness
                if (Random.value < 0.3f)
                {
                    message = message.Replace(".", "...");
                }
                break;
                
            case "lumen":
                // Lumen sometimes uses unconventional spacing for artistic effect
                if (Random.value < 0.2f)
                {
                    message = message.Replace(" ", "  ");
                }
                break;
        }
        
        return message;
    }

    #endregion

    #region Audio & Effects

    private void PlayTypingSound(char character)
    {
        if (Windows31DesktopManager.Instance != null)
        {
            // Vary typing sound based on character
            if (char.IsWhiteSpace(character))
            {
                // Space bar has different sound
                Windows31DesktopManager.Instance.PlaySound("spacebar");
            }
            else if (char.IsUpper(character) || char.IsPunctuation(character))
            {
                // Shifted characters sound different
                Windows31DesktopManager.Instance.PlaySound("type_shift");
            }
            else
            {
                Windows31DesktopManager.Instance.PlaySound("type");
            }
        }
    }

    private void PlayBackspaceSound()
    {
        if (Windows31DesktopManager.Instance != null)
        {
            Windows31DesktopManager.Instance.PlaySound("backspace");
        }
    }

    #endregion

    #region Cursor Management

    private void StartCursorBlink()
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(CursorBlinkLoop());
    }

    private IEnumerator CursorBlinkLoop()
    {
        bool showCursor = true;
        
        while (true)
        {
            yield return new WaitForSeconds(blinkInterval);
            
            if (terminalCanvasGroup != null && terminalCanvasGroup.alpha > 0)
            {
                showCursor = !showCursor;
                UpdateTerminalDisplay();
            }
        }
    }

    #endregion

    #region Display Management

    private void UpdateTerminalDisplay()
    {
        if (terminalText == null) return;
        
        tempBuilder.Clear();
        tempBuilder.Append(displayBuffer);
        
        // Add cursor if terminal is active and not typing
        if (terminalCanvasGroup != null && terminalCanvasGroup.alpha > 0)
        {
            if (!typingState.isTyping || Time.time % (blinkInterval * 2) < blinkInterval)
            {
                tempBuilder.Append(cursorSymbol);
            }
        }
        
        terminalText.text = tempBuilder.ToString();
        lastTextUpdate = Time.time;
    }

    private void TrimDisplayBuffer()
    {
        if (displayBuffer.Length <= maxTextLength) return;
        
        // Remove text from the beginning, keeping recent content
        int removeCount = displayBuffer.Length - maxTextLength + 200; // Remove extra for buffer
        
        // Try to remove at line boundaries for better readability
        string content = displayBuffer.ToString();
        int lineBreak = content.IndexOf('\n', removeCount);
        if (lineBreak > 0 && lineBreak < removeCount + 100)
        {
            removeCount = lineBreak + 1;
        }
        
        displayBuffer.Remove(0, removeCount);
    }

    #endregion

    #region Terminal State Management

    public void EnableTerminal()
    {
        if (terminalCanvasGroup == null) return;
        
        terminalCanvasGroup.alpha = 1f;
        terminalCanvasGroup.interactable = true;
        terminalCanvasGroup.blocksRaycasts = true;
        
        // Reset typing state
        typingState = new TypingState();
        
        // Clear display and show connection message
        displayBuffer.Clear();
        displayBuffer.Append(">>> NEURAL LINK ESTABLISHED...");
        displayBuffer.Append("\n>>> Initializing secure channel...");
        displayBuffer.Append("\n>>> Connection verified. Ready for communication.");
        
        UpdateTerminalDisplay();
        needsScrollUpdate = true;
        
        Debug.Log("TERMINAL: Enhanced terminal enabled with realistic typing");
    }

    public void DisableTerminal()
    {
        if (terminalCanvasGroup == null) return;
        
        terminalCanvasGroup.alpha = 0f;
        terminalCanvasGroup.interactable = false;
        terminalCanvasGroup.blocksRaycasts = false;
        
        // Stop all typing operations
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        
        typingState.isTyping = false;
    }

    public void SetCrisisMode(bool isCrisis)
    {
        if (titleBar != null)
        {
            titleBar.color = isCrisis ? Color.red : new Color32(0, 0, 128, 255);
        }
        
        if (titleBarText != null)
        {
            titleBarText.text = isCrisis ? "TERMINAL - !! SYSTEM ALERT !!" : "Terminal";
        }
        
        // Modify typing behavior during crisis
        if (isCrisis)
        {
            // Increase error rates and hesitation for all characters
            foreach (var profile in typingProfiles.Values)
            {
                profile.errorFrequency *= 2f;
                profile.hesitationChance *= 1.5f;
            }
        }
        else
        {
            // Reset profiles to normal
            InitializeTypingProfiles();
        }
    }

    #endregion

    #region Performance & Memory Management

    private void OptimizedMemoryManagement()
    {
        // Periodic garbage collection during safe periods
        if (Time.time - lastGCTime > GC_INTERVAL && !typingState.isTyping)
        {
            // Clear typing queue if it gets too large
            if (typingQueue.Count > 100)
            {
                typingQueue.Clear();
            }
            
            // Trim string builders
            if (displayBuffer.Capacity > 4096)
            {
                var content = displayBuffer.ToString();
                displayBuffer.Clear();
                displayBuffer.Capacity = 2048;
                displayBuffer.Append(content);
            }
            
            if (tempBuilder.Capacity > 512)
            {
                tempBuilder.Clear();
                tempBuilder.Capacity = 256;
            }
            
            // Force garbage collection during idle
            System.GC.Collect();
            lastGCTime = Time.time;
        }
    }

    #endregion

    #region Utility Methods

    private Color GetCharacterColor(string characterName)
    {
        switch (characterName.ToLower())
        {
            case "orion": return new Color(0.4f, 0.8f, 1f); // Light blue
            case "nova": return new Color(1f, 0.9f, 0.3f);  // Yellow
            case "echo": return new Color(1f, 0.4f, 0.8f);  // Pink
            case "lumen": return new Color(0.5f, 1f, 0.5f); // Light green
            case "overseer": return new Color(1f, 0.3f, 0.3f); // Red
            default: return Color.white;
        }
    }

    public bool IsReady() => isInitialized;

    public string GetDebugInfo()
    {
        return $"Terminal Active: {terminalCanvasGroup?.alpha > 0}, " +
               $"Typing: {typingState.isTyping}, " +
               $"Characters Typed: {typingState.charactersTyped}, " +
               $"Fatigue: {typingState.fatigueLevel:F2}, " +
               $"Buffer Length: {displayBuffer.Length}, " +
               $"Current Profile: {currentProfile?.characterName ?? "None"}";
    }

    #endregion

    #region Cleanup

    void OnDestroy()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
            
        StopAllCoroutines();
        
        // Clear collections
        typingQueue?.Clear();
        displayBuffer?.Clear();
        tempBuilder?.Clear();
        typingProfiles?.Clear();
    }

    #endregion
}