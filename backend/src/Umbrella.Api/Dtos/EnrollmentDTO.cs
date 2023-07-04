using System.ComponentModel.DataAnnotations;
using Umbrella.Api.Entities;
using Umbrella.Api.Entities.Enums;

namespace Umbrella.Api.Dtos;

public class EnrollmentDTO
{
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpireAt { get; set; }
    public Status Status { get; set; }
    public string? Warn { get; set; }
    public string? Rating { get; set; }
    public bool IsActive { get; set; }
    public int? ClientId { get; set; }

    [Required]
    public int InsurerId { get; set; }

    public InsurerDTO? Insurer { get; set; }
    public ICollection<TakerDTO>? Takers { get; set; }
    
    public ICollection<VirtualRateDTO>? VirtualRates { get; set; }

    private EnrollmentDTO(Enrollment entity)
    {
        CreatedAt = entity.CreatedAt;
        ExpireAt = entity.ExpireAt;
        Status = entity.Status;
        Warn = entity.Warn;
        Rating = entity.Rating;
        IsActive = entity.IsActive;
    }

    public EnrollmentDTO(Enrollment entity, Insurer? insurer, IEnumerable<Taker> takers, IEnumerable<VirtualRateDTO> virtualRates) :
        this(entity, insurer, null, takers)
    {
        VirtualRates = virtualRates.ToList();
    }

    public EnrollmentDTO(Enrollment entity, Insurer? insurer, Client? client, IEnumerable<Taker> takers) : this(entity)
    {
        if (insurer != null)
        {
            Insurer = new InsurerDTO(insurer);
            InsurerId = insurer.Id;
        }

        if (client != null)
        {
            ClientId = client.Id;
        }

        Takers = new List<TakerDTO>();
        
        foreach (Taker? taker in takers)
        {
            Takers.Add(new TakerDTO(taker));
        }
    }

    public EnrollmentDTO()
    {
    }
}