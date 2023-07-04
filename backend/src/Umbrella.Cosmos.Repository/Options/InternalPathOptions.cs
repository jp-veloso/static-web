using System.Reflection;
using Microsoft.Azure.Cosmos;

namespace Umbrella.Cosmos.Repository.Options;

public class InternalPathOptions
{
    public PatchOperationType Type { get; }
    public PropertyInfo PropertyInfo { get; }

    public object? NewValue { get; }

    public InternalPathOptions(PropertyInfo propertyInfo, object? newValue, PatchOperationType type)
    {
        PropertyInfo = propertyInfo;
        NewValue = newValue;
        Type = type;
    }
    
}