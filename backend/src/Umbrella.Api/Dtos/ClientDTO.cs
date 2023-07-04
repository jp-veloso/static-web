using System.ComponentModel.DataAnnotations;
using Umbrella.Api.Entities;
using Umbrella.Api.Entities.Enums;
using Umbrella.Api.Utils;

namespace Umbrella.Api.Dtos;

public class ClientDTO
{
    public int Id { get; set; }

    [Required]
    [Unique]
    public string Cnpj { get; set; } = "";
    public string Name { get; set; } = "";
    public string? PipeId { get; set; }
    public Segment? Segment { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool? Active { get; set; }
    public bool? IsClient { get; set; }
    public IssueDTO? LastIssue { get; set; }

    public ICollection<EnrollmentDTO>? Enrollments { get; set; }

    public ClientDTO(Client entity)
    {
        Id = entity.Id;
        Cnpj = TextUtil.FormatCNPJ(entity.Cnpj);
        Name = entity.Name;
        Segment = entity.Segment;
        CreatedAt = entity.CreatedAt;
        PipeId = entity.Pipe;
    }

    public ClientDTO(Client entity, IEnumerable<Enrollment> enrollments) : this(entity)
    {
        Enrollments = new List<EnrollmentDTO>();

        foreach (Enrollment? enrollment in enrollments)
        {
            Enrollments.Add(new EnrollmentDTO(enrollment, enrollment.Insurer, null, enrollment.Policyholders));
        }
    }

    public ClientDTO()
    {
    }
}