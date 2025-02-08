using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public CanvasGroup loginPage, registerPage, createPet, petPanel, taskPanel, friendsPanel, accessoryPanel, getPanel;

    private Dictionary<string, CanvasGroup> panels;
    private CanvasGroup activePanel; // Guardar el panel actualmente activo

    private void Awake()
    {
        panels = new Dictionary<string, CanvasGroup>
        {
            { "Login", loginPage },
            { "Register", registerPage },
            { "CreatePet", createPet },
            { "PetPanel", petPanel },
            { "TaskPanel", taskPanel },
            {"FriendsPanel",friendsPanel },
            {"AccessoryPanel",accessoryPanel },
            {"GetTaskPanel",getPanel }
        };

        // Activamos solo el panel inicial
        foreach (var panel in panels.Values)
        {
            panel.gameObject.SetActive(false);
            panel.alpha = 0;
        }

        // Definir el panel activo al inicio
        activePanel = panels["Login"];
        activePanel.gameObject.SetActive(true);
        activePanel.alpha = 1;
    }

    public void ShowPanel(string panelName)
    {
        if (!panels.ContainsKey(panelName) || panels[panelName] == activePanel)
            return; // Evitar cambiar al mismo panel activo

        StartCoroutine(SwitchPanel(panels[panelName]));
    }

    private IEnumerator SwitchPanel(CanvasGroup newPanel)
    {
        if (activePanel != null)
            yield return StartCoroutine(FadeOut(activePanel)); // Espera a que termine FadeOut

        newPanel.gameObject.SetActive(true);
        yield return StartCoroutine(FadeIn(newPanel)); // Aplica FadeIn al nuevo panel
        activePanel = newPanel;
    }

    private IEnumerator FadeOut(CanvasGroup panel)
    {
        for (float t = 1f; t >= 0; t -= Time.deltaTime * 3)
        {
            panel.alpha = t;
            yield return null;
        }
        panel.interactable = false;
        panel.blocksRaycasts = false;
        panel.gameObject.SetActive(false);
    }


    private IEnumerator FadeIn(CanvasGroup panel)
    {
        panel.gameObject.SetActive(true);
        panel.interactable = true;
        panel.blocksRaycasts = true;
        panel.alpha = 0;
        for (float t = 0; t <= 1f; t += Time.deltaTime * 3)
        {
            panel.alpha = t;
            yield return null;
        }
    }

}
