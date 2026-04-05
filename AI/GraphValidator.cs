using System;
using System.Text.Json;
using Godot;

namespace DifferentWay.AI
{
    public class GraphValidator
    {
        public bool ValidateJSON(string jsonString)
        {
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(jsonString))
                {
                    JsonElement root = doc.RootElement;

                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        if (root.TryGetProperty("character_thoughts", out _) &&
                            root.TryGetProperty("spoken_text", out _))
                        {
                            return true;
                        }
                    }
                }
                GD.PrintErr("JSON Validation Failed: Missing required properties (character_thoughts or spoken_text).");
                return false;
            }
            catch (JsonException ex)
            {
                GD.PrintErr($"JSON Parsing Failed: {ex.Message}");
                return false;
            }
        }
    }
}
