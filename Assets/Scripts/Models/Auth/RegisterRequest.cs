using System;

[Serializable]
public class RegisterRequest
{
    public string username;
    public string email;
    public string password;

    public RegisterRequest(string username, string email, string password)
    {
        this.username = username;
        this.email = email;
        this.password = password;
    }
}
