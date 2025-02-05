using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AuthManager : MonoBehaviour
{
    public GameObject loginPage;
    public GameObject registerPage;
    public GameObject petPanel;
    public GameObject createPetPanel; // Asegurar que está asignado en el inspector de Unity
    public UIManager uiManager;

    public InputField loginEmail;
    public InputField loginPassword;
    public InputField registerUsername;
    public InputField registerEmail;
    public InputField registerPassword;
    public Text loginMessage;
    public Text registerMessage;

    private string baseUrl = "http://localhost:8080/auth";

    private void Start()
    {
        string token = GetToken();

        if (!string.IsNullOrEmpty(token))
        {
            Debug.Log("Token encontrado. Verificando si el usuario tiene mascota...");
            StartCoroutine(CheckUserHasPet()); // Ahora sí verifica si tiene mascota
        }
        else
        {
            Debug.Log("No hay token guardado. Mostrando pantalla de login.");
            uiManager.ShowPanel("Login");
        }
    }

    public void Register()
    {
        StartCoroutine(RegisterRequest());
    }

    public void Login()
    {
        StartCoroutine(LoginRequest());
    }

    private IEnumerator RegisterRequest()
    {
        string json = $"{{\"username\":\"{registerUsername.text}\", \"email\":\"{registerEmail.text}\", \"password\":\"{registerPassword.text}\"}}";
        UnityWebRequest request = new UnityWebRequest(baseUrl + "/register", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            AuthResponse response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);
            if (response != null && !string.IsNullOrEmpty(response.token))
            {
                SaveToken(response.token);
                Debug.Log("Usuario registrado correctamente y token guardado.");
                StartCoroutine(CheckUserHasPet()); // Ahora verificamos si tiene mascota
            }
            else
            {
                Debug.LogError("El backend no devolvió un token.");
            }
        }
        else
        {
            Debug.LogError("Error en registro: " + request.error);
        }
    }

    private IEnumerator LoginRequest()
    {
        string json = $"{{\"email\":\"{loginEmail.text}\", \"password\":\"{loginPassword.text}\"}}";
        UnityWebRequest request = new UnityWebRequest(baseUrl + "/login", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            AuthResponse response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);
            if (!string.IsNullOrEmpty(response.token))
            {
                SaveToken(response.token);
                Debug.Log("Usuario logueado correctamente y token guardado.");
                StartCoroutine(CheckUserHasPet()); // Ahora verificamos si tiene mascota
            }
            else
            {
                Debug.LogError("El backend no devolvió un token.");
            }
        }
        else
        {
            Debug.LogError("Error en login: " + request.error);
        }
    }

    private void SaveToken(string token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            PlayerPrefs.SetString("authToken", token);
            PlayerPrefs.Save();
            Debug.Log("Token guardado en PlayerPrefs.");
        }
    }

    public string GetToken()
    {
        return PlayerPrefs.HasKey("authToken") ? PlayerPrefs.GetString("authToken") : null;
    }

    public void ClearToken()
    {
        PlayerPrefs.DeleteKey("authToken");
        PlayerPrefs.Save();
        Debug.Log("Token eliminado. Usuario deslogueado.");
        uiManager.ShowPanel("Login");
    }

    private IEnumerator CheckUserHasPet()
    {
        string url = "http://localhost:8080/users/pet";
        string token = GetToken();

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogWarning("No hay token. Redirigiendo...");
            StartCoroutine(RedirectToLogin());
            yield break;
        }

        Debug.Log("Enviando petición GET con token: " + token);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            Debug.Log("Código de respuesta: " + request.responseCode);
            Debug.Log("Cuerpo de la respuesta: '" + request.downloadHandler.text + "'");

            if (request.result == UnityWebRequest.Result.Success)
            {
                if (string.IsNullOrEmpty(request.downloadHandler.text) || request.responseCode == 204)
                {
                    Debug.Log("El usuario no tiene mascota. Mostrando pantalla de creación de mascota.");
                    uiManager.ShowPanel("CreatePet");
                }
                else
                {
                    Debug.Log("Mascota encontrada. Mostrando pantalla de mascota.");
                    uiManager.ShowPanel("PetPanel");
                }
            }
            else
            {
                Debug.LogError("Error verificando mascota: " + request.responseCode + " " + request.error);
            }
        }
    }


    private IEnumerator RedirectToLogin()
    {
        yield return new WaitForSeconds(1f);
        uiManager.ShowPanel("Login");
    }
}

[System.Serializable]
public class AuthResponse
{
    public string token;
}
