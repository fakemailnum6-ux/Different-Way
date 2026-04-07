using System.Security.Cryptography;
using System.Text;
using System.Runtime.Versioning;

namespace DifferentWay.Security;

using Godot;
using System.IO;

public static class KeyManager
{
    private static readonly string KeyFilePath = ProjectSettings.GlobalizePath("user://config/api_key.dat");

    [SupportedOSPlatform("windows")]
    public static byte[] EncryptKey(string apiKey)
    {
        byte[] secretData = Encoding.UTF8.GetBytes(apiKey);
        return ProtectedData.Protect(secretData, null, DataProtectionScope.CurrentUser);
    }

    [SupportedOSPlatform("windows")]
    public static string DecryptKey(byte[] encryptedData)
    {
        byte[] unencryptedData = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(unencryptedData);
    }

    [SupportedOSPlatform("windows")]
    public static void SaveKey(string apiKey)
    {
        try
        {
            // Ensure directory exists
            string dir = Path.GetDirectoryName(KeyFilePath)!;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            byte[] encryptedData = EncryptKey(apiKey);
            File.WriteAllBytes(KeyFilePath, encryptedData);
            DifferentWay.Core.GameLogger.Log("API Key securely saved.");
        }
        catch (System.Exception ex)
        {
            DifferentWay.Core.GameLogger.LogError($"Failed to save API key: {ex.Message}");
        }
    }

    [SupportedOSPlatform("windows")]
    public static string LoadKey()
    {
        try
        {
            if (File.Exists(KeyFilePath))
            {
                byte[] encryptedData = File.ReadAllBytes(KeyFilePath);
                return DecryptKey(encryptedData);
            }
        }
        catch (System.Exception ex)
        {
            DifferentWay.Core.GameLogger.LogError($"Failed to load API key: {ex.Message}");
        }
        return string.Empty;
    }
}
