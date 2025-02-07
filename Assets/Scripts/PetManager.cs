using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;


[Serializable]
public class PetData
{
    public string id;
    public string name;
    public int hungryness;
    public int cleanliness;
    public string[] accessories;
}
public class PetManager : MonoBehaviour
{
    public InputField petNameInput;
    public UIManager uiManager;
    public AuthManager authManager;
    public Text petNameText;
    public Slider hungrynessSlider;
    public Slider cleanlinessSlider;
    




    private string baseUrl = "http://localhost:8080/users"; // La misma URL de IntelliJ

    private void Start()
    {
        if (authManager == null)
            authManager = FindObjectOfType<AuthManager>();

        if (authManager == null)
        {
            Debug.LogError("AuthManager no encontrado en la escena!");
            return;
        }
    }

    public void CreatePet()
    {
        string petName = petNameInput.text.Trim();

        if (string.IsNullOrEmpty(petName))
        {
            Debug.Log("Pet name cannot be empty!");
            return;
        }

        StartCoroutine(SendPetCreationRequest(petName));
    }

    private IEnumerator SendPetCreationRequest(string petName)
    {
        string token = authManager.GetToken();

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("No token found! Redirecting to login.");
            uiManager.ShowPanel("LoginPage");
            yield break;
        }

        // Cuerpo de la solicitud con el nombre de la mascota
        string json = $"{{\"name\":\"{petName}\"}}";
        Debug.Log($"Enviando solicitud a {baseUrl} con body: {json} y token: {token}");

        // Configuración de la solicitud
        UnityWebRequest request = new UnityWebRequest(baseUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + token);

        // Enviar la solicitud
        yield return request.SendWebRequest();

        // Verificar la respuesta
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Pet created successfully!");
            uiManager.ShowPanel("PetPanel");  // Muestra el panel de la mascota
        }
        else
        {
            Debug.LogError($"Error creando mascota: {request.responseCode} - {request.error}");
            Debug.LogError($"Respuesta del servidor: {request.downloadHandler.text}");
        }
    }
    public void FetcthPet()
    {
        StartCoroutine(GetPet());
    }

    private IEnumerator GetPet()
    {
        string token = authManager.GetToken();
        using (UnityWebRequest request = UnityWebRequest.Get(baseUrl + "/pet"))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Pet retrieved successfully!");
                Debug.Log("Server Response: " + request.downloadHandler.text);

                // Parsear la respuesta JSON
                PetData pet = JsonUtility.FromJson<PetData>(request.downloadHandler.text);

                // Mostrar el nombre en la UI
                if (petNameText != null)
                {
                    petNameText.text = pet.name;
                }
                else
                {
                    Debug.LogError("petNameText no está asignado en el Inspector.");
                }

                // Actualizar los Sliders
                if (hungrynessSlider != null)
                {
                    hungrynessSlider.value = pet.hungryness;
                }
                else
                {
                    Debug.LogError("hungrynessSlider no está asignado en el Inspector.");
                }

                if (cleanlinessSlider != null)
                {
                    cleanlinessSlider.value = pet.cleanliness;
                }
                else
                {
                    Debug.LogError("cleanlinessSlider no está asignado en el Inspector.");
                }
            }
            else
            {
                Debug.LogError($"Error retrieving pet: {request.responseCode} - {request.error}");
                Debug.LogError($"Server Response: {request.downloadHandler.text}");
            }
        }
    }


}
