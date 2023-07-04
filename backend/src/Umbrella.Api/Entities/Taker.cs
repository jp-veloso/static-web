using System.ComponentModel.DataAnnotations.Schema;
using Umbrella.Api.Entities.Enums;

namespace Umbrella.Api.Entities;

[Table("Taker", Schema = "portal")]
public class Taker
{
    public int Id { get; set; }
    public double Limit { get; set; }
    public double Balance { get; set; }
    public Category Category { get; set; }
    public float Rate { get; set; }

    public Enrollment Enrollment { get; set; } = default!;

    public Taker(double limit, double balance, Category category, float rate)
    {
        Limit = limit;
        Balance = balance;
        Category = category;
        Rate = rate;
    }

    public Taker(Category category)
    {
        Category = category;
    }

    public Taker()
    {
    }
}