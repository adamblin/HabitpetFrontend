using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class PetService : MonoBehaviour
{
    public static PetService Instance { get; private set; }

    private const string baseUrl = "http://localhost:8080/pets";
    private PetData currentPet;
    public bool HasLoadedPet => currentPet != null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public IEnumerator CreatePet(string petName, string token, Action onSuccess, Action<string> onError)
    {
        string json = $"{{\"name\":\"{petName}\"}}";

        UnityWebRequest request = new UnityWebRequest(baseUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke();
        }
        else
        {
            string errorMsg = string.IsNullOrEmpty(request.downloadHandler.text) ? request.error : request.downloadHandler.text;
            onError?.Invoke(errorMsg);
        }
    }

    public IEnumerator GetPet(string token, Action<PetData> onSuccess, Action<string> onError)
    {
        UnityWebRequest request = UnityWebRequest.Get(baseUrl + "/user");
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                PetData pet = JsonUtility.FromJson<PetData>(request.downloadHandler.text);
                currentPet = pet;
                onSuccess?.Invoke(pet);
            }
            catch (Exception e)
            {
                Debug.LogError("Error parseando la mascota: " + e.Message);
                onError?.Invoke("Error al parsear la mascota");
            }
        }
        else
        {
            string errorMsg = string.IsNullOrEmpty(request.downloadHandler.text) ? request.error : request.downloadHandler.text;
            onError?.Invoke(errorMsg);
        }
    }
}
