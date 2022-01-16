using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LunarCrush.Objects
{
  // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
  public class Config
  {
    [JsonPropertyName("data")]
    public string Data { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }
  }

  public class Datum
  {
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("s")]
    public string S { get; set; }

    [JsonPropertyName("v")]
    public double V { get; set; }

    [JsonPropertyName("gs")]
    public double Gs { get; set; }

    [JsonPropertyName("acr")]
    public double Acr { get; set; }

    [JsonPropertyName("categories")]
    public string Categories { get; set; }

  }

  public class Root
  {
    [JsonPropertyName("config")]
    public Config Config { get; set; }

    [JsonPropertyName("data")]
    public List<Datum> Data { get; set; }
  }

}