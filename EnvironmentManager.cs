using UnityEngine;
using UnityEngine.UI;

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
        dialogueState = DialogueState.Instance;
        desktopBackground = Windows31DesktopManager.Instance.GetDesktopBackground();
        orionProfile = DialogueEngine.Instance.allCharacters["Orion"];

        SetWallpaper(defaultWallpaper);
    }

    void Update()
    {
        // Periodically check if the wallpaper should change based on mood.
        if (Time.frameCount % 300 == 0) // Check every 5 seconds
        {
            UpdateWallpaperBasedOnMood();
        }
    }

    private void UpdateWallpaperBasedOnMood()
    {
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
        if (wallpaper != null)
        {
            Debug.Log($"ENV_MAN: Changing wallpaper to {wallpaper.name}");
            desktopBackground.sprite = wallpaper;
        }
    }
}