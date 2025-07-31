using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class FriendService : MonoBehaviour
{
    public static FriendService Instance { get; private set; }

    private string baseUrl = "http://localhost:8080/friendships";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public IEnumerator SendFriendRequest(string friendUsername, string token, Action<bool, string> callback)
    {
        string url = $"{baseUrl}/request/{friendUsername}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        bool success = request.result == UnityWebRequest.Result.Success;
        callback?.Invoke(success, request.downloadHandler.text);
    }

    public IEnumerator GetFriendRequests(string token, Action<FriendData[]> onSuccess, Action<string> onError)
    {
        UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}/requests");
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            FriendData[] data = JsonHelper.FromJson<FriendData>(request.downloadHandler.text);
            onSuccess?.Invoke(data);
        }
        else
        {
            onError?.Invoke(request.downloadHandler.text);
        }
    }

    public IEnumerator GetFriends(string token, Action<FriendData[]> onSuccess, Action<string> onError)
    {
        UnityWebRequest request = UnityWebRequest.Get(baseUrl);
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            FriendData[] data = JsonHelper.FromJson<FriendData>(request.downloadHandler.text);
            onSuccess?.Invoke(data);
        }
        else
        {
            onError?.Invoke(request.downloadHandler.text);
        }
    }

    public IEnumerator AcceptFriend(string senderUsername, string token, Action<bool, string> callback)
    {
        string url = $"{baseUrl}/accept/{senderUsername}";
        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, "");
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        bool success = request.result == UnityWebRequest.Result.Success;
        callback?.Invoke(success, request.downloadHandler.text);
    }
}
