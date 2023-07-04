using System.ComponentModel.DataAnnotations.Schema;
using Umbrella.Api.Entities.Enums;

namespace Umbrella.Api.Entities;

[Table("Proposal_Parameters", Schema = "portal")]
public class ProposalParameters
{
    public int InsurerId { get; set; }
    public ProposalType ProposalType { get; set; }
    public double Ccg { get; set; }
    public int MinimumBrokerage { get; set; }
    public bool InternalRetroactivity { get; set; }
    public int ExternalRetroactivity { get; set; }
    public bool Exclusive { get; set; }
    public bool Pstp { get; set; }
    public float BaseCommission { get; set; }
    public float MaximumCommission { get; set; }
    public float MinimumBounty { get; set; }
    public string GrievanceRule { get; set; } = "";
    public Insurer? Insurer { get; set; }
}