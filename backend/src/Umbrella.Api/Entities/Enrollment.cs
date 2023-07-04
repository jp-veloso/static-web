using System.ComponentModel.DataAnnotations.Schema;
using Umbrella.Api.Entities.Enums;

namespace Umbrella.Api.Entities;

[Table("Enrollment", Schema = "portal")]
public class Enrollment
{
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpireAt { get; set; }
    public Status Status { get; set; }
    public string? Warn { get; set; }
    public bool IsActive { get; set; }
    public string? Rating { get; set; }

    // MAPPINGS
    public Client Client { get; set; } = default!;
    public Insurer Insurer { get; set; } = default!;
    public ICollection<Taker> Policyholders { get; set; } = new List<Taker>();

    public Enrollment(Client client, Insurer insurer)
    {
        Client = client;
        Insurer = insurer;
        CreatedAt = DateTime.UtcNow;
        Status = Status.PREPARING;
    }

    public Enrollment()
    {
    }
}