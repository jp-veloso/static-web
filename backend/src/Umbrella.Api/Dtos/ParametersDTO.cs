using Umbrella.Api.Entities;
using Umbrella.Api.Entities.Enums;

namespace Umbrella.Api.Dtos;

public class ParametersDTO
{
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
    public string GrievanceRule { get; set; }

    public ParametersDTO()
    {
        
    }

    public ParametersDTO(ProposalParameters entity, string grievanceRule)
    {
        ProposalType = entity.ProposalType;
        Ccg = entity.Ccg;
        MinimumBrokerage = entity.MinimumBrokerage;
        InternalRetroactivity = entity.InternalRetroactivity;
        ExternalRetroactivity = entity.ExternalRetroactivity;
        Exclusive = entity.Exclusive;
        Pstp = entity.Pstp;
        BaseCommission = entity.BaseCommission;
        MaximumCommission = entity.MaximumCommission;
        MinimumBounty = entity.MinimumBounty;
        GrievanceRule = grievanceRule;
    }
}