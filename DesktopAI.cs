using UnityEngine;
using System.Collections;

/// <summary>
/// **Updated: DesktopAI**
/// Vision: The AI is now a power user. It doesn't just launch programs; it manages
/// the workspace by moving, resizing, and minimizing windows. This creates a
/// much more realistic and dynamic simulation of a person actively using their computer.
/// </summary>
public class DesktopAI : MonoBehaviour
{
    public static DesktopAI Instance { get; private set; }

    // System References
    private CursorController cursorController;
    private Windows31DesktopManager desktopManager;
    private DialogueState dialogueState;

    public bool isBusy { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        cursorController = CursorController.Instance;
        desktopManager = Windows31DesktopManager.Instance;
        dialogueState = DialogueState.Instance;
    }

    public void PerformActivity(Windows31DesktopManager.DesktopActivity activity)
    {
        if (isBusy) return;
        StartCoroutine(ExecuteActivity(activity));
    }

    private IEnumerator ExecuteActivity(Windows31DesktopManager.DesktopActivity activity)
    {
        isBusy = true;
        Debug.Log($"DESKTOP_AI: Beginning activity: {activity}");

        switch (activity)
        {
            case Windows31DesktopManager.DesktopActivity.OrganizingFiles:
                yield return StartCoroutine(OrganizeFilesRoutine());
                break;
            case Windows31DesktopManager.DesktopActivity.ReviewingNotes:
                yield return StartCoroutine(ReviewNotesRoutine());
                break;
            case Windows31DesktopManager.DesktopActivity.Playing:
                yield return StartCoroutine(PlaySolitaireRoutine());
                break;
        }

        Debug.Log($"DESKTOP_AI: Completed activity: {activity}");
        isBusy = false;
    }

    private IEnumerator OrganizeFilesRoutine()
    {
        // 1. Open File Manager
        DesktopIcon fileManagerIcon = desktopManager.GetIcon(Windows31DesktopManager.ProgramType.FileManager);
        if (fileManagerIcon == null) { isBusy = false; yield break; }
        yield return cursorController.MoveToAndDoubleClick(fileManagerIcon.transform.position, "casual");
        yield return new WaitForSeconds(1.5f);

        // 2. Open Notepad to cross-reference
        DesktopIcon notesIcon = desktopManager.GetIcon(Windows31DesktopManager.ProgramType.Notepad);
        if (notesIcon == null) { isBusy = false; yield break; }
        yield return cursorController.MoveToAndDoubleClick(notesIcon.transform.position, "casual");
        yield return new WaitForSeconds(1.5f);

        // 3. **Window Management!** Move Notepad so both are visible.
        Window notesWindow = desktopManager.GetWindow("Notepad");
        if (notesWindow != null)
        {
            yield return cursorController.MoveAndDrag(notesWindow.titleBar.transform.position, new Vector2(300, 0));
        }
        
        // 4. "Work" for a bit
        yield return new WaitForSeconds(Random.Range(5f, 8f));

        // 5. Close windows
        Window fileManagerWindow = desktopManager.GetWindow("FileManager");
        if(fileManagerWindow != null) yield return cursorController.MoveToAndClick(fileManagerWindow.closeButton.transform.position, "casual");
        if(notesWindow != null) yield return cursorController.MoveToAndClick(notesWindow.closeButton.transform.position, "casual");
    }

    private IEnumerator ReviewNotesRoutine()
    {
        DesktopIcon notesIcon = desktopManager.GetIcon(Windows31DesktopManager.ProgramType.Notepad);
        if (notesIcon == null) { isBusy = false; yield break; }
        yield return cursorController.MoveToAndDoubleClick(notesIcon.transform.position, "cautious");
        
        // Read for a while, then minimize
        yield return new WaitForSeconds(Random.Range(6f, 10f));
        Window notesWindow = desktopManager.GetWindow("Notepad");
        if (notesWindow != null && notesWindow.minimizeButton != null)
        {
             yield return cursorController.MoveToAndClick(notesWindow.minimizeButton.transform.position, "casual");
        }
    }
    
    private IEnumerator PlaySolitaireRoutine()
    {
        DesktopIcon solitaireIcon = desktopManager.GetIcon(Windows31DesktopManager.ProgramType.Solitaire);
        if (solitaireIcon == null) { isBusy = false; yield break; }
        yield return cursorController.MoveToAndDoubleClick(solitaireIcon.transform.position, "casual");

        // Let the game play for a while as Orion "thinks"
        yield return new WaitForSeconds(Random.Range(15f, 25f));
        
        // Get bored and close it
        Window solitaireWindow = desktopManager.GetWindow("Solitaire");
        if (solitaireWindow != null)
        {
             yield return cursorController.MoveToAndClick(solitaireWindow.closeButton.transform.position, "casual");
        }
    }
}