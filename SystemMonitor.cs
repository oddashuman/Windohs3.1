using UnityEngine;
using TMPro;
using System.Collections;
using System.Text;

/// <summary>
/// **New Script: SystemMonitor**
/// Vision: Creates a dynamic, story-aware System Monitor. The process list is no
/// longer static; it updates in real-time and reflects the narrative state. The
/// mysterious "overseer_agent.exe" will appear and spike in usage during moments
/// of high tension, making the system itself a character in the story.
/// </summary>
public class SystemMonitor : MonoBehaviour
{
    public Window parentWindow;
    private TextMeshProUGUI processListText;
    private Coroutine monitorCoroutine;

    private float ramUsage = 128.4f;

    public void Initialize()
    {
        var textGO = new GameObject("ProcessListText");
        textGO.transform.SetParent(transform, false);
        processListText = textGO.AddComponent<TextMeshProUGUI>();
        processListText.font = Windows31DesktopManager.Instance.windows31Font;
        processListText.fontSize = 12;
        processListText.color = Color.black;
        processListText.alignment = TextAlignmentOptions.TopLeft;
        
        var rt = textGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(5, 5); rt.offsetMax = new Vector2(-5, -5);

        monitorCoroutine = StartCoroutine(UpdateProcesses());
    }

    private IEnumerator UpdateProcesses()
    {
        while (true)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Process\t\tCPU\tMemory");
            sb.AppendLine("----------------------------------");
            sb.AppendLine("csrss.exe\t\t1%\t12.4 MB");
            sb.AppendLine("winlogon.exe\t3%\t8.2 MB");
            sb.AppendLine("explorer.exe\t5%\t45.1 MB");
            
            // Dynamic RAM usage for neural_cascade
            ramUsage += Random.Range(0.1f, 1.5f);
            sb.AppendLine($"neural_cascade.dll\t{Random.Range(60, 85)}%\t{ramUsage:F1} MB");

            // The story-critical process
            if (ShouldOverseerBeVisible())
            {
                DialogueState state = DialogueState.Instance;
                int cpuUsage = (int)(state.globalTension * 50) + Random.Range(5, 15);
                string memUsage = state.systemIntegrityCompromised ? "???" : "4.7 MB";
                sb.AppendLine($"<color=red>overseer_agent.exe</color>\t{cpuUsage}%\t{memUsage}");
            }
            
            processListText.text = sb.ToString();

            yield return new WaitForSeconds(1.5f); // Refresh rate
        }
    }

    private bool ShouldOverseerBeVisible()
    {
        DialogueState state = DialogueState.Instance;
        // The Overseer process only appears when Orion is deeply suspicious or the system is unstable.
        return state.globalTension > 0.5f || state.metaAwareness > 0.7f || state.glitchCount > 3;
    }

    void OnDestroy()
    {
        if (monitorCoroutine != null)
        {
            StopCoroutine(monitorCoroutine);
        }
    }
}