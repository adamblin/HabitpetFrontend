using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class AccessoryService : MonoBehaviour
{
    public static AccessoryService Instance { get; private set; }

    private string baseUrl = "http://localhost:8080/accessories";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public IEnumerator GetAccessories(Action<AccessoryData[]> onSuccess, Action<string> onError)
    {
        UnityWebRequest request = UnityWebRequest.Get(baseUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = "{\"items\":" + request.downloadHandler.text + "}";
            AccessoryArray parsed = JsonUtility.FromJson<AccessoryArray>(json);

            if (parsed?.items != null)
            {
                onSuccess?.Invoke(parsed.items);
            }
            else
            {
                onError?.Invoke("Formato JSON inesperado");
            }
        }
        else
        {
            onError?.Invoke($"Error {request.responseCode}: {request.error}");
        }
    }
}
