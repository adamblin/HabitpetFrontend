using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AuthManager : MonoBehaviour
{
    public GameObject loginPage;
    public GameObject registerPage;
    public InputField loginEmail;
    public InputField loginPassword;
    public InputField registerUsername;
    public InputField registerEmail;
    public InputField registerPassword;
    public Text loginMessage;
    public Text registerMessage;

    private string baseUrl = "http://localhost:8080/auth";  

    //  Login
    public void Login()
    {
        StartCoroutine(LoginRequest());
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
            PlayerPrefs.SetString("authToken", response.token); 
            loginMessage.text = "Login successful!";
            Debug.Log("Token recibido: " + response.token);
        }
        else
        {
            loginMessage.text = "Login failed!";
            Debug.LogError("Error en login: " + request.error);
        }
    }

    //  Register
    public void Register()
    {
        StartCoroutine(RegisterRequest());
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
            registerMessage.text = "Register successful!";
            Debug.Log("Usuario registrado correctamente");
        }
        else
        {
            registerMessage.text = "Register failed!";
            Debug.LogError("Error en registro: " + request.error);
        }
    }

 
    public void ShowLogin()
    {
        loginPage.SetActive(true);
        registerPage.SetActive(false);
    }

    public void ShowRegister()
    {
        loginPage.SetActive(false);
        registerPage.SetActive(true);
    }
    private void SaveToken(string token)
    {
        PlayerPrefs.SetString("authToken", token);
        PlayerPrefs.Save();
    }

   
    public string GetToken()
    {
        if (PlayerPrefs.HasKey("authToken"))
        {
            return PlayerPrefs.GetString("authToken");
        }
        else
        {
            Debug.LogWarning("No token found. User might not be logged in.");
            return null;
        }
    }

   
    public void ClearToken()
    {
        if (PlayerPrefs.HasKey("authToken"))
        {
            PlayerPrefs.DeleteKey("authToken");
            Debug.Log("Token cleared. User logged out.");
        }
    }
}


[System.Serializable]
public class AuthResponse
{
    public string token;
}

