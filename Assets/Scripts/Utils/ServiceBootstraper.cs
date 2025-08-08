using UnityEngine;

public class ServiceBootstrapper : MonoBehaviour
{
    void Awake()
    {
        EnsureService<AuthService>("AuthService");
        EnsureService<TaskService>("TaskService");
        EnsureService<AccessoryService>("AccessoryService");
        EnsureService<FriendService>("FriendService");
        EnsureService<PetService>("PetService");
    }

    private void EnsureService<T>(string name) where T : MonoBehaviour
    {
        if (FindObjectOfType<T>() == null)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<T>();
        }
    }
}

