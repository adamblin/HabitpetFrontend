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

        if (string.IsNullOrEmpty(authManager.GetToken()))
        {
            Debug.Log("No hi ha cap token. Redirigint al login.");
            SceneManager.LoadScene("Auth");
        }
        else
        {
            Debug.Log("Token trobat. L'usuari està logejat.");
        }
    }

}
