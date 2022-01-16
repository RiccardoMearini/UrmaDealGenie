using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// Custom Json Serializer class to support enum/string conversions for AWS Lambda functions
public class JsonEnumSerializer : Amazon.Lambda.Serialization.Json.JsonSerializer
{
  public JsonEnumSerializer()
    : base(CustomizeSerializerSettings)
  {
  }

  private static void CustomizeSerializerSettings(JsonSerializerSettings serializerSettings)
  {
    serializerSettings.Converters = new List<JsonConverter>
    {
      new StringEnumConverter()
    };
  }
}