using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Umbrella.Cosmos.Repository;

public abstract class DynamicField : IDynamicField
{
    [JsonProperty("discs")] 
    public string Discs { get; set; }
    
    public string Property { get; set; }

    [JsonIgnore]
    public string[] DiscsArray
    {
        get => Discs.Split(",");
        set => Discs = string.Join(",", value);
    }

    protected DynamicField(string propertyName)
    {
        Property = propertyName;
        Discs = StringProcessor.ToSnake(GetType().Name);
    }

    public T GetAs<T>() where T : IDynamicField 
        => (T) Convert.ChangeType(this, typeof(T));

    protected void SetDiscriminators(params string[] discriminators)
    {
        DiscsArray = DiscsArray.Union(discriminators).ToArray();
    }

    public override string ToString()
    {
        string json = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings(){ContractResolver = new CamelCasePropertyNamesContractResolver()});

        return $"## {GetType().Name} Information\n{json}";
    }
}