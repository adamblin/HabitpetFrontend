using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class PetService
{
    private static readonly string baseUrl = "http://localhost:8080/users";

    public static IEnumerator CreatePet(string name, Action onSuccess, Action<string> onError)
    {
        string token = SessionManager.GetToken();
        string json = $"{{\"name\":\"{name}\"}}";

        Debug.Log("[Unity] Intentando crear mascota...");
        Debug.Log("Token: " + token);
        Debug.Log("Body JSON: " + json);

        UnityWebRequest request = new UnityWebRequest(baseUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("[Unity] Mascota creada correctamente.");
            onSuccess?.Invoke();
        }
        else
        {
            Debug.LogError("[Unity] Error creando mascota:");
            Debug.LogError("Código HTTP: " + request.responseCode);
            Debug.LogError("Error: " + request.error);
            Debug.LogError("Respuesta del servidor: " + request.downloadHandler.text);
            onError?.Invoke($"Error {request.responseCode}: {request.downloadHandler.text}");
        }
    }


    public static IEnumerator GetPet(string token, Action<PetData> onSuccess, Action<string> onError)
    {
        UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}/pet");
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            PetData pet = JsonUtility.FromJson<PetData>(request.downloadHandler.text);
            onSuccess?.Invoke(pet);
        }
        else
        {
            onError?.Invoke($"Error {request.responseCode}: {request.downloadHandler.text}");
        }
    }
}
