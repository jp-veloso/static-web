using Microsoft.EntityFrameworkCore;
using Umbrella.Api.Contexts;
using Umbrella.Api.Dtos;
using Umbrella.Api.Entities;
using Umbrella.Api.Entities.Enums;
using Umbrella.Api.Resources.Exceptions;
using Umbrella.Api.Services.Exceptions;
using Umbrella.Api.Utils;

namespace Umbrella.Api.Services;

public class IssueService
{
    public IssueDTO? Insert(InsertIssueDTO payload)
    {
        using RepositoryContext db = new();

        Issue? found = db.InsuranceIssued.SingleOrDefault(x => x.DealId == payload.Data.DealId);

        if (found != null)
        {
            return null;
        }

        Console.WriteLine("NOVO CLIENTE: " + payload.Data.DealId);

        IssueDTO dto = payload.Data;

        List<User> users = db.Users.Where(user => dto.Users.Contains(user.Username))
                             .ToList();

        Issue issue = new()
                      {
                          Commission = dto.Commission,
                          DealId = dto.DealId,
                          Insured = dto.InsuredCnpj,
                          Validity = dto.Validity,
                          ValidUntil = dto.ValidUntil,
                          AmountInsured = dto.Value,
                          Reason = dto.Reason,
                          Bounty = dto.Bounty,
                          IsPaid = dto.IsPaid,
                          LastRate = dto.LastRate,
                          PolicyId = dto.PolicyId,
                          IssuedAt = dto.IssuedAt,
                          Product = dto.Product,
                          Users = users
                      };

        if (payload is { Cnpj: { }, CompanyName: { } })
        {
            string cnpj = TextUtil.UnformatCNPJ(payload.Cnpj);
            Client client = db.Clients.SingleOrDefault(client => client.Cnpj.Equals(cnpj)) ?? new Client(cnpj, payload.CompanyName, Segment.LOW_TOUCH, DateTime.UtcNow);

            client.Name = payload.CompanyName.Length <= 99 ? payload.CompanyName : payload.CompanyName[..99];
            issue.Client = client;
        }

        if (payload.InsurerId != null)
        {
            Insurer insurer = db.Insurers.Single(insurer => insurer.Id == payload.InsurerId);
            issue.Insurer = insurer;
        }

        db.InsuranceIssued.Add(issue);
        db.SaveChanges();

        dto.Id = issue.Id;

        return dto;
    }

    public IssueDTO FindById(int id)
    {
        using RepositoryContext db = new();

        Issue? issue = db.InsuranceIssued.Include(x => x.Client)
                         .Include(x => x.Insurer)
                         .Include(x => x.Users)
                         .SingleOrDefault(x => x.Id == id);

        if (issue == null)
        {
            throw new ServiceException("Issue not found", new StandardError
                                                          {
                                                              Error = "Issue not found",
                                                              Message =
                                                                  $"Entity with issue_id = {id} not found",
                                                              Status = 404,
                                                              Timestamp = DateTime.Now
                                                          });
        }

        return new IssueDTO(issue);
    }
}