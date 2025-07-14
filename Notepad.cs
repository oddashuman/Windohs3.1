using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// **New Script: Notepad**
/// Vision: Implements the Notepad program for Orion to document his findings.
/// Features a character-by-character typing simulation to create the illusion
/// that Orion is actively writing and editing his thoughts in real-time.
/// </summary>
public class Notepad : MonoBehaviour
{
    public Window parentWindow;
    private TextMeshProUGUI notepadText;
    private Coroutine typingCoroutine;

    public void Initialize()
    {
        // Create the UI
        var textGO = new GameObject("NotepadTextArea");
        textGO.transform.SetParent(transform, false);
        notepadText = textGO.AddComponent<TextMeshProUGUI>();
        notepadText.font = Windows31DesktopManager.Instance.windows31Font;
        notepadText.fontSize = 12;
        notepadText.color = Color.black;
        notepadText.alignment = TextAlignmentOptions.TopLeft;
        
        var rt = textGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(5, 5);
        rt.offsetMax = new Vector2(-5, -5);
    }

    /// <summary>
    /// Starts the process of "typing" text into the notepad.
    /// </summary>
    public void TypeText(string fullText)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(SimulateTyping(fullText));
    }

    private IEnumerator SimulateTyping(string textToType)
    {
        notepadText.text = "";
        foreach (char c in textToType)
        {
            notepadText.text += c;
            // Play typing sound for immersion
            if (c != ' ')
            {
                Windows31DesktopManager.Instance.PlaySound("type");
            }
            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f)); // Simulate human typing speed variance
        }
    }
}