using System.Collections.Generic;

/// <summary>
/// Serialisable input and response classes for the lambda function.
/// </summary>
namespace UrmaDealGenie
{
  public class DealRuleSet
  {
    public bool UpdateDeals { get; set; }
    public List<ScalingTakeProfitDealRule> ScalingTakeProfitDealRules { get; set; }
    public List<SafetyOrderRangesDealRule> SafetyOrderRangesDealRules { get; set; }
  }

  public abstract class DealRuleBase
  {
    public string Rule { get; set; }
    public string BotNameIncludeTerms { get; set; }
    public string BotNameExcludeTerms { get; set; }
    public bool IgnoreTtpDeals { get; set; }
    public bool AllowTpReduction { get; set; }
  }

  public class ScalingTakeProfitDealRule : DealRuleBase
  {
    public int MaxSafetyOrderCount { get; set; }
    public decimal TpScale { get; set; }
  }

  public class SafetyOrderRangesDealRule : DealRuleBase
  {
    public Dictionary<string, decimal> SafetyOrderRanges { get; set; }
  }
  
  public class DealResponse
  {
    public string Rule { get; set; }
    public int NeedUpdatingCount { get; set; }
    public int UpdatedCount { get; set; }
  }
}