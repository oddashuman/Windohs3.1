using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// **New Script: FileManager**
/// Vision: Implements the File Manager program. It creates a virtual file system
/// for Orion to browse, filled with research documents, logs, and anomalous files
/// that build the narrative and provide environmental storytelling.
/// </summary>
public class FileManager : MonoBehaviour
{
    public Window parentWindow;
    private TextMeshProUGUI fileListText;

    // Virtual File System
    private VirtualFolder currentFolder;
    private Dictionary<string, VirtualFolder> fileSystem;

    public void Initialize()
    {
        CreateVirtualFileSystem();
        currentFolder = fileSystem["C:\\"];
        
        // Create the UI
        var textGO = new GameObject("FileList");
        textGO.transform.SetParent(transform, false);
        fileListText = textGO.AddComponent<TextMeshProUGUI>();
        fileListText.font = Windows31DesktopManager.Instance.windows31Font;
        fileListText.fontSize = 12;
        fileListText.color = Color.black;
        fileListText.alignment = TextAlignmentOptions.TopLeft;

        var rt = textGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(5, 5);
        rt.offsetMax = new Vector2(-5, -5);

        DisplayCurrentFolder();
    }

    private void CreateVirtualFileSystem()
    {
        fileSystem = new Dictionary<string, VirtualFolder>();

        // Root
        var root = new VirtualFolder("C:\\");
        root.AddFile(new VirtualFile("autoexec.bat", "1 KB"));
        root.AddFile(new VirtualFile("config.sys", "1 KB"));
        fileSystem["C:\\"] = root;

        // Folders
        var research = new VirtualFolder("RESEARCH");
        research.AddFile(new VirtualFile("Loop_Theory.txt", "18 KB"));
        research.AddFile(new VirtualFile("Signal_Analysis.log", "152 KB"));
        research.AddFile(new VirtualFile("Overseer_Hypothesis.doc", "5 KB"));
        research.AddFile(new VirtualFile("participant_nova.txt", "2 KB"));
        research.AddFile(new VirtualFile("participant_echo.txt", "3 KB"));
        research.AddFile(new VirtualFile("participant_lumen.txt", "4 KB"));
        root.AddFolder(research);

        var system = new VirtualFolder("SYSTEM");
        system.AddFile(new VirtualFile("kernel32.dll", "432 KB"));
        system.AddFile(new VirtualFile("user32.dll", "218 KB"));
        system.AddFile(new VirtualFile("neural_cascade.dll", "876 MB"));
        root.AddFolder(system);
        
        var logs = new VirtualFolder("LOGS");
        logs.AddFile(new VirtualFile("glitch_events.log", "98 KB"));
        logs.AddFile(new VirtualFile("user_connections.log", "44 KB"));
        // Anomalous file hinting at future events
        var futureLog = new VirtualFile("CRISIS_07142025.log", "?? KB");
        futureLog.timestamp = "07/14/2025";
        logs.AddFile(futureLog);
        system.AddFolder(logs);
    }

    private void DisplayCurrentFolder()
    {
        string header = $"Directory of {currentFolder.name}\n\n";
        string content = "";

        foreach (var folder in currentFolder.subfolders)
        {
            content += $"<color=yellow>[{folder.name}]</color>\n";
        }
        foreach (var file in currentFolder.files)
        {
            content += $"{file.name.PadRight(25)}{file.size.PadRight(10)}{file.timestamp}\n";
        }

        fileListText.text = header + content;
    }
}

public class VirtualFile
{
    public string name;
    public string size;
    public string timestamp;

    public VirtualFile(string name, string size)
    {
        this.name = name;
        this.size = size;
        this.timestamp = "07/13/2025"; // Default to today
    }
}

public class VirtualFolder
{
    public string name;
    public List<VirtualFile> files = new List<VirtualFile>();
    public List<VirtualFolder> subfolders = new List<VirtualFolder>();

    public VirtualFolder(string name) { this.name = name; }
    public void AddFile(VirtualFile file) { files.Add(file); }
    public void AddFolder(VirtualFolder folder) { subfolders.Add(folder); }
}