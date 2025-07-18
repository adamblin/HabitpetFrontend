using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;


[Serializable]
public class FriendData
{
    public string userUsername;
    public string friendUsername;
    public string userId;
    public string friendId;
    public string status;
}

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

    private string baseUrl = "http://localhost:8080/friendships";
    public AuthManager authManager;

    private void Start()
    {
        if (authManager == null)
        {
            Debug.LogError("AuthManager no encontrado en la escena!");
            return;
        }

        if (addFriendButton != null)
            addFriendButton.onClick.AddListener(SendFriendRequest);

        if (fetchRequestsButton != null)
            fetchRequestsButton.onClick.AddListener(() => StartCoroutine(GetFriendRequests()));

        if (fetchFriendsButton != null)
            fetchFriendsButton.onClick.AddListener(GetAcceptedFriends);

        FetchFriendsAndRequests();

    }

    public void FetchFriendsAndRequests()
    {
        StartCoroutine(GetFriendRequests());
        StartCoroutine(GetFriends());
    }

    private IEnumerator GetFriendRequests()
    {
        string token = authManager?.GetToken();
        string currentUsername = authManager?.GetUsername();

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(currentUsername))
        {
            Debug.LogError("No token or username found! Redirecting to login.");
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}/requests"))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                FriendData[] requests = JsonHelper.FromJson<FriendData>(request.downloadHandler.text);

                foreach (Transform child in requestListContent)
                    Destroy(child.gameObject);

                foreach (var requestItem in requests)
                {
                    if (requestItem.status == "PENDING")
                        AddRequestToUI(requestItem);
                }
            }
            else
            {
                Debug.LogError($"Error obteniendo solicitudes: {request.responseCode} - {request.error}");
                Debug.LogError($"Respuesta del servidor: {request.downloadHandler.text}");
            }
        }
    }

    private IEnumerator GetFriends()
    {
        string token = authManager?.GetToken();
        string currentUsername = authManager?.GetUsername();

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(currentUsername))
        {
            Debug.LogError("No token or username found! Redirecting to login.");
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}"))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                FriendData[] friends = JsonHelper.FromJson<FriendData>(request.downloadHandler.text);

                foreach (Transform child in friendListContent)
                    Destroy(child.gameObject);

                foreach (var friend in friends)
                {
                    if (friend.status == "ACCEPTED")
                        AddFriendToUI(friend);
                }
            }
            else
            {
                Debug.LogError($"Error obteniendo amigos: {request.responseCode} - {request.error}");
                Debug.LogError($"Respuesta del servidor: {request.downloadHandler.text}");
            }
        }
    }

    public void SendFriendRequest()
    {
        string friendUsername = friendNameInput.text.Trim();

        if (string.IsNullOrEmpty(friendUsername))
        {
            Debug.Log("Error: El nombre del amigo no puede estar vacío.");
            return;
        }

        StartCoroutine(SendFriendRequestCoroutine(friendUsername));
    }

    private IEnumerator SendFriendRequestCoroutine(string friendUsername)
    {
        string token = authManager?.GetToken();

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("No token found! Redirecting to login.");
            yield break;
        }

        string url = $"{baseUrl}/request/{friendUsername}";
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Solicitud de amistad enviada a {friendUsername}");
        }
        else if (request.responseCode == 403)
        {
            Debug.LogWarning($"Solicitud ya fue enviada a {friendUsername} o no permitida.");
        }
        else
        {
            Debug.LogError($"Error enviando solicitud: {request.responseCode} - {request.error}");
            Debug.LogError($"Respuesta del servidor: {request.downloadHandler.text}");
        }
    }

    private void AddRequestToUI(FriendData requestData)
    {
        GameObject requestItem = Instantiate(requestPrefab, requestListContent);

        var requestText = requestItem.transform.Find("UsernameText")?.GetComponent<TextMeshProUGUI>();
        if (requestText != null) requestText.text = requestData.userUsername;

        var acceptButton = requestItem.transform.Find("AcceptButton")?.GetComponent<Button>();
        if (acceptButton != null)
        {
            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(() => AcceptFriend(requestData.userUsername, requestItem));
        }

        var declineButton = requestItem.transform.Find("DeclineButton")?.GetComponent<Button>();
        if (declineButton != null)
        {
            declineButton.onClick.RemoveAllListeners();
            declineButton.onClick.AddListener(() => Destroy(requestItem));
        }
    }

    private void AcceptFriend(string senderUsername, GameObject requestItem)
    {
        StartCoroutine(AcceptFriendRequest(senderUsername, requestItem));
    }

    private IEnumerator AcceptFriendRequest(string senderId, GameObject requestItem)
    {
        string token = authManager?.GetToken();

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("No token found! Redirecting to login.");
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequest.PostWwwForm($"{baseUrl}/accept/{senderId}", ""))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Amistad aceptada con ID: {senderId}");

                string acceptedUsername = requestItem.transform.Find("UsernameText")?.GetComponent<TextMeshProUGUI>()?.text;

                if (!string.IsNullOrEmpty(acceptedUsername))
                {
                    FriendData newFriend = new FriendData
                    {
                        userUsername = authManager.GetUsername(),
                        friendUsername = acceptedUsername,
                        status = "ACCEPTED"
                    };

                    AddFriendToUI(newFriend);
                }
                else
                {
                    Debug.LogWarning("No se pudo obtener el nombre del usuario aceptado");
                }

                Destroy(requestItem);
            }
            else
            {
                Debug.LogError($"Error aceptando amigo: {request.responseCode} - {request.error}");
                Debug.LogError($"Respuesta del servidor: {request.downloadHandler.text}");
            }
        }
    }

    private void AddFriendToUI(FriendData friendData)
    {
        GameObject friendItem = Instantiate(friendPrefab, friendListContent);
        var friendText = friendItem.transform.Find("FriendText")?.GetComponent<TextMeshProUGUI>();

        if (friendText != null)
            friendText.text = friendData.friendUsername;
    }


    public void GetAcceptedFriends()
    {
        StartCoroutine(GetAcceptedFriendsCoroutine());
    }


    private IEnumerator GetAcceptedFriendsCoroutine()
    {
        string token = authManager?.GetToken();
        string currentUsername = authManager?.GetUsername();

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(currentUsername))
        {
            Debug.LogError("No token or username found! Redirecting to login.");
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}"))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            yield return request.SendWebRequest();

            // ⬇️ Añade estos logs aquí justo después de recibir la respuesta
            string rawJson = request.downloadHandler.text;
            Debug.Log("📥 JSON recibido: " + rawJson);

            FriendData[] friends = JsonHelper.FromJson<FriendData>(rawJson);
            Debug.Log("✅ Amigos parseados: " + friends.Length);

            foreach (var f in friends)
            {
                Debug.Log($"👤 Amigo: {f.friendUsername} | Estado: {f.status}");
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Limpiar lista anterior
                foreach (Transform child in friendListContent)
                    Destroy(child.gameObject);

                // Mostrar amigos
                foreach (var friend in friends)
                {
                    if (friend.status == "ACCEPTED")
                    {
                        AddFriendToUI(friend);
                    }
                }
            }
            else
            {
                Debug.LogError($"Error obteniendo amigos aceptados: {request.responseCode} - {request.error}");
                Debug.LogError($"Respuesta del servidor: {request.downloadHandler.text}");
            }
        }
    }



}
