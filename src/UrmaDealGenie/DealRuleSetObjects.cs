using System.Collections.Generic;

/// <summary>
/// Serialisable input and response classes for the lambda function.
/// </summary>
namespace UrmaDealGenie
{
  public class DealRuleSet // #### consider renaming as this is not just dealrules not, it's bots too
  {
    public bool LoadFromS3 { get; set; }
    public bool UpdateDeals { get; set; } // #### consider renaming to UpdateMode so that it affects bot pairs and deal settings
    public List<LunarCrushAltRankPairRule> LunarCrushAltRankPairRules { get; set; }
    public List<LunarCrushGalaxyScorePairRule> LunarCrushGalaxyScorePairRule { get; set; }
    public List<ScalingTakeProfitDealRule> ScalingTakeProfitDealRules { get; set; }
    public List<SafetyOrderRangesDealRule> SafetyOrderRangesDealRules { get; set; }
    public List<ActiveSafetyOrdersCountRangesDealRule> ActiveSafetyOrdersCountRangesDealRules { get; set; }
  }

  public abstract class DealRuleBase
  {
    public string Rule { get; set; }
    public string BotNameIncludeTerms { get; set; }
    public string BotNameExcludeTerms { get; set; }
    public bool IgnoreTtpDeals { get; set; }
  }

  public class LunarCrushPairRule
  {
    public int BotId { get; set; }
    public int MaxPairCount { get; set; }
    public string BlacklistPairs { get; set; }
    public int MaxAcrScore { get; set; }

  }

  public class LunarCrushAltRankPairRule : LunarCrushPairRule
  {
  }

  public class LunarCrushGalaxyScorePairRule : LunarCrushPairRule
  {
  }

  public class ScalingTakeProfitDealRule : DealRuleBase
  {
    public bool AllowTpReduction { get; set; }
    public int MaxSafetyOrderCount { get; set; }
    public decimal TpScale { get; set; }
  }

  public class SafetyOrderRangesDealRule : DealRuleBase
  {
    public bool AllowTpReduction { get; set; }
    public Dictionary<string, decimal> SafetyOrderRanges { get; set; }
  }
  
  public class ActiveSafetyOrdersCountRangesDealRule : DealRuleBase
  {
    public Dictionary<string, int> ActiveSafetyOrdersCountRanges { get; set; }
  }  

  public class DealResponse
  {
    public string Rule { get; set; }
    public int NeedUpdatingCount { get; set; }
    public int UpdatedCount { get; set; }
  }
}