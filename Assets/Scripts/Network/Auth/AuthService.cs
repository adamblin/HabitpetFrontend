using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AuthService : MonoBehaviour
{
    public static AuthService Instance { get; private set; }

    private readonly string baseUrl = "http://localhost:8080/auth";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public IEnumerator Login(string username, string password, bool rememberMe, Action onSuccess, Action<string> onError)
    {
        var loginData = new LoginRequest(username, password);
        string json = JsonUtility.ToJson(loginData);
        Debug.Log("[Unity] JSON Login: " + json);

        UnityWebRequest request = new UnityWebRequest($"{baseUrl}/login", "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);
            Debug.Log("[Unity] Login correcto. TOKEN: " + response.token);

            SessionManager.SaveToken(response.token, rememberMe);
            onSuccess?.Invoke();
        }
        else
        {
            Debug.LogError("[Unity] Error en Login: " + request.responseCode + " - " + request.error);
            onError?.Invoke(request.downloadHandler.text);
        }
    }

    public IEnumerator Register(string username, string email, string password, bool rememberMe, Action onSuccess, Action<string> onError)
    {
        var registerData = new RegisterRequest(username, email, password);
        string json = JsonUtility.ToJson(registerData);
        Debug.Log("[Unity] JSON Register: " + json);

        UnityWebRequest request = new UnityWebRequest($"{baseUrl}/register", "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);
            Debug.Log("[Unity] Registro correcto. TOKEN: " + response.token);

            SessionManager.SaveToken(response.token, rememberMe);
            onSuccess?.Invoke();
        }
        else
        {
            Debug.LogError("[Unity] Error en Registro: " + request.responseCode + " - " + request.error);
            onError?.Invoke(request.downloadHandler.text);
        }
    }
}
