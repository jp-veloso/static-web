using System.Reflection;
using Umbrella.Cosmos.Repository.Builders;

namespace Umbrella.Cosmos.Repository.Options;

public class InheritanceOptions
{
    internal readonly MemberInfo _memberInfo;
    internal readonly Dictionary<string[], Type> Configurations = new();
    
    private readonly DefaultInheritanceOptionsBuilder _builder;
    public InheritanceOptions(MemberInfo info, DefaultInheritanceOptionsBuilder builder)
    {
        _memberInfo = info;
        _builder = builder;
    }

    public InheritanceOptions ConfigureType<T>(params string[] discriminators)
    {
        Add(typeof(T), discriminators);
        return this;
    }

    public void Build() => _builder.Build(this);
    
    private void Add(Type type, params string[] discriminators)
    {
        var keys = Configurations.Keys;

        if (keys.Any(strings => strings.Any(discriminators.Contains)))
        {
            throw new ArgumentException("At least one discriminators has already been added", nameof(discriminators));
        }
        
        Configurations.Add(discriminators, type);
    }
}