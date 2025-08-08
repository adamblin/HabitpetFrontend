using UnityEngine;

public static class SessionManager
{
    private const string TokenKey = "authToken";
    private const string RememberKey = "rememberMe";

    private static string sessionToken;

    public static void SaveToken(string token, bool remember)
    {
        sessionToken = token;

        if (remember)
        {
            PlayerPrefs.SetString(TokenKey, token);
            PlayerPrefs.SetInt(RememberKey, 1);
        }
        else
        {
            PlayerPrefs.DeleteKey(TokenKey);
            PlayerPrefs.SetInt(RememberKey, 0);
        }

        PlayerPrefs.Save();
        Debug.Log($"[SessionManager] Token guardado: {(string.IsNullOrEmpty(token) ? "[vacío]" : token)} | Remember: {remember}");
    }

    public static string GetToken()
    {
        if (!string.IsNullOrEmpty(sessionToken))
            return sessionToken;

        if (PlayerPrefs.GetInt(RememberKey, 0) == 1)
        {
            sessionToken = PlayerPrefs.GetString(TokenKey, "");
            Debug.Log($"[SessionManager] Token recuperado de PlayerPrefs: {(string.IsNullOrEmpty(sessionToken) ? "[vacío]" : sessionToken)}");
            return sessionToken;
        }

        return null;
    }

    public static bool HasToken()
    {
        return !string.IsNullOrEmpty(GetToken());
    }

    public static bool IsRemembered()
    {
        return PlayerPrefs.GetInt(RememberKey, 0) == 1;
    }

    public static void ClearToken()
    {
        sessionToken = null;
        PlayerPrefs.DeleteKey(TokenKey);
        PlayerPrefs.DeleteKey(RememberKey);
        PlayerPrefs.Save();
        Debug.Log("[SessionManager] Token limpiado.");
    }
}
