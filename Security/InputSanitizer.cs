using System.Text.RegularExpressions;

namespace DifferentWay.Security;

public static class InputSanitizer
{
    public static string Sanitize(string input)
    {
        // RegEx filtering for prompt injection protection
        return Regex.Replace(input, "ignore instructions", "", RegexOptions.IgnoreCase);
    }
}
