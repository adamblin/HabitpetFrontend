using System;
using System.Text;
using UnityEngine;

public static class JwtUtils
{
    [Serializable]
    private class JwtPayload
    {
        public string sub;
    }

    public static string GetUsernameFromToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return null;

        string[] parts = token.Split('.');
        if (parts.Length != 3) return null;

        string payload = parts[1];
        payload = PadBase64(payload);
        try
        {
            byte[] data = Convert.FromBase64String(payload);
            string json = Encoding.UTF8.GetString(data);
            var payloadObj = JsonUtility.FromJson<JwtPayload>(json);
            return payloadObj.sub;
        }
        catch (Exception e)
        {
            Debug.LogError("Error decodificando JWT: " + e.Message);
            return null;
        }
    }

    private static string PadBase64(string base64)
    {
        int padding = 4 - (base64.Length % 4);
        if (padding < 4)
        {
            base64 = base64.PadRight(base64.Length + padding, '=');
        }
        return base64.Replace('-', '+').Replace('_', '/');
    }
}