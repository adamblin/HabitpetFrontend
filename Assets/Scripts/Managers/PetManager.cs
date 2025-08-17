using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

public class PetManager : MonoBehaviour
{
    [Header("UI")]
    public InputField petNameInput;
    public Slider hungrynessSlider;
    public Slider cleanlinessSlider;
    public Slider happynessSlider;

    [Header("Managers")]
    public UIManager uiManager;
    public AuthManager authManager;

    private void Start()
    {
        if (authManager == null) authManager = FindObjectOfType<AuthManager>();
        if (authManager == null)
        {
            Debug.LogWarning("AuthManager no encontrado en la escena.");
            return;
        }
    }

    public void CreatePet()
    {
        string petName = petNameInput.text.Trim();
        if (string.IsNullOrEmpty(petName))
        {
            Debug.LogWarning("El nombre de la mascota no puede estar vacío.");
            return;
        }

        string token = SessionManager.GetToken();
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogWarning("Sin token. Redirigiendo a Login.");
            uiManager.ShowPanel("Login");
            return;
        }

        StartCoroutine(PetService.Instance.CreatePet(
            petName,
            token,
            onSuccess: () =>
            {
                Debug.Log("Mascota creada correctamente.");
                uiManager.ShowPanel("PetPanel");
            },
            onError: error =>
            {
                Debug.LogWarning("Error creando mascota: " + error);
            }
        ));
    }

    public void FetchPet() 
    {
        string token = SessionManager.GetToken();
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogWarning("Sin token para obtener mascota.");
            uiManager.ShowPanel("Login");
            return;
        }

        if (PetService.Instance == null)
        {
            Debug.LogWarning("PetService.Instance es null. ¿Está en escena?");
            return;
        }

        StartCoroutine(PetService.Instance.GetPet(
            token,
            onSuccess: pet =>
            {
                if (pet == null)
                {
                    uiManager.ShowPanel("CreatePet");
                    return;
                }

                if (hungrynessSlider != null) hungrynessSlider.value = pet.satiated;
                if (cleanlinessSlider != null) cleanlinessSlider.value = pet.cleanliness;
                if (happynessSlider != null) happynessSlider.value = pet.happyness;

                Debug.Log("Mascota recibida: " + pet.name);
            },
            onError: error =>
            {
                Debug.LogWarning("Error obteniendo mascota: " + error);
            }
        ));
    }
}
