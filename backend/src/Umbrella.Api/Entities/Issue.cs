using System.ComponentModel.DataAnnotations.Schema;
using Umbrella.Api.Entities.Enums;

namespace Umbrella.Api.Entities;

[Table("Issue", Schema = "portal")]
public class Issue
{
    public int Id { get; set; }
    public double Bounty { get; set; }
    public double Commission { get; set; }
    public double? AmountInsured { get; set; }
    public int? Validity { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? DealId { get; set; }
    public string PolicyId { get; set; } = "";
    public string? Insured { get; set; }
    public DateTime IssuedAt { get; set; }
    public Product Product { get; set; }
    public bool IsPaid { get; set; }

    public float? LastRate { get; set; }
    public Reason? Reason { get; set; }

    // MAPPINGS
    public Client? Client { get; set; }
    public Insurer? Insurer { get; set; }
    public ICollection<User> Users { get; set; } = new HashSet<User>();
}