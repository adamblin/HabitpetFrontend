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
        if (fetchButton != null) fetchButton.onClick.AddListener(FetchAccessories);
        else Debug.LogWarning("FetchButton no asignado.");

        if (AccessoryService.Instance == null)
            Debug.LogWarning("AccessoryService no encontrado en escena.");

        FetchAccessories();
    }

    public void FetchAccessories()
    {
        StartCoroutine(AccessoryService.Instance.GetAccessories(
            accessories =>
            {
                foreach (Transform child in accessoryListContent) Destroy(child.gameObject);
                foreach (var acc in accessories) AddAccessoryToUI(acc);
            },
            error => Debug.LogWarning("Error al obtener accesorios: " + error)
        ));
    }

    private void AddAccessoryToUI(AccessoryData accessory)
    {
        if (accessoryPrefab == null || accessoryListContent == null)
        {
            Debug.LogWarning("Prefab o Content no asignado.");
            return;
        }

        GameObject go = Instantiate(accessoryPrefab, accessoryListContent);
        go.transform.localScale = Vector3.one;

        var nameText = go.transform.Find("NameText")?.GetComponent<TMP_Text>();
        var prizeText = go.transform.Find("PrizeText")?.GetComponent<TMP_Text>();

        if (nameText == null || prizeText == null)
        {
            Debug.LogWarning("Elementos del prefab no encontrados.");
            return;
        }

        nameText.text = accessory.name;
        prizeText.text = accessory.coins + " Coins";
    }
}
