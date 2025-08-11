using System;
using System.Collections;
using UnityEngine;

public class AuthService : MonoBehaviour
{
    public static AuthService Instance { get; private set; }

    private static string LoginUrl => Endpoints.Login();    
    private static string RegisterUrl => Endpoints.Register(); 

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public IEnumerator Login(string username, string password, bool rememberMe, Action onSuccess, Action<string> onError)
    {
        var dto = new LoginRequest(username, password);

        yield return ApiClient.Post<AuthResponse>(
            url: LoginUrl,
            data: dto,
            onSuccess: resp =>
            {
                SessionManager.SaveToken(resp.token, rememberMe);
                onSuccess?.Invoke();
            },
            onError: onError,
            withAuth: false 
        );
    }

    public IEnumerator Register(string username, string email, string password, bool rememberMe, Action onSuccess, Action<string> onError)
    {
        var dto = new RegisterRequest(username, email, password);

        yield return ApiClient.Post<AuthResponse>(
            url: RegisterUrl,
            data: dto,
            onSuccess: resp =>
            {
                SessionManager.SaveToken(resp.token, rememberMe);
                onSuccess?.Invoke();
            },
            onError: onError,
            withAuth: false 
        );
    }
}
