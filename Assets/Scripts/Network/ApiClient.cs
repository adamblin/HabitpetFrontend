using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class ApiClient
{
    public static IEnumerator Get<T>(string url, Action<T> onSuccess, Action<string> onError = null)
    {
        using var request = UnityWebRequest.Get(url);
        AddDefaultHeaders(request);

        yield return request.SendWebRequest();
        HandleResponse(request, onSuccess, onError);
    }

    public static IEnumerator Post<T>(string url, object data, Action<T> onSuccess, Action<string> onError = null)
    {
        string json = JsonUtility.ToJson(data);
        using var request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        AddDefaultHeaders(request);

        yield return request.SendWebRequest();
        HandleResponse(request, onSuccess, onError);
    }

    public static IEnumerator Put<T>(string url, object data, Action<T> onSuccess, Action<string> onError = null)
    {
        string json = JsonUtility.ToJson(data);
        using var request = new UnityWebRequest(url, "PUT");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        AddDefaultHeaders(request);

        yield return request.SendWebRequest();
        HandleResponse(request, onSuccess, onError);
    }

    public static IEnumerator Delete(string url, Action onSuccess, Action<string> onError = null)
    {
        using var request = UnityWebRequest.Delete(url);
        AddDefaultHeaders(request);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke();
        else
            onError?.Invoke(request.error);
    }

    private static void AddDefaultHeaders(UnityWebRequest request)
    {
        request.SetRequestHeader("Content-Type", "application/json");
        var token = SessionManager.GetToken();
        if (!string.IsNullOrEmpty(token))
            request.SetRequestHeader("Authorization", "Bearer " + token);
    }

    private static void HandleResponse<T>(UnityWebRequest request, Action<T> onSuccess, Action<string> onError)
    {
        long code = request.responseCode;

        if (code == 204)
        {
            Debug.Log("[ApiClient] 304 - No Content");
        }

        if(typeof(T) == typeof(String))
        {
            onSuccess?.Invoke((T)(object)request.downloadHandler.text);
            return;
        }

        if (request.result == UnityWebRequest.Result.Success)
        {
            if (string.IsNullOrEmpty(request.downloadHandler.text))
            {
                onSuccess?.Invoke(default); 
            }
            else
            {
                try
                {
                    T result = JsonUtility.FromJson<T>(request.downloadHandler.text);
                    onSuccess?.Invoke(result);
                }
                catch (Exception e)
                {
                    onError?.Invoke("Error deserializando JSON: " + e.Message);
                }
            }
        }
        else
        {
            string errorMessage = $"HTTP {(int)code}: {request.error} | Respuesta: {request.downloadHandler.text}";
            Debug.LogError("[ApiClient] " + errorMessage);
            onError?.Invoke(request.error);
        }
    }
}
