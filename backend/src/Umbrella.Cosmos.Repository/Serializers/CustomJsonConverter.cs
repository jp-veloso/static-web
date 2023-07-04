using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbrella.Cosmos.Repository.Providers;

namespace Umbrella.Cosmos.Repository.Serializers;
public class CustomJsonConverter : JsonConverter
{
    public override bool CanRead => true;
    public override bool CanWrite => false;
    
    private readonly DefaultItemInheritanceProvider _provider;
    
    public CustomJsonConverter(DefaultItemInheritanceProvider provider)
    {
        _provider = provider;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(DynamicField);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var obj = JObject.Load(reader);

        string propertyName = obj["property"]!.Value<string>();
        
        Type dynamicType = _provider.GetDynamicType(propertyName, obj["discs"]!.Value<string>().Split(","));
        
        return obj.ToObject(dynamicType);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotSupportedException("This converter handles only deserialization, not serialization.");
    }
}