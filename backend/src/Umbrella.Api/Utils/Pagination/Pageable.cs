namespace Umbrella.Api.Utils.Pagination;

public class Pageable
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 5;
    public string? Sort { get; set; }

    public override string ToString()
    {
        return $"Page {Page}, with size {Size} and sorted by {Sort}";
    }
}