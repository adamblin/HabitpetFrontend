using System;
using System.Collections;
using UnityEngine;

public class FriendService : MonoBehaviour
{
    public static FriendService Instance { get; private set; }

    private static string FriendsListUrl => Endpoints.FriendsList();
    private static string FriendRequestsUrl => Endpoints.FriendRequests();
    private static string RequestFriendUrl(string u) => Endpoints.RequestFriend(u);
    private static string AcceptFriendUrl(string u) => Endpoints.AcceptFriend(u);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public IEnumerator SendFriendRequest(string friendUsername, string token, Action<bool, string> callback)
    {
        if (string.IsNullOrWhiteSpace(friendUsername))
        {
            callback?.Invoke(false, "El nombre de usuario no puede estar vacío.");
            yield break;
        }
        if (string.IsNullOrEmpty(token))
        {
            callback?.Invoke(false, "Token vacío o nulo.");
            yield break;
        }

        yield return ApiClient.PostNoBody(
            url: RequestFriendUrl(friendUsername),
            onSuccess: _ => callback?.Invoke(true, null),
            onError: err => callback?.Invoke(false, err),
            withAuth: true
        );
    }

    public IEnumerator GetFriendRequests(string token, Action<FriendData[]> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrEmpty(token))
        {
            onError?.Invoke("Token vacío o nulo.");
            yield break;
        }

        yield return ApiClient.GetArray<FriendData>(
            url: FriendRequestsUrl,
            onSuccess: onSuccess,
            onError: onError,
            withAuth: true
        );
    }

    public IEnumerator GetFriends(string token, Action<FriendData[]> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrEmpty(token))
        {
            onError?.Invoke("Token vacío o nulo.");
            yield break;
        }

        yield return ApiClient.GetArray<FriendData>(
            url: FriendsListUrl,
            onSuccess: onSuccess,
            onError: onError,
            withAuth: true
        );
    }

    public IEnumerator AcceptFriend(string senderUsername, string token, Action<bool, string> callback)
    {
        if (string.IsNullOrWhiteSpace(senderUsername))
        {
            callback?.Invoke(false, "El remitente no puede estar vacío.");
            yield break;
        }
        if (string.IsNullOrEmpty(token))
        {
            callback?.Invoke(false, "Token vacío o nulo.");
            yield break;
        }

        yield return ApiClient.PostNoBody(
            url: AcceptFriendUrl(senderUsername),
            onSuccess: _ => callback?.Invoke(true, null),
            onError: err => callback?.Invoke(false, err),
            withAuth: true
        );
    }
}
