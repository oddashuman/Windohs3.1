using UnityEngine;
using System.Collections;

/// <summary>
/// **Updated: DesktopAI**
/// Vision: The AI is now a power user. It doesn't just launch programs; it manages
/// the workspace by moving, resizing, and minimizing windows. Its actions are
/// influenced by the narrative state (tension, awareness) to create a more
/// realistic and dynamic simulation of a person actively using their computer.
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
        // Link to singletons safely
        StartCoroutine(LinkComponents());
    }

    private IEnumerator LinkComponents()
    {
        yield return new WaitUntil(() => CursorController.Instance != null &&
                                       Windows31DesktopManager.Instance != null &&
                                       DialogueState.Instance != null);
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
        SimulationController.Instance.ReportActivity(); // Report activity at the start

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
        SimulationController.Instance.ReportActivity(); // Report activity at the end
    }

    private string GetMood()
    {
        if (dialogueState.globalTension > 0.7f) return "urgent";
        if (dialogueState.metaAwareness > 0.6f) return "cautious";
        return "casual";
    }

    private IEnumerator OrganizeFilesRoutine()
    {
        // 1. Open File Manager
        DesktopIcon fileManagerIcon = desktopManager.GetIcon(Windows31DesktopManager.ProgramType.FileManager);
        if (fileManagerIcon == null) { isBusy = false; yield break; }
        yield return cursorController.MoveToAndDoubleClick(fileManagerIcon.transform.position, GetMood());
        yield return new WaitForSeconds(1.5f);

        // 2. Open Notepad to cross-reference
        DesktopIcon notesIcon = desktopManager.GetIcon(Windows31DesktopManager.ProgramType.Notepad);
        if (notesIcon == null) { isBusy = false; yield break; }
        yield return cursorController.MoveToAndDoubleClick(notesIcon.transform.position, GetMood());
        yield return new WaitForSeconds(1.5f);

        // 3. **Window Management!** Move Notepad so both are visible.
        Window notesWindow = desktopManager.GetWindow("Notepad");
        if (notesWindow != null)
        {
            Vector2 targetPosition = new Vector2(notesWindow.GetComponent<RectTransform>().anchoredPosition.x + 300, notesWindow.GetComponent<RectTransform>().anchoredPosition.y);
            yield return cursorController.MoveAndDrag(notesWindow.titleBar.transform.position, targetPosition - (Vector2)notesWindow.titleBar.transform.position);
            SimulationController.Instance.ReportActivity();
        }

        // 4. "Work" for a bit, with duration influenced by state
        float workDuration = Random.Range(5f, 8f) * (1 - dialogueState.globalTension * 0.5f);
        yield return new WaitForSeconds(workDuration);
        SimulationController.Instance.ReportActivity();

        // 5. Close windows
        Window fileManagerWindow = desktopManager.GetWindow("FileManager");
        if (fileManagerWindow != null && fileManagerWindow.closeButton != null)
        {
            yield return cursorController.MoveToAndClick(fileManagerWindow.closeButton.transform.position, "casual");
        }
        if(notesWindow != null && notesWindow.closeButton != null)
        {
            yield return cursorController.MoveToAndClick(notesWindow.closeButton.transform.position, "casual");
        }
    }

    private IEnumerator ReviewNotesRoutine()
    {
        DesktopIcon notesIcon = desktopManager.GetIcon(Windows31DesktopManager.ProgramType.Notepad);
        if (notesIcon == null) { isBusy = false; yield break; }
        yield return cursorController.MoveToAndDoubleClick(notesIcon.transform.position, "cautious");

        // Read for a while, influenced by meta-awareness
        float readDuration = Random.Range(6f, 10f) + (dialogueState.metaAwareness * 5f);
        yield return new WaitForSeconds(readDuration);
        SimulationController.Instance.ReportActivity();

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
        // The actual thinking logic is now inside Solitaire.cs, this just gives it time to run
        float playDuration = Random.Range(15f, 25f);
        yield return new WaitForSeconds(playDuration);
        SimulationController.Instance.ReportActivity();

        // Get bored or stressed and close it
        Window solitaireWindow = desktopManager.GetWindow("Solitaire");
        if (solitaireWindow != null)
        {
             yield return cursorController.MoveToAndClick(solitaireWindow.closeButton.transform.position, GetMood());
        }
    }
}