namespace Umbrella.Cosmos.Repository;

public interface IDynamicField
{
    string Discs { get; set; }
    
    string Property { get; }

    public T GetAs<T>() where T : IDynamicField;
}