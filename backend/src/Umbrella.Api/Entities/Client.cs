using System.ComponentModel.DataAnnotations.Schema;
using Umbrella.Api.Entities.Enums;

namespace Umbrella.Api.Entities;

[Table("Client", Schema = "portal")]
public class Client
{
    public int Id { get; set; }
    public string Cnpj { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Pipe { get; set; }
    public Segment Segment { get; set; }
    public DateTime CreatedAt { get; set; }

    // MAPPINGS
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Issue> Issued { get; set; } = new HashSet<Issue>();

    public Client(string cnpj, string name, Segment segment, DateTime createdAt)
    {
        Cnpj = cnpj;
        Name = name;
        Segment = segment;
        CreatedAt = createdAt;
    }

    public Client()
    {
    }
}