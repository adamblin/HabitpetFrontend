using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Text;

[Serializable]
public class TaskData
{
    public string id;
    public string name;
    public int estimatedTime;
    public string type;
    public string status;
}

public class TaskManager : MonoBehaviour
{
    public TMP_InputField taskNameInput;
    public TMP_InputField estimatedTimeInput;
    public TMP_InputField typeInput;
    public Button createTaskButton;
    public Transform taskListContent;
    public GameObject taskPrefab;
    private string baseUrl = "http://localhost:8080/tasks";
    public AuthManager authManager;

    private void Start()
    {
        if (authManager == null)
        {
            Debug.LogError("AuthManager no encontrado en la escena!");
            return;
        }

        if (createTaskButton != null)
        {
            createTaskButton.onClick.RemoveAllListeners();
            createTaskButton.onClick.AddListener(CreateTask);
        }
        else
        {
            Debug.LogError("CreateTaskButton no asignado en el Inspector.");
        }
    }

    public void CreateTask()
    {
        string taskName = taskNameInput.text.Trim();
        string taskType = typeInput.text.Trim();
        int estimatedTime;

        if (string.IsNullOrEmpty(taskName) || string.IsNullOrEmpty(taskType) || !int.TryParse(estimatedTimeInput.text, out estimatedTime))
        {
            Debug.Log("Error: Asegúrate de que todos los campos estén llenos y el tiempo estimado sea un número.");
            return;
        }

        StartCoroutine(SendTaskCreationRequest(taskName, estimatedTime, taskType));
    }

    private IEnumerator SendTaskCreationRequest(string taskName, int estimatedTime, string taskType)
    {
        string token = authManager?.GetToken();

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("No token found! Redirecting to login.");
            yield break;
        }

        string json = $"{{\"name\":\"{taskName}\",\"estimatedTime\":{estimatedTime},\"type\":\"{taskType}\",\"status\":\"ToDo\"}}";
        Debug.Log($"Enviando solicitud a {baseUrl} con body: {json} y token: {token}");

        UnityWebRequest request = new UnityWebRequest(baseUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Task created successfully!");
            FetchTasks();
        }
        else
        {
            Debug.LogError($"Error creando tarea: {request.responseCode} - {request.error}");
            Debug.LogError($"Respuesta del servidor: {request.downloadHandler.text}");
        }
    }

    public void FetchTasks()
    {
        StartCoroutine(GetTasks());
    }

    private IEnumerator GetTasks()
    {
        string token = authManager?.GetToken();

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("No token found! Redirecting to login.");
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequest.Get(baseUrl + "/user"))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Tasks retrieved successfully!");
                Debug.Log($"JSON recibido del servidor: {request.downloadHandler.text}");

                TaskData[] tasks = JsonHelper.FromJson<TaskData>(request.downloadHandler.text);

                foreach (Transform child in taskListContent)
                {
                    Destroy(child.gameObject);
                }

                foreach (var task in tasks)
                {
                    AddTaskToUI(task);
                }
            }
            else
            {
                Debug.LogError($"Error retrieving tasks: {request.responseCode} - {request.error}");
                Debug.LogError($"Server Response: {request.downloadHandler.text}");
            }
        }
    }

    private void AddTaskToUI(TaskData task)
    {
        if (taskPrefab == null || taskListContent == null)
        {
            Debug.LogError("taskPrefab o taskListContent no está asignado en el Inspector.");
            return;
        }

        GameObject newTask = Instantiate(taskPrefab, taskListContent);
        RectTransform rectTransform = newTask.GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one; // Evita que aparezca más pequeño
            rectTransform.anchoredPosition3D = Vector3.zero;
        }

        TMP_Text taskNameText = newTask.transform.Find("TaskNameText")?.GetComponent<TMP_Text>();
        TMP_Text taskTypeText = newTask.transform.Find("TaskTypeText")?.GetComponent<TMP_Text>();
        TMP_Text estimatedTimeText = newTask.transform.Find("EstimatedTimeText")?.GetComponent<TMP_Text>();
        TMP_Text taskStatusText = newTask.transform.Find("TaskStatusText")?.GetComponent<TMP_Text>();

        if (taskNameText != null) taskNameText.text = task.name;
        if (taskTypeText != null) taskTypeText.text = task.type;
        if (estimatedTimeText != null) estimatedTimeText.text = $"Time: {task.estimatedTime} min";
        if (taskStatusText != null) taskStatusText.text = $"Status: {task.status}";

        Debug.Log($"Task {task.name} added to UI.");
    }

}
