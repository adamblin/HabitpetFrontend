using System.Collections;
using UnityEngine;
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

    private void Start()
    {
        string token = SessionManager.GetToken();

        if (!string.IsNullOrEmpty(token))
        {
            Debug.Log("Token encontrado. Verificando si el usuario tiene mascota...");
            StartCoroutine(CheckUserHasPet());
        }
        else
        {
            Debug.Log("No hay token guardado. Mostrando pantalla de login.");
            uiManager.ShowPanel("Login");
        }
    }

    public void Register()
    {
        StartCoroutine(AuthService.Register(
            registerUsername.text,
            registerEmail.text,
            registerPassword.text,
            registerRememberMeToggle.isOn,
            onSuccess: () => {
                Debug.Log("Registro exitoso");
                StartCoroutine(CheckUserHasPet());
            },
            onError: error => registerMessage.text = "Error: " + error
        ));
    }

    public void Login()
    {
        StartCoroutine(AuthService.Login(
            loginEmail.text,
            loginPassword.text,
            loginRememberMeToggle.isOn,
            onSuccess: () => {
                Debug.Log("Login exitoso");
                StartCoroutine(CheckUserHasPet());
            },
            onError: error => loginMessage.text = "Error: " + error
        ));
    }

    private IEnumerator CheckUserHasPet()
    {
        string token = SessionManager.GetToken();
        string url = "http://localhost:8080/users/pet";

        yield return ApiClient.Get<string>(url,
            onSuccess: response =>
            {
                if (string.IsNullOrEmpty(response))
                {
                    Debug.Log("El usuario no tiene mascota. Mostrando CreatePet.");
                    uiManager.ShowPanel("CreatePet");
                }
                else
                {
                    Debug.Log("Mascota encontrada. Mostrando PetPanel.");
                    uiManager.ShowPanel("PetPanel");
                }
            },
            onError: error =>
            {
                Debug.LogError("Error verificando mascota: " + error);
                uiManager.ShowPanel("Login");
            });
    }

    public void Logout()
    {
        SessionManager.ClearToken();
        uiManager.ShowPanel("Login");
    }

    public string GetUsername()
    {
        return JwtUtils.GetUsernameFromToken(SessionManager.GetToken());
    }
}
