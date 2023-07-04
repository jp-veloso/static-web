using RiskAnalysis;
using Umbrella.Api.Entities;

namespace Umbrella.Api.ProposalResources;

public class ProposalService
{
    private readonly List<Proposal> _proposals = new();
    private readonly List<ProposalRule> _rules = new();

    private ProposalParameters? _parameters;

    private ProposalRule? _rule;
    public ProposalInfo Data { get; }

    public ProposalService(ProposalInfo data, RiskAnalysisDataService analService)
    {
        Data = data;
        _rules.Add(new ProposalRule(analService));
        _rules.Add(new ProposalRuleTable(analService));
    }

    public ProposalService UseParameters(ProposalParameters parameters)
    {
        _parameters = parameters;
        return this;
    }

    public ProposalService ChangeRule(Func<ProposalRule, bool> predicate)
    {
        _rule = _rules.FirstOrDefault(predicate);
        return this;
    }

    public List<Proposal> GetProposals()
    {
        return _proposals.OrderByDescending(x => x.Commission)
                         .ThenBy(x => x.Bounty)
                         .ToList();
    }

    public void DoDefault()
    {
        _rule?.DefaultProposal(this);
    }

    public void DoProposal(Enrollment enrollment)
    {
        if (_parameters != null)
        {
            _rule?.UseRule(this, enrollment, _parameters);
        }
    }

    public void AddProposal(Proposal proposal)
    {
        Proposal? p = _proposals.FirstOrDefault(x => proposal.Insurer != null && x.Insurer?.Id == proposal.Insurer.Id);

        if (p == null)
        {
            _proposals.Add(proposal);
        }
        else
        {
            int index = _proposals.IndexOf(p);
            _proposals[index] = proposal;
        }
    }
}