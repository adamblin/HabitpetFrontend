using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SessionManager
{
    private static string sessionToken;

    public static void SaveToken(string token, bool remember)
    {
        sessionToken = token;
        if (remember)
        {
            PlayerPrefs.SetString("authToken", token);
            PlayerPrefs.SetInt("rememberMe", 1);
            PlayerPrefs.Save();
        }
    }

    public static string GetToken()
    {
        if (!string.IsNullOrEmpty(sessionToken))
            return sessionToken;

        if (PlayerPrefs.GetInt("rememberMe", 0) == 1)
        {
            sessionToken = PlayerPrefs.GetString("authToken");
            return sessionToken;
        }

        return null;
    }


    public static void ClearToken()
    {
        sessionToken = null;
        PlayerPrefs.DeleteKey("authToken");
        PlayerPrefs.DeleteKey("rememberMe");
        PlayerPrefs.Save();
    }
}

