using System;
using System.Collections;
using UnityEngine;

public class PetService : MonoBehaviour
{
    public static PetService Instance { get; private set; }

    private static string CreateUserPetUrl => Endpoints.CreateUserPet();  
    private static string CurrentUserPetUrl => Endpoints.CurrentUserPet();  

    private PetData currentPet;
    public bool HasLoadedPet => currentPet != null;

    [Serializable]
    private class PetCreateDto
    {
        public string name;
        public PetCreateDto(string name) { this.name = name; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public IEnumerator CreatePet(string petName, string token, Action onSuccess, Action<string> onError)
    {
        if (string.IsNullOrWhiteSpace(petName)) { onError?.Invoke("El nombre de la mascota no puede estar vacío."); yield break; }
        if (string.IsNullOrEmpty(token)) { onError?.Invoke("Token vacío o nulo."); yield break; }

        var dto = new PetCreateDto(petName);

        yield return ApiClient.Post<string>(
            url: CreateUserPetUrl,
            data: dto,
            onSuccess: _ => onSuccess?.Invoke(),
            onError: err => onError?.Invoke(err),
            withAuth: true
        );
    }

    public IEnumerator GetPet(string token, Action<PetData> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrEmpty(token)) { onError?.Invoke("Token vacío o nulo."); yield break; }

        yield return ApiClient.Get<PetData>(
            url: CurrentUserPetUrl,
            onSuccess: pet =>
            {
                currentPet = pet; 
                onSuccess?.Invoke(pet);
            },
            onError: onError,
            withAuth: true
        );
    }
}
