using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    /// <param name="input">Json deserialized class</param>
    /// <param name="context">Context of the lambda</param>
    /// <returns>Result summary of the updated deals</returns>
    public async Task<List<DealResponse>> FunctionHandler(DealRuleSet input, ILambdaContext context)
    {
      List<DealResponse> response = new List<DealResponse>();
      client = new Urma3cClient(
        Environment.GetEnvironmentVariable("APIKEY"),
        Environment.GetEnvironmentVariable("SECRET"));

      var updateDeals = input.UpdateDeals;
      Console.WriteLine($"updateDeals = {updateDeals}");
      foreach (ScalingTakeProfitDealRule dealRule in input.ScalingTakeProfitDealRules)
      {
        var updatedDeal = await ProcessDealRule(dealRule, updateDeals);
        response.Add(updatedDeal);
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
      var maxSoCount = dealRule.MaxSafetyOrderCount;
      var scaleTp = dealRule.TpScale;

      Console.WriteLine($"RULE = {rule}");
      Console.WriteLine($"includeTerms = {includeTerms}");
      Console.WriteLine($"excludeTerms = {excludeTerms}");
      Console.WriteLine($"ignoreTtpDeals = {ignoreTtpDeals}");
      Console.WriteLine($"maxSoCount = {maxSoCount}");
      Console.WriteLine($"scaleTp = {scaleTp}");

      // Get list of deals needing updating
      List<Deal> deals = await client.GetDealsNeedingUpdate(includeTerms.Split(','), excludeTerms.Split(','), ignoreTtpDeals, scaleTp, maxSoCount);
      DealResponse response = new DealResponse() 
      { 
        Rule = rule,
        NeedUpdatingCount = deals.Count 
      };
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
            int updatedCount = await client.AutoUpdateTakeProfit(deals, scaleTp, maxSoCount);
            response.UpdatedCount = updatedCount;
            Console.WriteLine($"Updated {response.UpdatedCount} deals");
          }
        }
      }
      Console.WriteLine($"");
      return response;
    }
  }
}
