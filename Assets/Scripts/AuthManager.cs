using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;



public class AuthManager : MonoBehaviour
{
    public GameObject loginPage;
    public GameObject registerPage;
    public GameObject petPanel;
    public GameObject createPetPanel; 
    public UIManager uiManager;

    public InputField loginEmail;
    public InputField loginPassword;
    public InputField registerUsername;
    public InputField registerEmail;
    public InputField registerPassword;
    public Text loginMessage;
    public Text registerMessage;
    public Toggle loginRememberMeToggle; 
    public Toggle registerRememberMeToggle;

    private string sessionToken;
    private string baseUrl = "http://localhost:8080/auth";

    private void Start()
    {
        string token = GetToken();

        if (!string.IsNullOrEmpty(token))
        {
            Debug.Log("Token encontrado. Verificando si el usuario tiene mascota...");
            StartCoroutine(CheckUserHasPet());
        }
        else
        {
            Debug.Log("No hay token guardado o usuario no marc� 'Remember Me'. Mostrando pantalla de login.");
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
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            AuthResponse response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);
            if (!string.IsNullOrEmpty(response.token))
            {
                bool rememberMe = registerRememberMeToggle != null && registerRememberMeToggle.isOn;
                SaveToken(response.token, rememberMe);
                Debug.Log("Usuario registrado correctamente y token guardado.");
                StartCoroutine(CheckUserHasPet());
            }
            else
            {
                Debug.LogError("El backend no devolvi� un token.");
            }
        }
        else
        {
            Debug.LogError("Error en registro: " + request.error);
        }
    }



    private IEnumerator LoginRequest()
    {
        string json = $"{{\"username\":\"{loginEmail.text}\", \"password\":\"{loginPassword.text}\"}}";
        UnityWebRequest request = new UnityWebRequest(baseUrl + "/login", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            AuthResponse response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);
            if (!string.IsNullOrEmpty(response.token))
            {
                bool rememberMe = loginRememberMeToggle != null && loginRememberMeToggle.isOn;
                SaveToken(response.token, rememberMe);
                Debug.Log("Usuario logueado correctamente y token guardado.");
                StartCoroutine(CheckUserHasPet());
            }
            else
            {
                Debug.LogError("El backend no devolvi� un token.");
            }
        }
        else
        {
            Debug.LogError("Error en login: " + request.error);
        }
    }



   

    private void SaveToken(string token, bool rememberMe)
    {
        if (!string.IsNullOrEmpty(token))
        {
            if (rememberMe)
            {
                PlayerPrefs.SetString("authToken", token);
                PlayerPrefs.SetInt("rememberMe", 1);
                PlayerPrefs.Save();
                Debug.Log("Token guardado en PlayerPrefs (sesi�n guardada).");
            }
            else
            {
                sessionToken = token; // Guarda el token solo en memoria
                PlayerPrefs.DeleteKey("authToken"); // No lo guarda en PlayerPrefs
                PlayerPrefs.DeleteKey("rememberMe");
                PlayerPrefs.Save();
                Debug.Log("Sesi�n iniciada sin recordar, el token no se guardar�.");
            }
        }
    }




    public string GetToken()
    {
        if (sessionToken != null)
        {
            return sessionToken; 
        }
        else if (PlayerPrefs.HasKey("rememberMe") && PlayerPrefs.GetInt("rememberMe") == 1)
        {
            return PlayerPrefs.HasKey("authToken") ? PlayerPrefs.GetString("authToken") : null;
        }
        else
        {
            return null;
        }
    }




    public void ClearToken()
    {
        sessionToken = null; 
        PlayerPrefs.DeleteKey("authToken");
        PlayerPrefs.DeleteKey("rememberMe");
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

        Debug.Log("Enviando petici�n GET con token: " + token);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            Debug.Log("C�digo de respuesta: " + request.responseCode);
            Debug.Log("Cuerpo de la respuesta: '" + request.downloadHandler.text + "'");

            if (request.result == UnityWebRequest.Result.Success)
            {
                if (string.IsNullOrEmpty(request.downloadHandler.text) || request.responseCode == 204)
                {
                    Debug.Log("El usuario no tiene mascota. Mostrando pantalla de creaci�n de mascota.");
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

    public string GetUsername()
    {
        string token = GetToken();
        if (string.IsNullOrEmpty(token)) return null;

        string[] parts = token.Split('.');
        if (parts.Length != 3) return null;

        string payload = parts[1];
        payload = payload.Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
            case 1: payload += "==="; break;
        }

        try
        {
            string json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            var data = JsonUtility.FromJson<JWTSubWrapper>(json);
            return data.sub;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error decoding JWT: {e.Message}");
            return null;
        }
    }

    private string DecodeBase64(string base64)
    {
        base64 = base64.Replace('-', '+').Replace('_', '/'); // Normalizar Base64 URL
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        byte[] data = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(data);
    }

    private string ExtractUsernameFromJson(string json)
    {
        int startIndex = json.IndexOf("\"sub\":\"") + 7; // Busca `"sub":"` y extrae el valor
        if (startIndex == 6) return null; // No encontrado

        int endIndex = json.IndexOf("\"", startIndex);
        if (endIndex == -1) return null; // No encontrado

        return json.Substring(startIndex, endIndex - startIndex);
    }

    [Serializable]
    private class JWTSubWrapper
    {
        public string sub;
    }
}

[System.Serializable]
public class AuthResponse
{
    public string token;
}



