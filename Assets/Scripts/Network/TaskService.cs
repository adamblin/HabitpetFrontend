using System;
using System.Collections;
using UnityEngine;

public class TaskService : MonoBehaviour
{
    public static TaskService Instance { get; private set; }

    private static string TasksBase => Endpoints.Tasks;              
    private static string TasksOfUserUrl => Endpoints.TasksOfCurrentUser(); 

    [Serializable]
    private class TaskCreateDto
    {
        public string name;
        public int estimatedTime;
        public string type;
        public string status;
        public TaskCreateDto(string name, int estimatedTime, string type, string status)
        {
            this.name = name;
            this.estimatedTime = estimatedTime;
            this.type = type;
            this.status = status;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public IEnumerator CreateTask(string taskName, int estimatedTime, string taskType, string token, Action<bool, string> callback)
    {
        if (string.IsNullOrWhiteSpace(taskName) || string.IsNullOrWhiteSpace(taskType))
        {
            callback?.Invoke(false, "Nombre y tipo son obligatorios.");
            yield break;
        }
        if (string.IsNullOrEmpty(token))
        {
            callback?.Invoke(false, "Token vacío o nulo.");
            yield break;
        }

        var dto = new TaskCreateDto(taskName, estimatedTime, taskType, "ToDo");

        yield return ApiClient.Post<string>(
            url: TasksBase,
            data: dto,
            onSuccess: _ => callback?.Invoke(true, null),
            onError: err => callback?.Invoke(false, err),
            withAuth: true
        );
    }

    public IEnumerator GetTasks(string token, Action<TaskData[]> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrEmpty(token))
        {
            onError?.Invoke("Token vacío o nulo.");
            yield break;
        }

        yield return ApiClient.GetArray<TaskData>(
            url: TasksOfUserUrl,
            onSuccess: onSuccess,
            onError: onError,
            withAuth: true
        );
    }
}
