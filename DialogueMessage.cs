/// <summary>
/// A simple data container for a single line of dialogue.
/// This class is now in its own file to be accessible project-wide.
/// </summary>
public class DialogueMessage
{
    public string speaker;
    public string text;

    public DialogueMessage(string speaker, string text)
    {
        this.speaker = speaker;
        this.text = text;
    }
}