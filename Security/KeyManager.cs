using System.Security.Cryptography;
using System.Text;
using System.Runtime.Versioning;

namespace DifferentWay.Security;

public static class KeyManager
{
    [SupportedOSPlatform("windows")]
    public static byte[] EncryptKey(string apiKey)
    {
        byte[] secretData = Encoding.UTF8.GetBytes(apiKey);
        return ProtectedData.Protect(secretData, null, DataProtectionScope.CurrentUser);
    }
}
