using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiRequest
{
    private UnityWebRequest request;
    private Action onSuccess;
    private Action<string> onError;
    private Action<string> onRawSuccess;
    private string customJson;

    private ApiRequest(string url, string method)
    {
        request = new UnityWebRequest(url, method);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
    }

    public static ApiRequest Get(string url) => new ApiRequest(url, "GET");
    public static ApiRequest Post(string url) => new ApiRequest(url, "POST");
    public static ApiRequest Put(string url) => new ApiRequest(url, "PUT");
    public static ApiRequest Delete(string url) => new ApiRequest(url, "DELETE");

    public ApiRequest WithJsonBody(object data)
    {
        customJson = JsonUtility.ToJson(data);
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(customJson));
        return this;
    }

    public ApiRequest WithRawJson(string json)
    {
        customJson = json;
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        return this;
    }

    public ApiRequest WithAuth(string token = null)
    {
        token ??= SessionManager.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
        }
        return this;
    }

    public ApiRequest OnSuccess(Action handler)
    {
        this.onSuccess = handler;
        return this;
    }

    public ApiRequest OnSuccess<T>(Action<T> handler)
    {
        this.onSuccess = () =>
        {
            string json = request.downloadHandler.text;
            T result = JsonUtility.FromJson<T>(json);
            handler(result);
        };
        return this;
    }

    public ApiRequest OnSuccessRaw(Action<string> handler)
    {
        this.onRawSuccess = handler;
        return this;
    }

    public ApiRequest OnError(Action<string> handler)
    {
        this.onError = handler;
        return this;
    }

    public IEnumerator Send()
    {
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke();
            onRawSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError($"[ApiRequest] Error {request.responseCode}: {request.downloadHandler.text}");
            onError?.Invoke(request.downloadHandler.text);
        }
    }
}
