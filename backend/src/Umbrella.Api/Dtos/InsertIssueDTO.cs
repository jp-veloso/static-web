namespace Umbrella.Api.Dtos;

public class InsertIssueDTO
{
    public IssueDTO Data { get; init; } = default!;
    public string? Cnpj { get; set; }
    public string? CompanyName { get; set; }
    public long? InsurerId { get; set; }
}