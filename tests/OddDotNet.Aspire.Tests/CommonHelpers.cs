using Bogus;
using Google.Protobuf;
using OpenTelemetry.Proto.Common.V1;
using OtelResource = OpenTelemetry.Proto.Resource.V1.Resource;
using OtelInstrumentationScope = OpenTelemetry.Proto.Common.V1.InstrumentationScope;
using OtelAnyValue = OpenTelemetry.Proto.Common.V1.AnyValue;

namespace OddDotNet.Aspire.Tests;

public class CommonHelpers
{
    public static OtelResource CreateResource()
    {
        var faker = new Faker();
        var item = new OtelResource()
        {
            Attributes = { CreateKeyValue(faker.Random.String2(8), faker.Random.String2(8)) },
            DroppedAttributesCount = faker.Random.UInt()
        };

        return item;
    }
    
    public static OtelInstrumentationScope CreateInstrumentationScope()
    {
        var faker = new Faker();
        var item = new OtelInstrumentationScope()
        {
            Name = faker.Random.String2(8),
            Version = faker.Random.String2(8),
            Attributes = { CreateKeyValue(faker.Random.String2(8), faker.Random.String2(8)) }
        };
        
        return item;
    }
    
    public static KeyValue CreateKeyValue<TValue>(string key, TValue value)
    {
        var anyValue = value switch
        {
            _ when value is string s => new OtelAnyValue(){ StringValue = s },
            _ when value is int i => new OtelAnyValue(){ IntValue = i },
            _ when value is double d => new OtelAnyValue(){ DoubleValue = d },
            _ when value is bool b => new OtelAnyValue(){ BoolValue = b },
            _ when value is ByteString b => new OtelAnyValue(){ BytesValue = b},
            _ when value is ArrayValue a => new OtelAnyValue() { ArrayValue = a},
            _ when value is KeyValueList kv => new OtelAnyValue() { KvlistValue = kv},
            _ => throw new NotImplementedException(), // TODO Is this the right exception?
        };

        return new KeyValue() { Key = key, Value = anyValue };
    }
}