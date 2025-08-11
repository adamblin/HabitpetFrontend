
public static class HttpUtils
{
    public static bool IsSuccess(long code) => code >= 200 && code <= 299;

    public static string NormalizeBearer(string token)
    {
        if (string.IsNullOrEmpty(token)) return "";
        return token.StartsWith("Bearer ", System.StringComparison.OrdinalIgnoreCase)
            ? token
            : $"Bearer {token}";
    }

    public static string BuildError(long status, string transport, string body)
    {
        if (!string.IsNullOrEmpty(body)) return $"HTTP {status}: {body}";
        if (!string.IsNullOrEmpty(transport)) return $"HTTP {status}: {transport}";
        return $"HTTP {status}: Error desconocido";
    }
}
