using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject[] panels;
    public GameObject defaultPanel; // Panel que debe estar activo por defecto

    public void ShowPanel(GameObject panelToShow)
    {
        foreach (GameObject panel in panels)
        {
            panel.SetActive(panel == panelToShow);
        }

        if (panelToShow == null && defaultPanel != null)
        {
            defaultPanel.SetActive(true);
        }
    }
}
