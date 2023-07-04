using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Umbrella.Cosmos.Repository.Builders;

namespace Umbrella.Cosmos.Repository.Options;

public class ContainerOptions
{
    internal Type Type { get; }
    internal string? Collection { get; private set; }
    internal string? PartitionKey { get; private set; }

    internal TimeSpan? TimeToLive { get; set; }
    internal string? InternalDatabase { get; private set; }
    internal ThroughputProperties? ThroughputProperties { get; private set; } = ThroughputProperties.CreateManualThroughput(400);
    internal ContainerOptions(Type type) => Type = type;

    public ContainerOptions WithContainer(string collection)
    {
        Collection = collection ?? throw new ArgumentNullException(nameof(collection));
        return this;
    }
    
    public ContainerOptions WithPartitionKey(string partitionKey)
    {
        PartitionKey = partitionKey ?? throw new ArgumentNullException(nameof(partitionKey));
        return this;
    }
    
    public ContainerOptions WithManualThroughput(int throughput = 400)
    {
        if (throughput < 400)
        {
            throw new ArgumentOutOfRangeException(nameof(throughput), "A container must at least set a throughput level of 400 RU/s");
        }

        ThroughputProperties = ThroughputProperties.CreateManualThroughput(throughput);
        return this;
    }

    public ContainerOptions WithInternalDatabase(string database)
    {
        InternalDatabase = database;
        return this;
    }
}