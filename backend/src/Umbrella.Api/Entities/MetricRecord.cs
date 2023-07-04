using Umbrella.Cosmos.Repository;

namespace Umbrella.Api.Entities;

public class MetricRecord : Document
{
    public string Period { get; set; } = "";
    public MetricNps? Nps { get; set; }
    public float MonthCommission { get; set; }
    public float Ltv { get; set; }
    public float Cac { get; set; }
    public float Recipe { get; set; }
    public int Clients { get; set; }
    
    public DateTime CreatedAt { get; set; }
}

public class MetricNps
{
    public double Score { get; set; }
    public int Promoters { get; set; }
    public int Detractors { get; set; }
    public int Neutrals { get; set; }
    public int NotAnswer { get; set; }

    public static MetricNps? GenerateNps(int[] scores, int requests)
    {
        if (scores.Length == 0)
        {
            return default;
        }

        int promoters = scores.Count(x => x  >= 9);
        int detractors = scores.Count(x => x <= 6);
        
        return new MetricNps()
        {
            Score = (double) (promoters - detractors) / scores.Length * 100,
            Promoters = promoters,
            Detractors = detractors,
            Neutrals = scores.Length - promoters - detractors,
            NotAnswer = requests - scores.Length
        };
    }
}