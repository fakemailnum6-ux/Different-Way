using Godot;
using System;
using System.IO;
using System.Threading;

public partial class Logger : Node
{
    private const string LogFilePath = "user://game_log.txt";
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
    private string _cachedLogFilePath;

    public override void _Ready()
    {
        _cachedLogFilePath = ProjectSettings.GlobalizePath(LogFilePath);
        try
        {
            File.WriteAllText(_cachedLogFilePath, $"--- Session Started: {DateTime.Now} ---\n");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to initialize logger: {e.Message}");
        }
    }

    public void LogInfo(string message)
    {
        WriteToFile($"[INFO] {DateTime.Now}: {message}");
        GD.Print($"[INFO] {message}");
    }

    public void LogWarning(string message)
    {
        WriteToFile($"[WARNING] {DateTime.Now}: {message}");
        GD.Print($"[WARNING] {message}");
    }

    public void LogError(string message)
    {
        WriteToFile($"[ERROR] {DateTime.Now}: {message}");
        GD.PrintErr($"[ERROR] {message}");
    }

    private void WriteToFile(string content)
    {
        if (string.IsNullOrEmpty(_cachedLogFilePath))
        {
            // Fallback just in case, though it relies on Godot API which might fail off main thread
            _cachedLogFilePath = ProjectSettings.GlobalizePath(LogFilePath);
        }

        _lock.EnterWriteLock();
        try
        {
            File.AppendAllText(_cachedLogFilePath, content + "\n");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Logger failed to write: {e.Message}");
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _lock?.Dispose();
        }
        base.Dispose(disposing);
    }
}
