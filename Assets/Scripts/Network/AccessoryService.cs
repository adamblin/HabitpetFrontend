using System;
using System.Collections;
using UnityEngine;

public class AccessoryService : MonoBehaviour
{
    public static AccessoryService Instance { get; private set; }

    private static string AccessoriesUrl => Endpoints.AccessoriesList();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public IEnumerator GetAccessories(Action<AccessoryData[]> onSuccess, Action<string> onError)
    {
        yield return ApiClient.GetArray<AccessoryData>(
            url: AccessoriesUrl,
            onSuccess: onSuccess,
            onError: onError,
            withAuth: false
        );
    }
}
