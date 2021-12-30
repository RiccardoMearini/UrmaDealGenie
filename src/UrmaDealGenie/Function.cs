using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using XCommas.Net.Objects;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace UrmaDealGenie
{
  /// <summary>
  ///  The main AWS Lambda Function class and handler
  /// </summary>
  public class Function
  {
    private Urma3cClient client = null;

    /// <summary>
    /// Modify the TP % of all deals that have botnames that match the specified include/exclude terms.
    /// The TP is modified based on the modifier and the deal's completed SO count.
    /// Environment variables are used as follows:
    /// e.g. IncludeTerms = "urma,hodl" - to modify deals with botnames containing "urma" or "hodl"
    ///      ExcludeTerms = "btc,express bot,accum" - to NOT modify deals with botnames containing "btc" or "express bot" or "accum"
    ///      TpScale = 1.05 - to modify each matching deals' TP % to equal 1.05 x completed SO count
    ///      MaxSafetyOrderCount = modify deals less than max SO count
    ///      IgnoreTtpDeals = true - ignore deals with TTP enabled
    /// </summary>
    /// <param name="input">Input deal rules, or flag to trigger load from S3</param>
    /// <param name="context">Context of the lambda</param>
    /// <returns>Result summary of the updated deals</returns>
    public async Task<List<DealResponse>> FunctionHandler(DealRuleSet dealRuleSet, ILambdaContext context)
    {
      List<DealResponse> response = new List<DealResponse>();
      var apiKey = Environment.GetEnvironmentVariable("APIKEY");
      var secret = Environment.GetEnvironmentVariable("SECRET");
      var bucket = Environment.GetEnvironmentVariable("BUCKET");

      if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(secret))
      {
        Console.WriteLine($"ERROR: Missing APIKEY and/or SECRET environment variables");
      }
      else
      {
        Console.WriteLine($"LoadFromS3 = {dealRuleSet.LoadFromS3}");
        if (dealRuleSet.LoadFromS3)
        {
          // Load deal rules configuration from S3 bucket
          RegionEndpoint region = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION"));
          string dealRulesString = await BucketFileReader.ReadObjectDataAsync(region, bucket, "dealrules.json");
          if (!string.IsNullOrEmpty(dealRulesString))
          {
            dealRuleSet = JsonSerializer.Deserialize<DealRuleSet>(dealRulesString);
          }
          else
          {
            dealRuleSet = null;
          }
        }

        // By now we should have a deal rule set from S3 bucket or input parameter
        if (dealRuleSet != null)
        {
          client = new Urma3cClient(apiKey, secret);
          await client.RetrieveAllDeals();
          var updateDeals = dealRuleSet.UpdateDeals;
          Console.WriteLine($"updateDeals = {updateDeals}");
          Console.WriteLine($"======================");
          Console.WriteLine($"ActiveSafetyOrdersCountRangesDealRules.Count = {dealRuleSet.ActiveSafetyOrdersCountRangesDealRules.Count}");
          foreach (ActiveSafetyOrdersCountRangesDealRule dealRule in dealRuleSet.ActiveSafetyOrdersCountRangesDealRules)
          {
            var updatedDeal = await ProcessDealRule(dealRule, updateDeals);
            response.Add(updatedDeal);
          }

          Console.WriteLine($"======================");
          Console.WriteLine($"SafetyOrderRangesDealRules.Count = {dealRuleSet.SafetyOrderRangesDealRules.Count}");
          foreach (SafetyOrderRangesDealRule dealRule in dealRuleSet.SafetyOrderRangesDealRules)
          {
            var updatedDeal = await ProcessDealRule(dealRule, updateDeals);
            response.Add(updatedDeal);
          }

          Console.WriteLine($"======================");
          Console.WriteLine($"ScalingTakeProfitDealRules.Count = {dealRuleSet.ScalingTakeProfitDealRules.Count}");
          foreach (ScalingTakeProfitDealRule dealRule in dealRuleSet.ScalingTakeProfitDealRules)
          {
            var updatedDeal = await ProcessDealRule(dealRule, updateDeals);
            response.Add(updatedDeal);
          }
        }
        else
        {
          Console.WriteLine($"Error: cannot find deal rules from input parameter or S3 bucket");
        }
      }
      return response;
    }

    /// Process the Scaling Take Profit deal rule
    /// <param name="dealRule">Deal rule to be processed</param>
    /// <param name="updateDeal">If true, update this deal</param>
    /// <returns>Result summary of the updated deal</returns>
    public async Task<DealResponse> ProcessDealRule(ScalingTakeProfitDealRule dealRule, bool updateDeal)
    {
      var rule = dealRule.Rule;
      var includeTerms = dealRule.BotNameIncludeTerms;
      var excludeTerms = dealRule.BotNameExcludeTerms;
      var ignoreTtpDeals = dealRule.IgnoreTtpDeals;
      var allowTpReduction = dealRule.AllowTpReduction;
      var maxSoCount = dealRule.MaxSafetyOrderCount;
      var scaleTp = dealRule.TpScale;

      Console.WriteLine($"----------------------");
      Console.WriteLine($"RULE = {rule}");
      Console.WriteLine($" includeTerms = {includeTerms}");
      Console.WriteLine($" excludeTerms = {excludeTerms}");
      Console.WriteLine($" ignoreTtpDeals = {ignoreTtpDeals}");
      Console.WriteLine($" allowTpReduction = {allowTpReduction}");
      Console.WriteLine($" maxSoCount = {maxSoCount}");
      Console.WriteLine($" scaleTp = {scaleTp}");

      DealResponse response = new DealResponse()
      {
        Rule = rule,
        NeedUpdatingCount = 0
      };

      if (scaleTp > 0)
      {
        // Get list of deals needing updating
        List<Deal> deals = client.GetMatchingDealsScalingTakeProfit(includeTerms.Split(','), excludeTerms.Split(','), ignoreTtpDeals, allowTpReduction, scaleTp, maxSoCount);
        response.NeedUpdatingCount = deals.Count;
        Console.WriteLine($"Found {response.NeedUpdatingCount} deals needing updating");

        // #### Refactor this bit out to a separate method
        if (updateDeal)
        {
          if (deals.Count > 0)
          {
            // Get a nice output of the deals to log
            string outputDealSummaries = client.GetDealSummariesText(deals);
            Console.WriteLine(outputDealSummaries);

            if (updateDeal)
            {
              Console.WriteLine("Updating TP% for each deal...");

              // Automatically update the take profit of each deal using the specified TP scale modifier
              int updatedCount = await client.UpdateDealsScalingTakeProfit(deals, scaleTp, maxSoCount);
              response.UpdatedCount = updatedCount;
              Console.WriteLine($"Updated {response.UpdatedCount} deals");
            }
          }
        }
      }
      else
      {
        // TODO throw errors and return errors in responses
        Console.WriteLine($"Error: scaleTp must be more than 0");
      }
      Console.WriteLine($"");
      return response;
    }

    /// Process the Active Safety Order Count ranges deal rule
    /// <param name="dealRule">Deal rule to be processed</param>
    /// <param name="updateDeal">If true, update this deal</param>
    /// <returns>Result summary of the updated deal</returns>
    public async Task<DealResponse> ProcessDealRule(ActiveSafetyOrdersCountRangesDealRule dealRule, bool updateDeal)
    {
      var rule = dealRule.Rule;
      var includeTerms = dealRule.BotNameIncludeTerms;
      var excludeTerms = dealRule.BotNameExcludeTerms;
      var ignoreTtpDeals = dealRule.IgnoreTtpDeals;
      var mastcRanges = dealRule.ActiveSafetyOrdersCountRanges;

      Console.WriteLine($"----------------------");
      Console.WriteLine($"RULE = {rule}");
      Console.WriteLine($" includeTerms = {includeTerms}");
      Console.WriteLine($" excludeTerms = {excludeTerms}");
      Console.WriteLine($" ignoreTtpDeals = {ignoreTtpDeals}");

      Dictionary<int, int> soRangesDictionary = GetActiveSafetyOrdersCountRangesDictionary(dealRule);
      // Get list of deals needing updating
      List<Deal> deals = client.GetMatchingDealsActiveSafetyOrdersCountRanges(
        includeTerms.Split(','),
        excludeTerms.Split(','),
        ignoreTtpDeals,
        soRangesDictionary);

      DealResponse response = new DealResponse() { Rule = rule, NeedUpdatingCount = deals.Count };
      Console.WriteLine($"Found {response.NeedUpdatingCount} deals needing updating");

      // #### Refactor this bit out to a separate method
      if (updateDeal)
      {
        if (deals.Count > 0)
        {
          // Get a nice output of the deals to log
          string outputDealSummaries = client.GetDealSummariesText(deals);
          Console.WriteLine(outputDealSummaries);

          if (updateDeal)
          {
            Console.WriteLine("Updating TP% for each deal...");

            // Automatically update the take profit of each deal using the specified TP scale modifier
            int updatedCount = await client.UpdateDealsActiveSafetyOrderCountRanges(deals, soRangesDictionary);
            response.UpdatedCount = updatedCount;
            Console.WriteLine($"Updated {response.UpdatedCount} deals");
          }
        }
      }
      Console.WriteLine($"");
      return response;
    }

    /// Process the Safety Order Ranges deal rule
    /// <param name="dealRule">Deal rule to be processed</param>
    /// <param name="updateDeal">If true, update this deal</param>
    /// <returns>Result summary of the updated deal</returns>
    public async Task<DealResponse> ProcessDealRule(SafetyOrderRangesDealRule dealRule, bool updateDeal)
    {
      var rule = dealRule.Rule;
      var includeTerms = dealRule.BotNameIncludeTerms;
      var excludeTerms = dealRule.BotNameExcludeTerms;
      var ignoreTtpDeals = dealRule.IgnoreTtpDeals;
      var allowTpReduction = dealRule.AllowTpReduction;
      var soRanges = dealRule.SafetyOrderRanges;

      Console.WriteLine($"----------------------");
      Console.WriteLine($"RULE = {rule}");
      Console.WriteLine($" includeTerms = {includeTerms}");
      Console.WriteLine($" excludeTerms = {excludeTerms}");
      Console.WriteLine($" ignoreTtpDeals = {ignoreTtpDeals}");
      Console.WriteLine($" allowTpReduction = {allowTpReduction}");

      Dictionary<int, decimal> soRangesDictionary = GetSafetyOrderRangesDictionary(dealRule);
      // Get list of deals needing updating
      List<Deal> deals = client.GetMatchingDealsSafetyOrderRanges(
        includeTerms.Split(','),
        excludeTerms.Split(','),
        ignoreTtpDeals,
        allowTpReduction,
        soRangesDictionary);

      DealResponse response = new DealResponse() { Rule = rule, NeedUpdatingCount = deals.Count };
      Console.WriteLine($"Found {response.NeedUpdatingCount} deals needing updating");

      // #### Refactor this bit out to a separate method
      if (updateDeal)
      {
        if (deals.Count > 0)
        {
          // Get a nice output of the deals to log
          string outputDealSummaries = client.GetDealSummariesText(deals);
          Console.WriteLine(outputDealSummaries);

          if (updateDeal)
          {
            Console.WriteLine("Updating TP% for each deal...");

            // Automatically update the take profit of each deal using the specified TP scale modifier
            int updatedCount = await client.UpdateDealsSafetyOrderRanges(deals, soRangesDictionary);
            response.UpdatedCount = updatedCount;
            Console.WriteLine($"Updated {response.UpdatedCount} deals");
          }
        }
      }
      Console.WriteLine($"");
      return response;
    }

    /// <summary>
    /// Return a complete dictionary of safety order ranges and their take profits,
    /// according to specified deal rule.  This will include each safety order level from 1 upwards.
    /// </summary>
    /// <param name="dealRule">The safety order ranges deal rule</param>
    /// <returns>Dictionary of safety order ranges and their take profits</returns>
    private Dictionary<int, decimal> GetSafetyOrderRangesDictionary(SafetyOrderRangesDealRule dealRule)
    {
      var soRangesDictionary = new Dictionary<int, decimal>();
      int start = int.Parse(dealRule.SafetyOrderRanges.First().Key);
      int end = start;
      decimal takeProfit = 0;
      foreach (var soRange in dealRule.SafetyOrderRanges.Keys)
      {
        end = int.Parse(soRange);
        for (int safetyOrder = start; safetyOrder < end; safetyOrder++)
        {
          soRangesDictionary.Add(safetyOrder, takeProfit);
          Console.WriteLine($" soRangesDictionary: {safetyOrder} = {takeProfit}% TP");
        }
        takeProfit = dealRule.SafetyOrderRanges[soRange];
        start = end;
      }
      // Add the last SO level (which will include all higher SO counts)
      soRangesDictionary.Add(end, takeProfit);
      Console.WriteLine($" soRangesDictionary: {end}+ = {takeProfit}% TP");
      return soRangesDictionary;
    }

    /// <summary>
    /// Return a complete dictionary of safety order ranges and their Active Safety Orders Count (aka MASTC),
    /// according to specified deal rule.  This will include each safety order level from 1 upwards.
    /// </summary>
    /// <param name="dealRule">The Active Safety Orders Count ranges deal rule</param>
    /// <returns>Dictionary of safety order ranges and their take Active Safety Orders Count (aka MASTC)</returns>
    private Dictionary<int, int> GetActiveSafetyOrdersCountRangesDictionary(ActiveSafetyOrdersCountRangesDealRule dealRule)
    {
      var soRangesDictionary = new Dictionary<int, int>();
      int start = int.Parse(dealRule.ActiveSafetyOrdersCountRanges.First().Key);
      int end = start;
      int mastc = 0;
      foreach (var soRange in dealRule.ActiveSafetyOrdersCountRanges.Keys)
      {
        end = int.Parse(soRange);
        for (int safetyOrder = start; safetyOrder < end; safetyOrder++)
        {
          soRangesDictionary.Add(safetyOrder, mastc);
          Console.WriteLine($" soRangesDictionary: {safetyOrder} = {mastc} MASTC");
        }
        mastc = dealRule.ActiveSafetyOrdersCountRanges[soRange];
        start = end;
      }
      // Add the last SO level (which will include all higher SO counts)
      soRangesDictionary.Add(end, mastc);
      Console.WriteLine($" soRangesDictionary: {end}+ = {mastc} MASTC");
      return soRangesDictionary;
    }
  }
}
