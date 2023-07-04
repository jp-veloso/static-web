using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Umbrella.Cosmos.Repository.Options;

namespace Umbrella.Cosmos.Repository.Builders;

public class DefaultPatchOperationBuilder<T> where T : IDocument
{
    private readonly List<PatchOperation> _patchOperations = new();
    private readonly NamingStrategy _namingStrategy;

    private readonly List<InternalPathOptions> _rawPatchOperations = new();

    public IReadOnlyList<PatchOperation> PatchOperations => _patchOperations;

    public DefaultPatchOperationBuilder() =>
        _namingStrategy = new CamelCaseNamingStrategy();

    public DefaultPatchOperationBuilder(CosmosPropertyNamingPolicy? cosmosPropertyNamingPolicy) =>
        _namingStrategy = cosmosPropertyNamingPolicy == CosmosPropertyNamingPolicy.Default
            ? new DefaultNamingStrategy()
            : new CamelCaseNamingStrategy();

    public DefaultPatchOperationBuilder<T> Replace<TValue>(Expression<Func<T, TValue>> expression, TValue? value)
    {
        Type type = typeof(T);

        if (expression.Body is not MemberExpression member)
            throw new ArgumentException($"Expression '{expression}' refers to a method, not a property.");

        if (member.Member is not PropertyInfo propInfo)
            throw new ArgumentException($"Expression '{expression}' refers to a field, not a property.");

#pragma warning disable IDE0046 // Convert to conditional expression
        if (propInfo.ReflectedType != null &&
            type != propInfo.ReflectedType &&
            !type.IsSubclassOf(propInfo.ReflectedType))
            throw new ArgumentException($"Expression '{expression}' refers to a property that is not from type {type}.");
#pragma warning restore IDE0046 // Convert to conditional expression

        PropertyInfo property = propInfo;
        
        var propertyToReplace = GetPropertyToReplace(property);
        
        _rawPatchOperations.Add(new InternalPathOptions(property, value, PatchOperationType.Replace));
        _patchOperations.Add(PatchOperation.Replace($"/{propertyToReplace}", value));
        return this;
    }

    private string GetPropertyToReplace(MemberInfo propertyInfo)
    {
        JsonPropertyAttribute[] attributes =
            propertyInfo.GetCustomAttributes<JsonPropertyAttribute>(true).ToArray();

        return attributes.Length is 0
            ? _namingStrategy.GetPropertyName(propertyInfo.Name, false)
            : attributes[0].PropertyName;
    }
}