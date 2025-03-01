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
    public bool accepted;
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
        {
            addFriendButton.onClick.RemoveAllListeners();
            addFriendButton.onClick.AddListener(SendFriendRequest);
        }
        else
        {
            Debug.LogError("AddFriendButton no asignado en el Inspector.");
        }

       
        if (fetchRequestsButton != null)
        {
            fetchRequestsButton.onClick.RemoveAllListeners();
            fetchRequestsButton.onClick.AddListener(() =>
            {
                StartCoroutine(GetFriendRequests());
            });
        }
        else
        {
            Debug.LogError("fetchRequestsButton no asignado en el Inspector.");
        }

        if (fetchFriendsButton != null)
        {
            fetchFriendsButton.onClick.RemoveAllListeners();
            fetchFriendsButton.onClick.AddListener(() =>
            {
                StartCoroutine(GetFriends());
            });
        }
        else
        {
            Debug.LogError("fetchFriendsButton no asignado en el Inspector.");
        }
    }

    // (OPCIONAL) Si no deseas que se haga fetch automáticamente al habilitar el GameObject, comenta o elimina OnEnable
    // void OnEnable()
    // {
    //     FetchFriendsAndRequests();
    // }

    // Método que podrías seguir usando si quisieras un botón único que haga ambas peticiones
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
                Debug.Log($"Solicitudes de amistad obtenidas: {request.downloadHandler.text}");

                FriendData[] requests = JsonHelper.FromJson<FriendData>(request.downloadHandler.text);
                Debug.Log($"Número de solicitudes recibidas: {requests.Length}");

                foreach (Transform child in requestListContent)
                {
                    Destroy(child.gameObject);
                }

                foreach (var requestItem in requests)
                {
                    // 'userUsername' es el remitente de la solicitud
                    Debug.Log($"Solicitud de: {requestItem.userUsername} (envía) para: {requestItem.friendUsername}, aceptada: {requestItem.accepted}");

                    // Mostrar la solicitud solo si aún no ha sido aceptada
                    if (!requestItem.accepted)
                    {
                        AddRequestToUI(requestItem.userUsername);
                    }
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

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("No token found! Redirecting to login.");
            yield break;
        }

        // Ajusta la ruta según tu backend; si /accepted no existe, usa /friendships
        using (UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}"))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Amigos obtenidos: {request.downloadHandler.text}");
                FriendData[] friends = JsonHelper.FromJson<FriendData>(request.downloadHandler.text);

                // Limpiar la lista anterior
                foreach (Transform child in friendListContent)
                {
                    Destroy(child.gameObject);
                }

                // Mostrar amigos
                foreach (var friend in friends)
                {
                    AddFriendToUI(friend.friendUsername);
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
        Debug.Log($"Enviando solicitud a {url} con token: {token}");

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
            Debug.LogWarning($"Solicitud de amistad a {friendUsername} ya fue enviada previamente.");
        }
        else
        {
            Debug.LogError($"Error enviando solicitud: {request.responseCode} - {request.error}");
            Debug.LogError($"Respuesta del servidor: {request.downloadHandler.text}");
        }
    }

    private void AddRequestToUI(string displayName)
    {
        if (requestPrefab == null || requestListContent == null)
        {
            Debug.LogError("requestPrefab o requestListContent no asignado en el Inspector.");
            return;
        }

        GameObject requestItem = Instantiate(requestPrefab, requestListContent);
        TextMeshProUGUI requestText = requestItem.transform.Find("UsernameText")?.GetComponent<TextMeshProUGUI>();
        Button acceptButton = requestItem.transform.Find("AcceptButton")?.GetComponent<Button>();
        Button declineButton = requestItem.transform.Find("DeclineButton")?.GetComponent<Button>();


        if (requestText != null)
            requestText.text = displayName;
        else
            Debug.LogError("No se encontró UsernameText en el prefab.");

        if (acceptButton != null)
            acceptButton.onClick.AddListener(() => AcceptFriend(displayName, requestItem));
        else
            Debug.LogError("No se encontró AcceptButton en el prefab.");

        if (declineButton != null)
            declineButton.onClick.AddListener(() => Destroy(requestItem));
        else
            Debug.LogError("No se encontró DeclineButton en el prefab.");
    }

    private void AcceptFriend(string friendUsername, GameObject requestItem)
    {
        StartCoroutine(AcceptFriendRequest(friendUsername, requestItem));
    }

    private IEnumerator AcceptFriendRequest(string friendUsername, GameObject requestItem)
    {
        string token = authManager?.GetToken();

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("No token found! Redirecting to login.");
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequest.PostWwwForm($"{baseUrl}/accept/{friendUsername}", ""))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Amistad con {friendUsername} aceptada.");
                Destroy(requestItem);
                AddFriendToUI(friendUsername);
            }
            else
            {
                Debug.LogError($"Error aceptando amigo: {request.responseCode} - {request.error}");
                Debug.LogError($"Respuesta del servidor: {request.downloadHandler.text}");
            }
        }
    }

    private void AddFriendToUI(string friendUsername)
    {
        if (friendPrefab == null || friendListContent == null)
        {
            Debug.LogError("friendPrefab o friendListContent no está asignado en el Inspector.");
            return;
        }

        GameObject friendItem = Instantiate(friendPrefab, friendListContent);
        TextMeshProUGUI friendText = friendItem.transform.Find("FriendText")?.GetComponent<TextMeshProUGUI>();

        if (friendText != null)
            friendText.text = friendUsername;
        else
            Debug.LogError("No se encontró FriendText en el prefab.");

        Debug.Log($"Amigo {friendUsername} añadido a la lista.");
    }
}
