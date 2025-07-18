using UnityEngine;
using System;

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        // Detectar si empieza por [ y termina en ] es un array puro
        if (json.TrimStart().StartsWith("["))
        {
            json = "{\"items\":" + json + "}";
        }

        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.items;
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }
}
