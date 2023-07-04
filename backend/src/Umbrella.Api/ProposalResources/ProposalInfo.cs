using Umbrella.Api.Entities.Enums;
using Umbrella.Api.ProposalResources.Enums;

namespace Umbrella.Api.ProposalResources;

public class ProposalInfo
{
    public double InsuredAmount { get; set; }
    public int Period { get; set; }
    public double SalesRate { get; set; }
    public int Retroactivity { get; set; }
    public Category Category { get; set; }
    public ProposalType Contract { get; set; }
    public ProposalValues Modality { get; set; }
    public Dictionary<string, bool> Terms { get; set; } = new();
}