using RiskAnalysis;
using Umbrella.Api.Contexts;
using Umbrella.Api.Dtos;
using Umbrella.Api.Entities;
using Umbrella.Api.Entities.Enums;
using Umbrella.Api.ProposalResources.Enums;
using Umbrella.Api.Services;

namespace Umbrella.Api.ProposalResources;

public class ProposalRuleTable : ProposalRule
{
    private readonly Dictionary<double, double> CommissionTableAVLA = new()
                                                                      {
                                                                          {9.99, 0.25},
                                                                          {19.99, 0.2625},
                                                                          {29.99, 0.275},
                                                                          {39.99, 0.2875},
                                                                          {49.99, 0.30},
                                                                          {59.99, 0.3125},
                                                                          {69.99, 0.3250},
                                                                          {79.99, 0.3375},
                                                                          {89.99, 0.35},
                                                                          {99.99, 0.3625},
                                                                          {109.99, 0.3750},
                                                                          {110.0, 0.4}
                                                                      };

    private readonly Dictionary<double, double> CommissionTableJUNTO = new()
                                                                       {
                                                                           {00.0, 0.25},
                                                                           {15.0, 0.275},
                                                                           {33.0, 0.3},
                                                                           {53.0, 0.325},
                                                                           {76.0, 0.35},
                                                                           {101.0, 0.375},
                                                                           {123.0, 0.385},
                                                                           {149.0, 0.4}
                                                                       };

    public ProposalRuleTable(RiskAnalysisDataService service) : base(service)
    {
        
    }

    public override string Rule => "TABLE";

    public override void UseRule(ProposalService service, Enrollment enrollment, ProposalParameters parameters)
    {
        Proposal proposal = new() {Insurer = new InsurerDTO(enrollment.Insurer)};
        Taker? taker = enrollment.Policyholders.FirstOrDefault(x => x.Category == service.Data.Category);

        if (taker == null)
        {
            ApplyTakerWarns(service, proposal, enrollment, taker);

            List<string> ratings;
            
            using (var db = new RepositoryContext())
            {
                ratings = db.Enrollments
                    .Where(x => x.Client.Id == enrollment.Client.Id && x.Status == Status.CREATED && !string.IsNullOrEmpty(x.Rating))
                    .Select(x => x.Rating!.Replace(" ", "").Replace("SERASA:", ""))
                    .Distinct()
                    .ToList();

                if (!ratings.Any())
                {
                    proposal.Status = ProposalValues.FAILED;
                    service.AddProposal(proposal);
                    return;
                }
            }

            taker = new Taker()
            {
                Category = Category.OUTRAS,
                Rate = _analysis.Predict(enrollment.Insurer.Id, ratings.First(), true)
            };
        }

        if (!Validate(service, proposal, parameters, taker))
        {
            service.AddProposal(proposal);
            return;
        }

        ApplyCCG(proposal, parameters, taker!, service.Data.InsuredAmount);
        ApplyExtraTerms(proposal, service.Data);

        float grievance = GetGrievance(service.Data, parameters, enrollment.Rating ?? "");
        double aux = 1 + grievance           / 100;
        
        double bountyBase = 0.01 * taker!.Rate            * service.Data.Period * service.Data.InsuredAmount / 365;
        double bountySold = 0.01 * service.Data.SalesRate * service.Data.Period * service.Data.InsuredAmount / 365;

        double auxBountyBase = Math.Max(bountyBase, parameters.MinimumBounty);
        double auxBountySold = Math.Max(bountySold, parameters.MinimumBounty);

        double agg = (auxBountySold - auxBountyBase) / auxBountyBase;

        Dictionary<double, double> table = enrollment.Insurer.Id == (int) ProposalInsurer.AVLA
                                               ? CommissionTableAVLA
                                               : CommissionTableJUNTO;

        double commission = table.MinBy(x => Math.Abs(x.Key - agg * 100)).Value;

        double bounty = agg < 0 ? bountyBase * aux : bountySold * aux;
        bounty = Math.Max(bounty, parameters.MinimumBounty);

        proposal.Bounty = bounty;
        proposal.Commission = commission;
        proposal.Rate = taker.Rate;
        proposal.Balance = taker.Balance;
        proposal.Status = ProposalValues.SUCCESS;

        service.AddProposal(proposal);
    }
}