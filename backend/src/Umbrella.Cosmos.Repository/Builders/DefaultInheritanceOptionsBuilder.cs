using System.Linq.Expressions;
using Umbrella.Cosmos.Repository.Options;

namespace Umbrella.Cosmos.Repository.Builders;

public class DefaultInheritanceOptionsBuilder
{
    private readonly List<InheritanceOptions> _options = new();
    public IReadOnlyList<InheritanceOptions> Options => _options;
    
    public InheritanceOptions WithInheritance<T>(Expression<Func<T, IDynamicField?>> propertyCatcher)  where T : IDocument
    {
        if (propertyCatcher.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Invalid expression. You should use something like 'o => o.Property'", nameof(propertyCatcher));
        }

        var options = new InheritanceOptions(memberExpression.Member, this);

        return options;
    }

    public void Build(InheritanceOptions options)
    {
        _options.Add(options);
    }
}