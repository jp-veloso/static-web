using Microsoft.Azure.Cosmos;

namespace Umbrella.Cosmos.Repository.Options;

public class ItemConfiguration
{
    public Type Type { get; }

    public string ContainerName { get; }

    public string PartitionKeyPath { get; }

    public UniqueKeyPolicy? UniqueKeyPolicy { get; }

    public ThroughputProperties? ThroughputProperties { get; }

    public int DefaultTimeToLive { get; }
    
    public string? InternalDatabase { get; }

    public ItemConfiguration(
        Type type, 
        string containerName, 
        string partitionKeyPath, 
        UniqueKeyPolicy? uniqueKeyPolicy, 
        ThroughputProperties? throughputProperties,
        string? internalDatabase,
        int defaultTimeToLive = -1
        )
    {
        Type = type;
        ContainerName = containerName;
        PartitionKeyPath = partitionKeyPath;
        UniqueKeyPolicy = uniqueKeyPolicy;
        ThroughputProperties = throughputProperties;
        DefaultTimeToLive = defaultTimeToLive;
        InternalDatabase = internalDatabase;
    }
}