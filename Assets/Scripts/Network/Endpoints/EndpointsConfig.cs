using UnityEngine;

[CreateAssetMenu(fileName = "EndpointsConfig", menuName = "Config/Endpoints")]
public class EndpointsConfig : ScriptableObject
{
    [Header("API Base URL (sin barra final)")]
    [SerializeField] private string baseUrl = "http://localhost:8080";

    public string BaseUrl => string.IsNullOrWhiteSpace(baseUrl) ? "http://localhost:8080" : baseUrl.TrimEnd('/');

    public void Apply()
    {
        Endpoints.SetBase(BaseUrl);
    }
}
