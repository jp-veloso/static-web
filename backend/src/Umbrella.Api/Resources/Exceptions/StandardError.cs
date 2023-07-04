namespace Umbrella.Api.Resources.Exceptions;

public class StandardError
{
    public DateTime Timestamp { get; set; }
    public int Status { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
    public string? Path { get; set; }
}