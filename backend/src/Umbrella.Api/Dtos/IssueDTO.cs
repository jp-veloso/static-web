using Umbrella.Api.Entities;
using Umbrella.Api.Entities.Enums;

namespace Umbrella.Api.Dtos;

public class IssueDTO
{
    public int Id { get; set; }
    public double Bounty { get; set; }
    public double Commission { get; set; }
    public int? Validity { get; set; }

    public string? DealId { get; set; }
    public string PolicyId { get; set; }

    public string? InsuredCnpj { get; set; }
    public DateTime IssuedAt { get; set; }
    public Product Product { get; set; }
    public DateTime? ValidUntil { get; set; }
    public double? Value { get; set; }

    public bool IsPaid { get; set; }
    public Reason? Reason { get; set; }

    public float? LastRate { get; set; }

    public ClientDTO? Client { get; set; }
    public InsurerDTO? Insurer { get; set; }
    public List<string> Users { get; set; }

    public IssueDTO(Issue entity)
    {
        Id = entity.Id;
        Bounty = entity.Bounty;
        Commission = entity.Commission;
        Validity = entity.Validity;
        DealId = entity.DealId;
        PolicyId = entity.PolicyId;
        Value = entity.AmountInsured;
        ValidUntil = entity.ValidUntil;
        InsuredCnpj = entity.Insured;
        IssuedAt = entity.IssuedAt;
        Product = entity.Product;
        IsPaid = entity.IsPaid;
        Reason = entity.Reason;
        LastRate = entity.LastRate;

        if (entity.Client != null)
        {
            Client = new ClientDTO(entity.Client);
        }

        if (entity.Insurer != null)
        {
            Insurer = new InsurerDTO(entity.Insurer);
        }

        Users = entity.Users.Select(x => string.IsNullOrEmpty(x.Name) ? x.Username : x.Name)
                      .ToList();
    }

    public IssueDTO()
    {
    }
}