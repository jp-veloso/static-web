using System.ComponentModel.DataAnnotations.Schema;

namespace Umbrella.Api.Entities;

[Table("ScoreRequest", Schema = "nps")]
public class ScoreRequest
{
    public int Id { get; set; }
    public string Key { get; set; }
    public string Email { get; set; }
    public string Reference { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Expired { get; set; }
    public Score? Score { get; set; }
}