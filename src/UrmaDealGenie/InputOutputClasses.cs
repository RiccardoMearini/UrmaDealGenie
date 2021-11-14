using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Serialisable input and response classes for the lambda function.
/// </summary>
namespace UrmaDealGenie
{
  public enum RuleType
  {
    ScaleTp,
    FixedTp
  }
  public class DealRule
  {
    public string Rule { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RuleType RuleType { get; set; }
    public string BotNameIncludeTerms { get; set; }
    public string BotNameExcludeTerms { get; set; }
    public bool IgnoreTtpDeals { get; set; }
    public int MaxSafetyOrderCount { get; set; }
    public decimal TpScale { get; set; }
  }

  public class Root
  {
    public bool UpdateDeals { get; set; }
    public List<DealRule> DealRules { get; set; }
  }

  public class DealResponse
  {
    public string Rule { get; set; }
    public int NeedUpdatingCount { get; set; }
    public int UpdatedCount { get; set; }
  }
}