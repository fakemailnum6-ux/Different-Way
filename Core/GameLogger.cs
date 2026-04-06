using System;
using System.IO;
using System.Threading;
using Godot;

namespace DifferentWay.Core;

public partial class GameLogger : Node
{
    [Signal]
    public delegate void LogMessageEventHandler(string message);

    private static readonly string LogFilePath = ProjectSettings.GlobalizePath("user://game_log.txt");
    private static ReaderWriterLockSlim _logLock = new ReaderWriterLockSlim();

    private static GameLogger? _instance;

    public override void _Ready()
    {
        _instance = this;
        // Optionally clear log file at start
        try
        {
            if (File.Exists(LogFilePath))
            {
                File.Delete(LogFilePath);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to clear old game log: {ex.Message}");
        }

        Log("GameLogger initialized.");
    }

    public static void Log(string message)
    {
        string formattedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";

        // Output to Godot debugger console as well
        GD.Print(formattedMessage);

        _logLock.EnterWriteLock();
        try
        {
            using StreamWriter writer = File.AppendText(LogFilePath);
            writer.WriteLine(formattedMessage);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to write to game log: {ex.Message}");
        }
        finally
        {
            _logLock.ExitWriteLock();
        }

        // CallDeferred ensures the signal is emitted safely on the main UI thread
        _instance?.CallDeferred(MethodName.EmitLogSignal, formattedMessage);
    }

    public static void LogError(string message)
    {
        Log($"ERROR: {message}");
    }

    // Instance wrapper for GDScript's .call()
    public void WriteLog(string message)
    {
        Log(message);
    }

    // Instance wrapper for GDScript's .call()
    public void WriteLogError(string message)
    {
        LogError(message);
    }

    private void EmitLogSignal(string message)
    {
        EmitSignal(SignalName.LogMessage, message);
    }
}
