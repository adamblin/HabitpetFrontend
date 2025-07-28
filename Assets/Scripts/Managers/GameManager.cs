using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private AuthManager authManager;

    private void Start()
    {
        authManager = FindObjectOfType<AuthManager>();

        if (authManager == null)
        {
            Debug.LogError("AuthManager no trobat! Assegura't que està a la escena.");
            return;
        }

        string token = SessionManager.GetToken();

        if (string.IsNullOrEmpty(token))
        {
            Debug.Log("No hi ha cap token. Mostrant pantalla de login.");
            authManager.uiManager.ShowPanel("Login"); // Mostrar el login en lugar de redirigir
        }
        else
        {
            Debug.Log("Token trobat. L'usuari està logejat.");
        }
    }
}
