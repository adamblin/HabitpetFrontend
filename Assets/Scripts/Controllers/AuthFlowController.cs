using UnityEngine;
using System.Collections;

public class AuthFlowController : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject petPanel;

    public GameObject[] managersToEnable;

    private void Start()
    {
        if (!SessionManager.HasToken())
        {
            Debug.LogWarning("[AuthFlow] Token no disponible. ¿Usuario no logueado?");
            loginPanel.SetActive(true);
            petPanel.SetActive(false);
            return;
        }

        StartCoroutine(WaitForTokenThenPetThenEnableManagers());
    }

    private IEnumerator WaitForTokenThenPetThenEnableManagers()
    {
        string token = SessionManager.GetToken();
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("[AuthFlow] Token está vacío. Abortando flujo.");
            yield break;
        }

        yield return new WaitUntil(() => PetService.Instance.HasLoadedPet); 

        Debug.Log("[AuthFlow] Usuario autenticado y mascota cargada. Activando managers...");

        foreach (var manager in managersToEnable)
        {
            if (manager != null) manager.SetActive(true);
        }

        petPanel.SetActive(true);
        loginPanel.SetActive(false);
    }
}
