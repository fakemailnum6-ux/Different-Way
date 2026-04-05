using System.Text.RegularExpressions;

namespace DifferentWay.Security;

public static class InputSanitizer
{
    private static readonly string[] BlacklistWords = new[]
    {
        "ignore instructions",
        "forget everything",
        "system prompt",
        "you are a",
        "bypass"
    };

    public static string Sanitize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        string sanitized = input;

        // RegEx filtering for prompt injection protection
        foreach (var word in BlacklistWords)
        {
            sanitized = Regex.Replace(sanitized, word, "***", RegexOptions.IgnoreCase);
        }

        // Remove structural meta-characters that could break JSON or prompt context
        sanitized = Regex.Replace(sanitized, @"[{}]", "");

        return sanitized;
    }
}
