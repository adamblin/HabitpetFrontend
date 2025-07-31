using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class FriendManager : MonoBehaviour
{
    public TMP_InputField friendNameInput;
    public Button addFriendButton;
    public Button fetchRequestsButton;
    public Button fetchFriendsButton;

    public Transform requestListContent;
    public Transform friendListContent;
    public GameObject requestPrefab;
    public GameObject friendPrefab;

    public AuthManager authManager;

    private void Start()
    {
        if (authManager == null)
            authManager = FindObjectOfType<AuthManager>();

        if (FriendService.Instance == null)
            Debug.LogError("FriendService no está presente en la escena.");

        addFriendButton?.onClick.AddListener(SendFriendRequest);
        fetchRequestsButton?.onClick.AddListener(FetchRequests);
        fetchFriendsButton?.onClick.AddListener(FetchFriends);

        FetchFriendsAndRequests();
    }

    public void FetchFriendsAndRequests()
    {
        FetchRequests();
        FetchFriends();
    }

    public void SendFriendRequest()
    {
        string friendUsername = friendNameInput.text.Trim();
        if (string.IsNullOrEmpty(friendUsername)) return;

        string token = SessionManager.GetToken();
        StartCoroutine(FriendService.Instance.SendFriendRequest(friendUsername, token, (success, response) =>
        {
            if (success)
                Debug.Log("Solicitud enviada.");
            else
                Debug.LogWarning("Error enviando solicitud: " + response);
        }));
    }

    public void FetchRequests()
    {
        string token = SessionManager.GetToken();
        StartCoroutine(FriendService.Instance.GetFriendRequests(token, requests =>
        {
            foreach (Transform t in requestListContent) Destroy(t.gameObject);
            foreach (var r in requests)
                if (r.status == "PENDING")
                    AddRequestToUI(r);
        },
        error => Debug.LogError("Error cargando solicitudes: " + error)));
    }

    public void FetchFriends()
    {
        string token = SessionManager.GetToken();
        StartCoroutine(FriendService.Instance.GetFriends(token, friends =>
        {
            foreach (Transform t in friendListContent) Destroy(t.gameObject);
            foreach (var f in friends)
                if (f.status == "ACCEPTED")
                    AddFriendToUI(f);
        },
        error => Debug.LogError("Error cargando amigos: " + error)));
    }

    private void AddRequestToUI(FriendData data)
    {
        GameObject go = Instantiate(requestPrefab, requestListContent);
        var nameText = go.transform.Find("UsernameText")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null) nameText.text = data.userUsername;

        go.transform.Find("AcceptButton")?.GetComponent<Button>()?.onClick.AddListener(() => AcceptFriend(data.userUsername, go));
        go.transform.Find("DeclineButton")?.GetComponent<Button>()?.onClick.AddListener(() => Destroy(go));
    }

    private void AddFriendToUI(FriendData data)
    {
        GameObject go = Instantiate(friendPrefab, friendListContent);
        var nameText = go.transform.Find("FriendText")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null) nameText.text = data.friendUsername;
    }

    private void AcceptFriend(string senderUsername, GameObject requestItem)
    {
        string token = SessionManager.GetToken();

        StartCoroutine(FriendService.Instance.AcceptFriend(senderUsername, token, (success, response) =>
        {
            if (success)
            {
                string acceptedUsername = requestItem.transform.Find("UsernameText")?.GetComponent<TextMeshProUGUI>()?.text;
                if (!string.IsNullOrEmpty(acceptedUsername))
                {
                    AddFriendToUI(new FriendData
                    {
                        userUsername = authManager.GetUsername(),
                        friendUsername = acceptedUsername,
                        status = "ACCEPTED"
                    });
                }

                Destroy(requestItem);
            }
            else
            {
                Debug.LogError("Error al aceptar solicitud: " + response);
            }
        }));
    }
}
