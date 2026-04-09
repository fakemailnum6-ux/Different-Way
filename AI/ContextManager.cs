using Godot;
using System.Collections.Generic;
using System.Text;

public partial class ContextManager : RefCounted
{
    private const int MaxMessages = 10;
    private readonly Queue<string> _messageHistory = new Queue<string>();

    public void AddMessage(string role, string content)
    {
        string formattedMsg = $"{role}: {content}";

        if (_messageHistory.Count >= MaxMessages)
        {
            _messageHistory.Dequeue();
        }

        _messageHistory.Enqueue(formattedMsg);
    }

    public void ClearHistory()
    {
        _messageHistory.Clear();
    }

    /// <summary>
    /// Builds the P4 context layer (Chat History) zero-allocation style.
    /// </summary>
    public void AppendHistoryToBuilder(StringBuilder builder)
    {
        builder.AppendLine("--- DIALOGUE HISTORY (P4) ---");
        if (_messageHistory.Count == 0)
        {
            builder.AppendLine("No previous history.");
            return;
        }

        foreach (var msg in _messageHistory)
        {
            builder.AppendLine(msg);
        }
    }
}
