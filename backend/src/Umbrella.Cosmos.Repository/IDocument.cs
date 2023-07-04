namespace Umbrella.Cosmos.Repository;

public interface IDocument
{
    string Id { get; set; }
    
    string Type { get; set; }
    
    string PartitionKey { get; }
}