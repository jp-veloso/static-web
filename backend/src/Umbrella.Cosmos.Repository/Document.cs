using Newtonsoft.Json;

namespace Umbrella.Cosmos.Repository;

public class Document : IDocument
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonProperty("type")]
    public string Type { get; set; }
    
    string IDocument.PartitionKey => GetPartitionKeyValue();
    
    public Document() => Type = GetType().Name;
    protected virtual string GetPartitionKeyValue() => Id;
}