using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

public class PetManager : MonoBehaviour
{
    [Header("UI")]
    public InputField petNameInput;
    public Text petNameText;
    public Slider hungrynessSlider;
    public Slider cleanlinessSlider;

    [Header("Managers")]
    public UIManager uiManager;
    public AuthManager authManager;

    private void Start()
    {
        if (authManager == null)
            authManager = FindObjectOfType<AuthManager>();

        if (authManager == null)
        {
            Debug.LogError("AuthManager no encontrado en la escena.");
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
            Debug.LogError("No hay token disponible. Redirigiendo al login...");
            uiManager.ShowPanel("LoginPage");
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
                Debug.LogError("Error creando mascota: " + error);
            }
        ));
    }

    public void FetcthPet()
    {
        string token = SessionManager.GetToken();

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("No hay token disponible para obtener la mascota.");
            uiManager.ShowPanel("LoginPage");
            return;
        }

        if (PetService.Instance == null)
        {
            Debug.LogError("PetService.Instance es null. ¿Está en escena o registrado por ServiceBootstrapper?");
            return;
        }

        StartCoroutine(PetService.Instance.GetPet(
            token,
            onSuccess: pet =>
            {
                Debug.Log($"Mascota recibida: {pet.name}");

                if (petNameText != null)
                    petNameText.text = pet.name;

                if (hungrynessSlider != null)
                    hungrynessSlider.value = pet.hungryness;

                if (cleanlinessSlider != null)
                    cleanlinessSlider.value = pet.cleanliness;
            },
            onError: error =>
            {
                Debug.LogError("Error obteniendo mascota: " + error);
            }
        ));
    }
}
