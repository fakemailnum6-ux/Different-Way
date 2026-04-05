using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Godot;

namespace DifferentWay.Security
{
    public class KeyManager
    {
        private readonly string _prefsFilePath = "user_prefs.cfg";

        public void SaveApiKey(string apiKey)
        {
            try
            {
                byte[] dataToEncrypt = Encoding.UTF8.GetBytes(apiKey);
                // Use DPAPI for encryption (Windows only, but meets Arc.md specs).
                // If on non-Windows, this will throw PlatformNotSupportedException.
                // In a true cross-platform scenario, a platform check + local AES with
                // securely generated/stored key in a keystore would be needed.
                byte[] encryptedData = ProtectedData.Protect(dataToEncrypt, null, DataProtectionScope.CurrentUser);

                File.WriteAllBytes(_prefsFilePath, encryptedData);
                GD.Print("API Key saved securely via DPAPI.");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to save API key: {ex.Message}");
            }
        }

        public string LoadApiKey()
        {
            if (!File.Exists(_prefsFilePath))
            {
                return null;
            }

            try
            {
                byte[] encryptedData = File.ReadAllBytes(_prefsFilePath);
                byte[] decryptedData = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);

                return Encoding.UTF8.GetString(decryptedData);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to load API key: {ex.Message}");
                return null;
            }
        }
    }
}
