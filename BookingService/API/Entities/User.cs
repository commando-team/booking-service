namespace API.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; }
    public string PasswordHash {get; private set;}

    private User() { }
    
    public User(string username, string passwordHash)
    {
        Id = Guid.NewGuid();
        Username = username;
        PasswordHash = passwordHash;
    }
}