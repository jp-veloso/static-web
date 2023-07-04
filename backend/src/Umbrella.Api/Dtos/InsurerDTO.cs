using Umbrella.Api.Entities;
using Umbrella.Api.Entities.Enums;

namespace Umbrella.Api.Dtos;

public class InsurerDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool HasIntegration { get; set; }
    public bool Active { get; set; }
    public string? Picture { get; set; }

    public ICollection<ParametersDTO>? Parameters { get; set; }

    public InsurerDTO(Insurer entity)
    {
        Id = entity.Id;
        Name = entity.Name;
        HasIntegration = entity.HasIntegration;
        Active = entity.Active;
        Picture = entity.Picture;
    }

    public InsurerDTO(Insurer entity, ICollection<ProposalParameters> parameters, ProposalType type) : this(entity)
    {
        Parameters = new List<ParametersDTO>();
        
        foreach (var item in parameters.Where(x => x.ProposalType == type))
        {
            string[] gRules = item.GrievanceRule.Split(";");
            string rule;

            if (gRules[0] != "#")
            {
                rule = "Para todos";
                
                if (gRules[0] != "*")
                {
                    rule = "Rating " + gRules[0];
                }
                
                rule += ", agravo de " + gRules[1] + "% da taxa base.";

                if (double.TryParse(gRules[0], out _))
                {
                    rule = "Para todos, agravo de 50% da taxa base.";
                }
            }
            else
            {
                rule = "Não agrava";
            }

            Parameters.Add(new ParametersDTO(item, rule));
        }
    }
}