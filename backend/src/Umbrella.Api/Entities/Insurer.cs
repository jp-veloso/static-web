using System.ComponentModel.DataAnnotations.Schema;

namespace Umbrella.Api.Entities;

[Table("Insurer", Schema = "portal")]
public class Insurer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool HasIntegration { get; set; }
    public bool Active { get; set; }

    public string? RealName { get; set; }
    public string? Cnpj { get; set; }
    public string? Picture { get; set; }

    // MAPPINGS
    public ICollection<ProposalParameters> ProposalParameters { get; set; } = new List<ProposalParameters>();
    public ICollection<Enrollment>? Enrollments { get; set; }
    public ICollection<Issue>? Issued { get; set; }

    public Insurer(int id, string name, string picture, bool hasIntegration)
    {
        Id = id;
        Name = name;
        HasIntegration = hasIntegration;
        Picture = picture;
        Active = true;
    }
}