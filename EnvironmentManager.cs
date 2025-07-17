using UnityEngine;
using UnityEngine.UI;
using System.Collections; // <-- FIX: Added this missing directive for IEnumerator

/// <summary>
/// **New Script: EnvironmentManager**
/// Vision: Controls the ambient storytelling elements of the desktop. It dynamically
/// changes the wallpaper based on the narrative's mood (calm, tense, paranoid) and
/// can introduce other subtle environmental changes, like clock anomalies, to
/// enhance the feeling of an unstable reality.
/// </summary>
public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance { get; private set; }

    [Header("Wallpapers")]
    public Sprite defaultWallpaper;
    public Sprite curiousWallpaper; // A blueprint or schematic
    public Sprite paranoidWallpaper; // A dark, glitchy, or oppressive image

    // References
    private Image desktopBackground;
    private DialogueState dialogueState;
    private CharacterProfile orionProfile;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Defer component linking until all singletons are guaranteed to be awake
        StartCoroutine(LinkComponents());
    }

    private IEnumerator LinkComponents()
    {
        // Wait until all required managers are initialized
        yield return new WaitUntil(() => DialogueState.Instance != null);
        yield return new WaitUntil(() => Windows31DesktopManager.Instance != null && Windows31DesktopManager.Instance.IsReady());
        yield return new WaitUntil(() => DialogueEngine.Instance != null && DialogueEngine.Instance.IsReady());

        dialogueState = DialogueState.Instance;
        desktopBackground = Windows31DesktopManager.Instance.GetDesktopBackground();
        
        // Safely get character profile
        if (DialogueEngine.Instance.allCharacters.ContainsKey("Orion"))
        {
            orionProfile = DialogueEngine.Instance.allCharacters["Orion"];
        }
        else
        {
            Debug.LogError("ENVIRONMENT_MANAGER: Orion character profile not found!");
            yield break; // Stop if Orion doesn't exist
        }

        SetWallpaper(defaultWallpaper);
    }


    void Update()
    {
        // Ensure we don't run Update logic until initialization is complete
        if (orionProfile == null) return;

        // Periodically check if the wallpaper should change based on mood.
        if (Time.frameCount % 300 == 0) // Check every 5 seconds
        {
            UpdateWallpaperBasedOnMood();
        }
    }

    /// <summary>
    /// Updates the desktop wallpaper based on Orion's current mood.
    /// Now public to be accessible by NarrativeTriggerManager.
    /// </summary>
    public void UpdateWallpaperBasedOnMood()
    {
        if (orionProfile == null || desktopBackground == null) return;

        orionProfile.UpdateMood();

        switch (orionProfile.mood)
        {
            case CharacterProfile.Mood.Curious:
            case CharacterProfile.Mood.Inspired:
                if (desktopBackground.sprite != curiousWallpaper) SetWallpaper(curiousWallpaper);
                break;

            case CharacterProfile.Mood.Paranoid:
            case CharacterProfile.Mood.Scared:
            case CharacterProfile.Mood.Frustrated:
                 if (desktopBackground.sprite != paranoidWallpaper) SetWallpaper(paranoidWallpaper);
                break;

            case CharacterProfile.Mood.Neutral:
            default:
                 if (desktopBackground.sprite != defaultWallpaper) SetWallpaper(defaultWallpaper);
                break;
        }
    }

    private void SetWallpaper(Sprite wallpaper)
    {
        if (wallpaper != null && desktopBackground != null)
        {
            Debug.Log($"ENV_MAN: Changing wallpaper to {wallpaper.name}");
            desktopBackground.sprite = wallpaper;
        }
    }
}