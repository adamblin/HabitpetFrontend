using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;



public class AccessoryStoreManager : MonoBehaviour
{
    public Transform accessoryListContent;
    public GameObject accessoryPrefab;
    public Button fetchButton;

    private void Start()
    {
        if (fetchButton != null)
        {
            fetchButton.onClick.AddListener(FetchAccessories);
        }
        else
        {
            Debug.LogError("FetchButton no asignado.");
        }

        if (AccessoryService.Instance == null)
        {
            Debug.LogError("AccessoryService no encontrado en escena.");
        }
    }

    public void FetchAccessories()
    {
        StartCoroutine(AccessoryService.Instance.GetAccessories(OnAccessoriesReceived, OnAccessoriesError));
    }

    private void OnAccessoriesReceived(AccessoryData[] accessories)
    {
        foreach (Transform child in accessoryListContent)
        {
            Destroy(child.gameObject);
        }

        foreach (var accessory in accessories)
        {
            AddAccessoryToUI(accessory);
        }
    }

    private void OnAccessoriesError(string error)
    {
        Debug.LogError("Error al obtener accesorios: " + error);
    }

    private void AddAccessoryToUI(AccessoryData accessory)
    {
        if (accessoryPrefab == null || accessoryListContent == null)
        {
            Debug.LogError("Prefab o Content no asignado.");
            return;
        }

        GameObject newAccessory = Instantiate(accessoryPrefab, accessoryListContent);
        newAccessory.transform.localScale = Vector3.one;

        TMP_Text nameText = newAccessory.transform.Find("NameText")?.GetComponent<TMP_Text>();
        TMP_Text prizeText = newAccessory.transform.Find("PrizeText")?.GetComponent<TMP_Text>();

        if (nameText == null || prizeText == null)
        {
            Debug.LogError("Elementos del prefab no encontrados.");
            return;
        }

        nameText.text = accessory.name;
        prizeText.text = accessory.coins + " Coins";

        Debug.Log($"Accesorio añadido a la UI: {accessory.name} - {accessory.coins} Coins");
    }
}
