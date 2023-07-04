using System.ComponentModel.DataAnnotations;

namespace Umbrella.Api.Dtos;

public class UserDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }

    [Required]
    public string Password { get; set; } = "";

    [Required]
    public string Username { get; set; } = "";

    public string[]? Roles { get; set; }
}