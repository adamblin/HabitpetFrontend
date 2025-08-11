using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class ApiClient
{
    public static IEnumerator Get<T>(string url, Action<T> onSuccess, Action<string> onError = null, bool withAuth = true)
    {
        using var req = UnityWebRequest.Get(url);
        req.downloadHandler = new DownloadHandlerBuffer();
        AddDefaultHeaders(req, withAuth);
        yield return req.SendWebRequest();
        HandleResponse(req, onSuccess, onError);
    }

    public static IEnumerator GetArray<T>(string url, Action<T[]> onSuccess, Action<string> onError = null, bool withAuth = true)
    {
        using var req = UnityWebRequest.Get(url);
        req.downloadHandler = new DownloadHandlerBuffer();
        AddDefaultHeaders(req, withAuth);
        yield return req.SendWebRequest();

        if (HttpUtils.IsSuccess(req.responseCode))
        {
            try
            {
                string wrapped = "{\"items\":" + (req.downloadHandler?.text ?? "[]") + "}";
                var box = JsonUtility.FromJson<Box<T>>(wrapped);
                onSuccess?.Invoke(box?.items ?? Array.Empty<T>());
            }
            catch (Exception e)
            {
                onError?.Invoke("Error deserializando JSON[]: " + e.Message);
            }
        }
        else
        {
            onError?.Invoke(HttpUtils.BuildError(req.responseCode, req.error, req.downloadHandler?.text));
        }
    }

    public static IEnumerator Post<T>(string url, object data, Action<T> onSuccess, Action<string> onError = null, bool withAuth = true)
    {
        string json = data is string s ? s : JsonUtility.ToJson(data);
        using var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        AddDefaultHeaders(req, withAuth);
        yield return req.SendWebRequest();
        HandleResponse(req, onSuccess, onError);
    }

    public static IEnumerator PostNoBody(string url, Action<string> onSuccess, Action<string> onError = null, bool withAuth = true)
    {
        using var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
        req.downloadHandler = new DownloadHandlerBuffer();
        AddDefaultHeaders(req, withAuth);
        yield return req.SendWebRequest();

        if (HttpUtils.IsSuccess(req.responseCode))
            onSuccess?.Invoke(req.downloadHandler?.text ?? "");
        else
            onError?.Invoke(HttpUtils.BuildError(req.responseCode, req.error, req.downloadHandler?.text));
    }

    public static IEnumerator Put<T>(string url, object data, Action<T> onSuccess, Action<string> onError = null, bool withAuth = true)
    {
        string json = data is string s ? s : JsonUtility.ToJson(data);
        using var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPUT);
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        AddDefaultHeaders(req, withAuth);
        yield return req.SendWebRequest();
        HandleResponse(req, onSuccess, onError);
    }

    public static IEnumerator Patch<T>(string url, object data, Action<T> onSuccess, Action<string> onError = null, bool withAuth = true)
    {
        string json = data is string s ? s : JsonUtility.ToJson(data);
        using var req = new UnityWebRequest(url, "PATCH");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        AddDefaultHeaders(req, withAuth);
        yield return req.SendWebRequest();
        HandleResponse(req, onSuccess, onError);
    }

    public static IEnumerator PatchNoBody(string url, Action<string> onSuccess, Action<string> onError = null, bool withAuth = true)
    {
        using var req = new UnityWebRequest(url, "PATCH");
        req.downloadHandler = new DownloadHandlerBuffer();
        AddDefaultHeaders(req, withAuth);
        yield return req.SendWebRequest();

        if (HttpUtils.IsSuccess(req.responseCode))
            onSuccess?.Invoke(req.downloadHandler?.text ?? "");
        else
            onError?.Invoke(HttpUtils.BuildError(req.responseCode, req.error, req.downloadHandler?.text));
    }

    public static IEnumerator Delete(string url, Action onSuccess, Action<string> onError = null, bool withAuth = true)
    {
        using var req = UnityWebRequest.Delete(url);
        req.downloadHandler = new DownloadHandlerBuffer();
        AddDefaultHeaders(req, withAuth);
        yield return req.SendWebRequest();

        if (HttpUtils.IsSuccess(req.responseCode)) onSuccess?.Invoke();
        else onError?.Invoke(HttpUtils.BuildError(req.responseCode, req.error, req.downloadHandler?.text));
    }


    private static void AddDefaultHeaders(UnityWebRequest req, bool withAuth)
    {
        if (withAuth)
        {
            var tok = SessionManager.GetToken();
            if (!string.IsNullOrEmpty(tok))
                req.SetRequestHeader("Authorization", HttpUtils.NormalizeBearer(tok));
        }
    }

    private static void HandleResponse<T>(UnityWebRequest req, Action<T> onSuccess, Action<string> onError)
    {
        long code = req.responseCode;

        if (code == 204)
        {
            Debug.Log("[ApiClient] 204 - No Content");
            onSuccess?.Invoke(default);
            return;
        }

        if (typeof(T) == typeof(string))
        {
            onSuccess?.Invoke((T)(object)(req.downloadHandler?.text ?? ""));
            return;
        }

        if (HttpUtils.IsSuccess(code))
        {
            var body = req.downloadHandler?.text;
            if (string.IsNullOrEmpty(body)) { onSuccess?.Invoke(default); return; }

            try
            {
                var result = JsonUtility.FromJson<T>(body);
                onSuccess?.Invoke(result);
            }
            catch (Exception e)
            {
                onError?.Invoke("Error deserializando JSON: " + e.Message);
            }
        }
        else
        {
            onError?.Invoke(HttpUtils.BuildError(code, req.error, req.downloadHandler?.text));
        }
    }

    [Serializable] private class Box<TT> { public TT[] items; }
}
