using Godot;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public partial class KeyManager : RefCounted
{
    // The user explicitly requested user://settings.cfg in the prompt,
    // overriding the memory (user://config/api_key.dat)
    private const string ApiKeyPath = "user://settings.cfg";

    public static void SaveApiKey(string key)
    {
        string path = ProjectSettings.GlobalizePath(ApiKeyPath);
        Directory.CreateDirectory(Path.GetDirectoryName(path));

        // Simplified DPAPI encryption for Windows, fallback to base64 for cross-platform stub
        try
        {
            if (OperatingSystem.IsWindows())
            {
                byte[] encryptedData = ProtectedData.Protect(
                    Encoding.UTF8.GetBytes(key),
                    null,
                    DataProtectionScope.CurrentUser
                );
                File.WriteAllBytes(path, encryptedData);
            }
            else
            {
                File.WriteAllText(path, Convert.ToBase64String(Encoding.UTF8.GetBytes(key)));
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to save API key: {e.Message}");
        }
    }

    public static string LoadApiKey()
    {
        string path = ProjectSettings.GlobalizePath(ApiKeyPath);
        if (!File.Exists(path)) return null;

        try
        {
            if (OperatingSystem.IsWindows())
            {
                byte[] encryptedData = File.ReadAllBytes(path);
                byte[] unencryptedData = ProtectedData.Unprotect(
                    encryptedData,
                    null,
                    DataProtectionScope.CurrentUser
                );
                return Encoding.UTF8.GetString(unencryptedData);
            }
            else
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(File.ReadAllText(path)));
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to load API key: {e.Message}");
            return null;
        }
    }
}
