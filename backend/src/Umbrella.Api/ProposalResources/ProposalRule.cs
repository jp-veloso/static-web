using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RiskAnalysis;
using Umbrella.Api.Contexts;
using Umbrella.Api.Dtos;
using Umbrella.Api.Entities;
using Umbrella.Api.Entities.Enums;
using Umbrella.Api.ProposalResources.Enums;

namespace Umbrella.Api.ProposalResources;

public class ProposalRule
{
    protected readonly RiskAnalysisDataService _analysis;
    public virtual string Rule => "COMMON";
    private static string CCG_WARN => "Pode ser necessário assinatura.";
    private static string EXCLUSIVE_WARN => "Fora da política de aceitação.";
    private static string RETROACTIVITY_WARN => "Fora da política de aceitação.";
    private static string PSTP_WARN => "A companhia não aceita prestação de serviços com trabalhista e previdenciário.";
    private static string INTERNAL_WARN => "Até dois dias para emissão.";

    public ProposalRule(RiskAnalysisDataService analysis)
    {
        _analysis = analysis;
    }

    protected void ApplyTakerWarns(ProposalService service, Proposal proposal, Enrollment enrollment, Taker? taker)
    {
        if (enrollment.Status == Status.ERROR)
        {
            proposal.AddWarn("Cadastro", "O cliente está com problemas no cadastro com a seguradora.");
        }

        if (enrollment.Status != Status.ERROR && taker == null)
        {
            proposal.AddWarn("Taxa", $"O cliente não tem a taxa {service.Data.Category} na seguradora.");
        }
    }

    protected void ApplyCCG(Proposal proposal, ProposalParameters parameters, Taker taker, double insuredAmount)
    {
        if (taker.Balance != 0 && taker.Limit != 0)
        {
            if (parameters.Ccg - (taker.Limit - taker.Balance) - insuredAmount < 0)
            {
                proposal.AddWarn("CCG", CCG_WARN);
            }
        }
        else
        {
            if (parameters.Ccg < insuredAmount * 0.9)
            {
                proposal.AddWarn("CCG", CCG_WARN);
            }
        }
    }

    private static bool ApplyRetroactive(Proposal proposal, ProposalParameters parameters, int retroactivity)
    {
        if (parameters.ExternalRetroactivity >= retroactivity)
        {
            return true;
        }

        proposal.AddWarn("Retroatividade", RETROACTIVITY_WARN);
        if (parameters.InternalRetroactivity)
        {
            return true;
        }

        proposal.Status = ProposalValues.FAILED;
        return false;
    }

    private static bool ApplyExclusive(Proposal proposal, ProposalParameters parameters, bool isExclusive)
    {
        if (!isExclusive)
        {
            return true;
        }

        if (parameters.Exclusive)
        {
            return true;
        }

        proposal.AddWarn("Exclusividade", EXCLUSIVE_WARN);
        proposal.Status = ProposalValues.FAILED;
        return false;
    }

    private static bool ApplyPSTP(Proposal proposal, ProposalParameters parameters, bool hasPstp)
    {
        if (!hasPstp)
        {
            return true;
        }

        if (parameters.Pstp || parameters.ProposalType == ProposalType.PRIVATE_CONTRACT)
        {
            return true;
        }

        proposal.AddWarn("PSTP", PSTP_WARN);
        proposal.Status = ProposalValues.FAILED;
        return false;
    }

    protected void ApplyExtraTerms(Proposal proposal, ProposalInfo info)
    {
        string[] terms = {"deceit", "irrevocable", "law", "corruption"};

        if (terms.Any(term => info.Terms.Any(x => x.Key == term && x.Value)))
        {
            proposal.AddWarn("Prazo", INTERNAL_WARN);
        }
    }

    protected float GetGrievance(ProposalInfo info, ProposalParameters parameters, string rating)
    {
        float grievance = 0;

        if (parameters.ProposalType == ProposalType.PRIVATE_CONTRACT && info.Terms["penalties"])
        {
            grievance += 50;
        }
        
        if (!info.Terms["security"])
        {
            return grievance;
        }

        string rule = parameters.GrievanceRule.Split(";")[0];

        if (rule == "*") // Agrava para todos
        {
            grievance += 50;
        } else if (rule != "#")
        {
            if (rating == rule)
            {
                grievance += 50;
            }
        }

        return grievance;
    }

    protected bool Validate(ProposalService service, Proposal proposal, ProposalParameters parameters, Taker taker)
    {
        
        if (taker.Balance < service.Data.InsuredAmount && taker.Category == Category.TRADICIONAL)
        {
            proposal.Status = ProposalValues.FAILED;
            proposal.AddWarn("Saldo", "Sem limite para emissão.");
            return false;
        }

        if (!ApplyRetroactive(proposal, parameters, service.Data.Retroactivity))
        {
            return false;
        }

        return ApplyExclusive(proposal, parameters, service.Data.Terms["exclusive"]) && 
               ApplyPSTP(proposal, parameters, service.Data.Terms["security"] && service.Data.Modality == ProposalValues.PRESTADOR);
    }

    public void DefaultProposal(ProposalService service)
    {
        float grievance = 0F;
        double defaultPrize = 0.01 * service.Data.SalesRate * service.Data.Period * service.Data.InsuredAmount / 365;
        
        if (service.Data.Terms["security"]) grievance += 50;
        if (service.Data.Terms["penalties"]) grievance += 50;
        
        defaultPrize *= 1 + grievance / 100;

        Proposal proposal = new()
                            {
                                Bounty = defaultPrize,
                                Status = ProposalValues.SUCCESS
                            };
        
        ApplyExtraTerms(proposal, service.Data);

        if (service.Data.Terms["exclusive"])
        {
            proposal.AddWarn("Exclusividade", "Disponível somente nas cias Junto, BMG e Pottencial.");
        }

        service.AddProposal(proposal);
    }

    public virtual void UseRule(ProposalService service, Enrollment enrollment, ProposalParameters parameters)
    {
        Proposal proposal = new() { Insurer = new InsurerDTO(enrollment.Insurer) };
        Taker? taker = enrollment.Policyholders.FirstOrDefault(x => x.Category == service.Data.Category);

        if (taker == null)
        {
            ApplyTakerWarns(service, proposal, enrollment, taker);

            List<string> ratings;
            
            using (var db = new RepositoryContext())
            {
                var results = db.Enrollments.Where(x => x.Client.Id == enrollment.Client.Id && x.Status == Status.CREATED).ToList();
                
                ratings = results.Where(x => !string.IsNullOrEmpty(x.Rating)).Select(x => x.Rating!.Replace(" ", "").Replace("SERASA:", "")).Distinct().ToList();

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

        ApplyCCG(proposal, parameters, taker, service.Data.InsuredAmount);
        ApplyExtraTerms(proposal, service.Data);

        float grievance = GetGrievance(service.Data, parameters, enrollment.Rating ?? "");
        double aux = 1 + grievance           / 100;

        double bountyBase = 0.01 * taker.Rate            * service.Data.Period * service.Data.InsuredAmount / 365;
        double bountySold = 0.01 * service.Data.SalesRate * service.Data.Period * service.Data.InsuredAmount / 365;

        double auxBountyBase = Math.Max(bountyBase, parameters.MinimumBounty);
        double auxBountySold = Math.Max(bountySold, parameters.MinimumBounty);

        double agg = (auxBountySold - auxBountyBase) / auxBountyBase;

        double commission = agg < 0 ? parameters.BaseCommission : (agg + 1) * parameters.BaseCommission;

        commission = Math.Max(commission, parameters.BaseCommission);
        commission = Math.Min(commission, parameters.MaximumCommission);
        
        double bounty = agg < 0 ? bountyBase * aux : bountySold * aux;

        bounty = Math.Max(bounty, parameters.MinimumBounty);

        if (bounty == parameters.MinimumBounty && double.TryParse(parameters.GrievanceRule.Split(";")[0], out var rule) && service.Data.Terms["security"])
        {
            bounty = rule;
        }
        
        proposal.Bounty = bounty;
        proposal.Commission = commission;
        proposal.Rate = taker.Rate;
        proposal.Status = ProposalValues.SUCCESS;
        proposal.Balance = taker.Balance;

        service.AddProposal(proposal);
    }
}