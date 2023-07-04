using System.ComponentModel.DataAnnotations.Schema;

namespace Umbrella.Api.Entities;

[Table("Score", Schema = "nps")]
public class Score
{
    public int Id { get; set; }
    public string? Comment { get; set; }
    public int Value { get; set; }
    public ScoreRequest? Request { get; set; }
}