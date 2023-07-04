using Umbrella.Api.Entities;
using Umbrella.Api.Entities.Enums;

namespace Umbrella.Api.Dtos;

public class TakerDTO
{
    public int Id { get; set; }

    public double Limit { get; set; }

    public double Balance { get; set; }

    public Category Category { get; set; }

    public float Rate { get; set; }

    public TakerDTO(Taker entity)
    {
        Id = entity.Id;
        Limit = entity.Limit;
        Balance = entity.Balance;
        Category = entity.Category;
        Rate = entity.Rate;
    }

    public TakerDTO()
    {
    }
}