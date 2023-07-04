using System.ComponentModel.DataAnnotations.Schema;

namespace Umbrella.Api.Entities;

[Table("User", Schema = "portal")]
public class User
{
    private string _authorities = "";
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Password { get; set; }
    public string Username { get; set; } = "";
    public string? Department { get; set; }

    public DateTime? LastLogin { get; set; }
    public ICollection<Issue> Issues { get; set; } = new HashSet<Issue>();

    public string[] Authorities
    {
        get => _authorities.ToLower()
                           .Split(",");
        set => _authorities = string.Join(",", value);
    }

    public User(string username, string name, string[] authorities)
    {
        Authorities = authorities;
        Name = name;
        Username = username;
        LastLogin = DateTime.UtcNow;
    }

    public User()
    {
    }
}