using System;
using System.Text.RegularExpressions;
using Godot;

namespace DifferentWay.AI
{
    public class ContentFilter
    {
        // Add more patterns as required
        private readonly string[] _stopPatterns = new string[]
        {
            @"\bОкей\b",
            @"\bБро\b",
            @"\bКомпьютер\b",
            @"Как языковая модель"
        };

        public bool CheckContent(string text)
        {
            foreach (var pattern in _stopPatterns)
            {
                if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
                {
                    GD.Print($"Content filter triggered by pattern: {pattern}");
                    return false;
                }
            }
            return true;
        }
    }
}
