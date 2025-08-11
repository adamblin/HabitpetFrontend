using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();

        var token = SessionManager.GetToken();
        if (string.IsNullOrEmpty(token))
        {
            Debug.Log("Sin token. Mostrando Login.");
            uiManager.ShowPanel("Login");
        }
        else
        {
            Debug.Log("Token presente. Comprobando mascota…");
            StartCoroutine(CheckUserHasPet());
        }
    }

    public void Login()
    {
        var email = loginUsername.text.Trim();
        var password = loginPassword.text.Trim();
        var remember = loginRememberMeToggle?.isOn ?? false;

        StartCoroutine(AuthService.Instance.Login(email, password, remember,
            onSuccess: () => {
                Debug.Log("Login OK.");
                StartCoroutine(CheckUserHasPet());
            },
            onError: err => {
                Debug.LogWarning("Error en login: " + err);
                if (loginMessage != null) loginMessage.text = "Error: " + err;
            }
        ));
    }

    public void Register()
    {
        var username = registerUsername.text.Trim();
        var email = registerEmail.text.Trim();
        var password = registerPassword.text.Trim();
        var remember = registerRememberMeToggle?.isOn ?? false;

        StartCoroutine(AuthService.Instance.Register(username, email, password, remember,
            onSuccess: () => {
                Debug.Log("Registro OK.");
                StartCoroutine(CheckUserHasPet());
            },
            onError: err => {
                Debug.LogWarning("Error en registro: " + err);
                if (registerMessage != null) registerMessage.text = "Error: " + err;
            }
        ));
    }

    private IEnumerator CheckUserHasPet()
    {
        var token = SessionManager.GetToken();
        if (string.IsNullOrEmpty(token))
        {
            uiManager.ShowPanel("Login");
            yield break;
        }

        yield return PetService.Instance.GetPet(token,
            onSuccess: pet =>
            {
                if (pet == null)
                {
                    Debug.Log("Sin mascota. Mostrar CreatePet.");
                    uiManager.ShowPanel("CreatePet");
                }
                else
                {
                    Debug.Log("Mascota encontrada. Mostrar PetPanel.");
                    uiManager.ShowPanel("PetPanel");
                }
            },
            onError: err =>
            {
                Debug.LogWarning("Error al verificar mascota: " + err);
                uiManager.ShowPanel("Login");
            }
        );
    }

    public void Logout()
    {
        SessionManager.ClearToken();
        uiManager.ShowPanel("Login");
    }

    public string GetUsername() => JwtUtils.GetUsernameFromToken(SessionManager.GetToken());
}
