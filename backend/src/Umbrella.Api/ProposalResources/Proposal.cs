using Umbrella.Api.Dtos;
using Umbrella.Api.ProposalResources.Enums;

namespace Umbrella.Api.ProposalResources;

public class Proposal
{
    public double Bounty { get; set; }
    public double Commission { get; set; }
    public double Rate { get; set; }
    public double Balance { get; set; }
    public ProposalValues Status { get; set; }
    public InsurerDTO? Insurer { get; init; }
    public Dictionary<string, string> Warns { get; } = new();

    public void AddWarn(string identifier, string value)
    {
        Warns.Add(identifier, value);
    }
}