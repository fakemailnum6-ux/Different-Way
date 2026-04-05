using System;
using System.IO;
using Godot;

namespace DifferentWay.AI;

public static class TelemetryLogger
{
    private static readonly string LogFilePath = ProjectSettings.GlobalizePath("user://hallucinations_log.txt");

    // 8. TelemetryLogger: Сброс галлюцинаций в локальный текстовый лог для дальнейшего анализа.
    public static void LogHallucination(string requestJson, string responseJson, string validationError)
    {
        try
        {
            using StreamWriter writer = File.AppendText(LogFilePath);
            writer.WriteLine($"--- AI HALLUCINATION LOG [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ---");
            writer.WriteLine($"Request:\n{requestJson}");
            writer.WriteLine($"Response:\n{responseJson}");
            writer.WriteLine($"Validation Error: {validationError}");
            writer.WriteLine("----------------------------------------------------------\n");

            GD.Print($"Logged AI hallucination to {LogFilePath}");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to write telemetry log: {ex.Message}");
        }
    }
}
