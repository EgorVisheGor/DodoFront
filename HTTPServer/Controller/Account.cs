namespace HTTPServer.Controller;

public class Account
{
    public string Login { get; set; }
    
    public string Password { get; set; }
    
    public int Id { get; set; }

    public Account(string login, string password, int id)
    {
        Login = login;
        Password = password;
        Id = id;
    }
}