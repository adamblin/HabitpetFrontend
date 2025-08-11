using UnityEngine;

public class GameManager : MonoBehaviour
{
    private AuthManager authManager;

    private void Start()
    {
        authManager = FindObjectOfType<AuthManager>();
        if (authManager == null)
        {
            Debug.LogWarning("AuthManager no encontrado en la escena.");
            return;
        }

        string token = SessionManager.GetToken();
        if (string.IsNullOrEmpty(token))
        {
            Debug.Log("Sin token. Mostrar Login.");
            authManager.uiManager.ShowPanel("Login"); 
        }
        else
        {
            Debug.Log("Token presente. Usuario logueado.");
        }
    }
}
