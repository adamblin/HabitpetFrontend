using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaskManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField taskNameInput;
    public TMP_InputField estimatedTimeInput;
    public TMP_InputField typeInput;
    public Button createTaskButton;
    public Transform taskListContent;
    public GameObject taskPrefab;

    [Header("Managers")]
    public AuthManager authManager;

    private void Start()
    {
        if (authManager == null)
            authManager = FindObjectOfType<AuthManager>();

        if (authManager == null)
            Debug.LogError("AuthManager no encontrado.");

        if (createTaskButton != null)
        {
            createTaskButton.onClick.RemoveAllListeners();
            createTaskButton.onClick.AddListener(CreateTask);
        }
        else
        {
            Debug.LogError("CreateTaskButton no asignado.");
        }

        if (TaskService.Instance == null)
            Debug.LogError("TaskService no encontrado. Asegúrate de tenerlo en escena.");
    }

    public void CreateTask()
    {
        string taskName = taskNameInput?.text.Trim();
        string taskType = typeInput?.text.Trim();
        int estimatedTime;

        if (string.IsNullOrEmpty(taskName) || string.IsNullOrEmpty(taskType) || !int.TryParse(estimatedTimeInput?.text, out estimatedTime))
        {
            Debug.LogWarning("Rellena todos los campos correctamente.");
            return;
        }

        string token = SessionManager.GetToken();
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("Token vacío. Redirigir a login.");
            return;
        }

        StartCoroutine(TaskService.Instance.CreateTask(taskName, estimatedTime, taskType, token, (success, response) =>
        {
            if (success)
            {
                Debug.Log("Tarea creada.");
                FetchTasks();
            }
            else
            {
                Debug.LogError("Error creando tarea: " + response);
            }
        }));
    }

    public void FetchTasks()
    {
        string token = SessionManager.GetToken();
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("Token vacío. Redirigir a login.");
            return;
        }

        StartCoroutine(TaskService.Instance.GetTasks(token, tasks =>
        {
            foreach (Transform child in taskListContent)
                Destroy(child.gameObject);

            foreach (var task in tasks)
                AddTaskToUI(task);
        },
        error =>
        {
            Debug.LogError("Error obteniendo tareas: " + error);
        }));
    }

    private void AddTaskToUI(TaskData task)
    {
        if (taskPrefab == null || taskListContent == null)
        {
            Debug.LogError("Prefab o contenedor no asignado.");
            return;
        }

        GameObject newTask = Instantiate(taskPrefab, taskListContent);
        RectTransform rectTransform = newTask.GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one;
            rectTransform.anchoredPosition3D = Vector3.zero;
        }

        TMP_InputField taskNameIF = newTask.transform.Find("InputFields/TaskNameIF")?.GetComponent<TMP_InputField>();
        TMP_InputField estimatedTimeIF = newTask.transform.Find("InputFields/TimeIF")?.GetComponent<TMP_InputField>();
        TMP_InputField typeIF = newTask.transform.Find("InputFields/TypeIF")?.GetComponent<TMP_InputField>();
        TMP_InputField statusIF = newTask.transform.Find("InputFields/StatusIF")?.GetComponent<TMP_InputField>();
        Text textName = newTask.transform.Find("InputFields/TextName")?.GetComponent<Text>();

        if (taskNameIF != null) taskNameIF.text = task.name;
        if (estimatedTimeIF != null) estimatedTimeIF.text = task.estimatedTime.ToString();
        if (typeIF != null) typeIF.text = task.type;
        if (statusIF != null) statusIF.text = task.status;
        if (textName != null) textName.text = task.name;
    }
}
