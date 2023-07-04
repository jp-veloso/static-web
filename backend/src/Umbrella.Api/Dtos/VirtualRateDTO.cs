namespace Umbrella.Api.Dtos;

public class VirtualRateDTO
{
    public float VirtualRate { get; set; }
    public string Rating { get; set; } = "default";
    public string RatingSource { get; set; } = "cia";
}