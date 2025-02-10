using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class Accessory
{
    public string id;
    public string name;
    public int coins;
}

public class AccessoryStoreManager : MonoBehaviour
{
    public Transform accessoryListContent;  // Content del Scroll View
    public GameObject accessoryPrefab;      // Prefab del accesorio
    public Button fetchButton;              // Botón para obtener accesorios
    private string baseUrl = "http://localhost:8080/accessories";

    private void Start()
    {
        if (fetchButton != null)
        {
            fetchButton.onClick.AddListener(FetchAccessories);
        }
        else
        {
            Debug.LogError("FetchButton no asignado en el Inspector.");
        }
    }

    public void FetchAccessories()
    {
        StartCoroutine(GetAccessories());
    }

    private IEnumerator GetAccessories()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(baseUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Accessories retrieved successfully!");
                Debug.Log($"Response JSON: {request.downloadHandler.text}");

                // Parsea el JSON correctamente
                AccessoryArray accessoriesWrapper = JsonUtility.FromJson<AccessoryArray>("{\"items\":" + request.downloadHandler.text + "}");

                if (accessoriesWrapper != null && accessoriesWrapper.items != null)
                {
                    // Limpiar la lista antes de añadir nuevos elementos
                    foreach (Transform child in accessoryListContent)
                    {
                        Destroy(child.gameObject);
                    }

                    // Agregar cada accesorio a la UI
                    foreach (var accessory in accessoriesWrapper.items)
                    {
                        AddAccessoryToUI(accessory);
                    }
                }
                else
                {
                    Debug.LogError("Error parsing accessories: response JSON might not be in expected format.");
                }
            }
            else
            {
                Debug.LogError($"Error retrieving accessories: {request.responseCode} - {request.error}");
            }
        }
    }

    private void AddAccessoryToUI(Accessory accessory)
    {
        if (accessoryPrefab == null || accessoryListContent == null)
        {
            Debug.LogError("Prefab o Content no están asignados en el Inspector.");
            return;
        }

        GameObject newAccessory = Instantiate(accessoryPrefab, accessoryListContent);
        newAccessory.transform.localScale = Vector3.one; // Evita problemas de tamaño

        // Verificar si se están encontrando correctamente los elementos en el prefab
        TMP_Text nameText = newAccessory.transform.Find("NameText")?.GetComponent<TMP_Text>();
        TMP_Text prizeText = newAccessory.transform.Find("PrizeText")?.GetComponent<TMP_Text>();

        if (nameText == null || prizeText == null)
        {
            Debug.LogError("No se encontraron los elementos en el prefab. Revisa la estructura.");
            return;
        }

        nameText.text = accessory.name;
        prizeText.text = accessory.coins + " Coins";

        Debug.Log($"Accesorio añadido a la UI: {accessory.name} - {accessory.coins} Coins");
    }


    [Serializable]
    private class AccessoryArray
    {
        public Accessory[] items;
    }
}
