public static class Endpoints
{
    public static string BaseUrl { get; private set; } = "http://localhost:8080";
    public static void SetBase(string baseUrl) => BaseUrl = baseUrl?.TrimEnd('/');

    public static string Auth => $"{BaseUrl}/auth";
    public static string Users => $"{BaseUrl}/users";
    public static string Tasks => $"{BaseUrl}/tasks";
    public static string Friendships => $"{BaseUrl}/friendships";
    public static string Accessories => $"{BaseUrl}/accessories";

    public static string Join(string root, params string[] parts)
    {
        var u = root.TrimEnd('/');
        foreach (var p in parts) if (!string.IsNullOrEmpty(p)) u += "/" + p.Trim('/');
        return u;
    }

    public static string Login() => Join(Auth, "login");
    public static string Register() => Join(Auth, "register");

    public static string CurrentUserPet() => Join(Users, "pet");          
    public static string CreateUserPet() => Users;                        

    public static string TasksOfCurrentUser() => Join(Tasks, "user");
    public static string TaskById(string id) => Join(Tasks, id);

    public static string FriendsList() => Friendships;                 
    public static string FriendRequests() => Join(Friendships, "requests");
    public static string RequestFriend(string u) => Join(Friendships, "request", u);
    public static string AcceptFriend(string u) => Join(Friendships, "accept", u);

    public static string AccessoriesList() => Accessories;
}
