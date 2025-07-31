using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class TaskService : MonoBehaviour
{
    public static TaskService Instance { get; private set; }

    private string baseUrl = "http://localhost:8080/tasks";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public IEnumerator CreateTask(string taskName, int estimatedTime, string taskType, string token, Action<bool, string> callback)
    {
        string json = $"{{\"name\":\"{taskName}\",\"estimatedTime\":{estimatedTime},\"type\":\"{taskType}\",\"status\":\"ToDo\"}}";

        UnityWebRequest request = new UnityWebRequest(baseUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        bool success = request.result == UnityWebRequest.Result.Success;
        callback?.Invoke(success, request.downloadHandler.text);
    }

    public IEnumerator GetTasks(string token, Action<TaskData[]> onSuccess, Action<string> onError)
    {
        UnityWebRequest request = UnityWebRequest.Get(baseUrl + "/user");
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            TaskData[] tasks = JsonHelper.FromJson<TaskData>(request.downloadHandler.text);
            onSuccess?.Invoke(tasks);
        }
        else
        {
            onError?.Invoke(request.downloadHandler.text);
        }
    }
}
