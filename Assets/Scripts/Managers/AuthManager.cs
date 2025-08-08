using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AuthManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject loginPage;
    public GameObject registerPage;
    public GameObject petPanel;
    public GameObject createPetPanel;

    [Header("UI Elements")]
    public InputField loginUsername;
    public InputField loginPassword;
    public Toggle loginRememberMeToggle;
    public Text loginMessage;

    public InputField registerUsername;
    public InputField registerEmail;
    public InputField registerPassword;
    public Toggle registerRememberMeToggle;
    public Text registerMessage;

    [Header("Managers")]
    public UIManager uiManager;

    private void Start()
    {
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();

        if (string.IsNullOrEmpty(SessionManager.GetToken()))
        {
            Debug.Log("No hay token guardado. Mostrando pantalla de login.");
            uiManager.ShowPanel("Login");
        }
        else
        {
            Debug.Log("Token encontrado. Verificando si el usuario tiene mascota...");
            StartCoroutine(CheckUserHasPet());
        }
    }

    public void Login()
    {
        string email = loginUsername.text.Trim();
        string password = loginPassword.text.Trim();
        bool remember = loginRememberMeToggle?.isOn ?? false;

        StartCoroutine(AuthService.Instance.Login(email, password, remember,
            onSuccess: () => {
                Debug.Log("Login exitoso.");
                StartCoroutine(CheckUserHasPet());
            },
            onError: error => {
                Debug.LogError("Error en login: " + error);
                if (loginMessage != null) loginMessage.text = "Error: " + error;
            }
        ));
    }

    public void Register()
    {
        string username = registerUsername.text.Trim();
        string email = registerEmail.text.Trim();
        string password = registerPassword.text.Trim();
        bool remember = registerRememberMeToggle?.isOn ?? false;

        StartCoroutine(AuthService.Instance.Register(username, email, password, remember,
            onSuccess: () => {
                Debug.Log("Registro exitoso.");
                StartCoroutine(CheckUserHasPet());
            },
            onError: error => {
                Debug.LogError("Error en registro: " + error);
                if (registerMessage != null) registerMessage.text = "Error: " + error;
            }
        ));
    }

    private IEnumerator CheckUserHasPet()
    {
        string token = SessionManager.GetToken();
        string url = "http://localhost:8080/users/pet";

        yield return ApiClient.Get<PetData>(url,
            onSuccess: pet =>
            {
                if (pet == null)
                {
                    Debug.Log("Usuario sin mascota. Mostrando CreatePet.");
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
                Debug.LogError("Error al verificar mascota: " + error);
                uiManager.ShowPanel("Login");
            }
        );
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
