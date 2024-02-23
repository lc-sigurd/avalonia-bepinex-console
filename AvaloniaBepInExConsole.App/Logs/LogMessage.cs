namespace AvaloniaBepInExConsole.App.Logs;

public class LogMessage
{
    public string Content { get; }

    internal LogMessage(string content)
    {
        Content = content;
    }
}
