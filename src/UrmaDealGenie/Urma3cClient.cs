using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XCommas.Net;
using XCommas.Net.Objects;

namespace UrmaDealGenie
{
  public class Urma3cClient
  {
    public XCommasApi XCommasClient = null;
    private List<Deal> cachedRealDeals = null;
    private List<Deal> cachedPaperDeals = null;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="apiKey">The 3Commas API key</param>
    /// <param name="secret">The 3Commas secret</param>
    public Urma3cClient(string apiKey, string secret)
    {
      XCommasClient = new XCommasApi(apiKey, secret);
    }

    /// <summary>
    /// Process the specified deal rule set
    /// </summary>
    /// <param name="dealRuleSet">The deal rule set to process</param>
    /// <returns>Result summary of the updated deals</returns>
    public async Task<DealGenieResponse> ProcessRules(DealRuleSet dealRuleSet)
    {
      DealGenieResponse response = new DealGenieResponse();
      var update = dealRuleSet.Update;
      Console.WriteLine($"update = {update}");

      // Process the LunarCrush rule sets
      LunarCrushPairRuleProcessor crush = new LunarCrushPairRuleProcessor(this.XCommasClient);
      if (dealRuleSet.LunarCrushPairRules?.Any() ?? false)
      {
        response.BotPairResponses = await crush.ProcessRules(dealRuleSet.LunarCrushPairRules, update);
      }

      if (dealRuleSet.ActiveSafetyOrdersCountRangesDealRules?.Any() ?? false)
      {
        Console.WriteLine($"==================================================");
        Console.WriteLine($"ActiveSafetyOrdersCountRangesDealRules.Count = {dealRuleSet.ActiveSafetyOrdersCountRangesDealRules.Count}");
        foreach (ActiveSafetyOrdersCountRangesDealRule dealRule in dealRuleSet.ActiveSafetyOrdersCountRangesDealRules)
        {
          var updatedDeal = await ProcessDealRule(dealRule, update);
          response.DealResponses.Add(updatedDeal);
        }
      }

      if (dealRuleSet.SafetyOrderRangesDealRules?.Any() ?? false)
      {
        Console.WriteLine($"==================================================");
        Console.WriteLine($"SafetyOrderRangesDealRules.Count = {dealRuleSet.SafetyOrderRangesDealRules.Count}");
        foreach (SafetyOrderRangesDealRule dealRule in dealRuleSet.SafetyOrderRangesDealRules)
        {
          var updatedDeal = await ProcessDealRule(dealRule, update);
          response.DealResponses.Add(updatedDeal);
        }
      }

      if (dealRuleSet.ScalingTakeProfitDealRules?.Any() ?? false)
      {
        Console.WriteLine($"==================================================");
        Console.WriteLine($"ScalingTakeProfitDealRules.Count = {dealRuleSet.ScalingTakeProfitDealRules.Count}");
        foreach (ScalingTakeProfitDealRule dealRule in dealRuleSet.ScalingTakeProfitDealRules)
        {
          var updatedDeal = await ProcessDealRule(dealRule, update);
          response.DealResponses.Add(updatedDeal);
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
      var paper = dealRule.Paper;
      var userMode = paper ? UserMode.Paper : UserMode.Real;
      var includeTerms = dealRule.BotNameIncludeTerms;
      var excludeTerms = dealRule.BotNameExcludeTerms;
      var ignoreTtpDeals = dealRule.IgnoreTtpDeals;
      var allowTpReduction = dealRule.AllowTpReduction;
      var maxSoCount = dealRule.MaxSafetyOrderCount;
      var scaleTp = dealRule.TpScale;

      Console.WriteLine($"----------------------");
      Console.WriteLine($"RULE = {rule}");
      Console.WriteLine($" userMode = {userMode}");
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
        List<Deal> deals = await GetMatchingDealsScalingTakeProfit(
          includeTerms.Split(','), excludeTerms.Split(','),
          ignoreTtpDeals, allowTpReduction, scaleTp, maxSoCount, userMode);

        response.NeedUpdatingCount = deals.Count;
        Console.WriteLine($"Found {response.NeedUpdatingCount} deals needing updating");

        // #### Refactor this bit out to a separate method
        if (updateDeal)
        {
          if (deals.Count > 0)
          {
            // Get a nice output of the deals to log
            string outputDealSummaries = GetDealSummariesText(deals);
            Console.WriteLine(outputDealSummaries);

            if (updateDeal)
            {
              Console.WriteLine("Updating TP% for each deal...");

              // Automatically update the take profit of each deal using the specified TP scale modifier
              int updatedCount = await UpdateDealsScalingTakeProfit(deals, scaleTp, maxSoCount);
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
      var paper = dealRule.Paper;
      var userMode = paper ? UserMode.Paper : UserMode.Real;
      var includeTerms = dealRule.BotNameIncludeTerms;
      var excludeTerms = dealRule.BotNameExcludeTerms;
      var ignoreTtpDeals = dealRule.IgnoreTtpDeals;
      var mastcRanges = dealRule.ActiveSafetyOrdersCountRanges;

      Console.WriteLine($"----------------------");
      Console.WriteLine($"RULE = {rule}");
      Console.WriteLine($" userMode = {userMode}");
      Console.WriteLine($" includeTerms = {includeTerms}");
      Console.WriteLine($" excludeTerms = {excludeTerms}");
      Console.WriteLine($" ignoreTtpDeals = {ignoreTtpDeals}");

      Dictionary<int, int> soRangesDictionary = GetActiveSafetyOrdersCountRangesDictionary(dealRule);
      // Get list of deals needing updating
      List<Deal> deals = await GetMatchingDealsActiveSafetyOrdersCountRanges(
        includeTerms.Split(','),
        excludeTerms.Split(','),
        ignoreTtpDeals,
        soRangesDictionary,
        userMode);

      DealResponse response = new DealResponse() { Rule = rule, NeedUpdatingCount = deals.Count };
      Console.WriteLine($"Found {response.NeedUpdatingCount} deals needing updating");

      // #### Refactor this bit out to a separate method
      if (updateDeal)
      {
        if (deals.Count > 0)
        {
          // Get a nice output of the deals to log
          string outputDealSummaries = GetDealSummariesText(deals);
          Console.WriteLine(outputDealSummaries);

          if (updateDeal)
          {
            Console.WriteLine("Updating TP% for each deal...");

            // Automatically update the take profit of each deal using the specified TP scale modifier
            int updatedCount = await UpdateDealsActiveSafetyOrderCountRanges(deals, soRangesDictionary);
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
      var paper = dealRule.Paper;
      var userMode = paper ? UserMode.Paper : UserMode.Real;
      var includeTerms = dealRule.BotNameIncludeTerms;
      var excludeTerms = dealRule.BotNameExcludeTerms;
      var ignoreTtpDeals = dealRule.IgnoreTtpDeals;
      var allowTpReduction = dealRule.AllowTpReduction;
      var soRanges = dealRule.SafetyOrderRanges;

      Console.WriteLine($"----------------------");
      Console.WriteLine($"RULE = {rule}");
      Console.WriteLine($" userMode = {userMode}");
      Console.WriteLine($" includeTerms = {includeTerms}");
      Console.WriteLine($" excludeTerms = {excludeTerms}");
      Console.WriteLine($" ignoreTtpDeals = {ignoreTtpDeals}");
      Console.WriteLine($" allowTpReduction = {allowTpReduction}");

      Dictionary<int, decimal> soRangesDictionary = GetSafetyOrderRangesDictionary(dealRule);
      // Get list of deals needing updating
      List<Deal> deals = await GetMatchingDealsSafetyOrderRanges(
        includeTerms.Split(','),
        excludeTerms.Split(','),
        ignoreTtpDeals,
        allowTpReduction,
        soRangesDictionary,
        userMode);

      DealResponse response = new DealResponse() { Rule = rule, NeedUpdatingCount = deals.Count };
      Console.WriteLine($"Found {response.NeedUpdatingCount} deals needing updating");

      // #### Refactor this bit out to a separate method
      if (updateDeal)
      {
        if (deals.Count > 0)
        {
          // Get a nice output of the deals to log
          string outputDealSummaries = GetDealSummariesText(deals);
          Console.WriteLine(outputDealSummaries);

          if (updateDeal)
          {
            Console.WriteLine("Updating TP% for each deal...");

            // Automatically update the take profit of each deal using the specified TP scale modifier
            int updatedCount = await UpdateDealsSafetyOrderRanges(deals, soRangesDictionary);
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

    /// <summary>
    /// Get list of deals that match the criteria for scaling take profit.
    /// Only return deals that match the include terms, but not the exclude terms, and that need TP updating.
    /// Skip deals that haven't completed any SO yet.
    /// </summary>
    /// <param name="includeTerms">String array of terms that a bot name must contain</param>
    /// <param name="excludeTerms">String array of terms that a bot name must NOT contain</param>
    /// <param name="ignoreTtpDeals">Ignore TTP deals if True</param>
    /// <param name="allowTpReduction">Allow new TP to be less than the current TP</param>
    /// <param name="scaleTp">Scale modifier to use on the completed SO count when checking the TP %</param>    
    /// <param name="maxSoCount">If more than 0, set TP based on a max SO count</param>
    /// <param name="userMode">Retrieve Real account deals (default) or Paper account deals</param>
    /// <returns>List of deals needing to be updated</returns>
    public async Task<List<Deal>> GetMatchingDealsScalingTakeProfit(
      string[] includeTerms, string[] excludeTerms,
      bool ignoreTtpDeals, bool allowTpReduction,
      decimal scaleTp, int maxSoCount, UserMode userMode)
    {
      Console.WriteLine($"GetMatchingDealsScalingTakeProfit()");
      var deals = await GetCachedDeals(userMode);

      List<Deal> dealsToUpdate = new List<Deal>();
      foreach (var deal in deals)
      {
        // Ignore TTP deals if configured to
        if (!deal.IsTrailingEnabled || (deal.IsTrailingEnabled && !ignoreTtpDeals))
        {
          // Include deals with botnames containing any of these terms 
          if (includeTerms[0].Length == 0 ||
            includeTerms.Any(s => deal.BotName.Contains(s, StringComparison.CurrentCultureIgnoreCase)))
          {
            // Exclude deals with botnames containing any of these terms 
            if (excludeTerms[0].Length == 0 ||
              !excludeTerms.Any(s => deal.BotName.Contains(s, StringComparison.CurrentCultureIgnoreCase)))
            {
              if (deal.CompletedSafetyOrdersCount > 0)
              {
                // Determine the SO count to compare against - if a max SO count is specified, don't go higher than it
                var soCount = maxSoCount > 0 ? Math.Min(maxSoCount, deal.CompletedSafetyOrdersCount) : deal.CompletedSafetyOrdersCount;

                // This deal needs updating, but only if the TP needs increasing
                // OR TP needs decreasing and we're allowed to reduce TP
                if (deal.TakeProfit < soCount * scaleTp || (soCount * scaleTp < deal.TakeProfit && allowTpReduction))
                {
                  Console.WriteLine($"'{deal.BotName}':'{deal.Pair}' TP {deal.TakeProfit}% needs updating - SO {deal.CompletedSafetyOrdersCount} => Set TP {soCount * scaleTp} %");
                  dealsToUpdate.Add(deal);
                }
              }
            }
          }
        }
      }
      return dealsToUpdate;
    }

    /// <summary>
    /// Updates the take profit of the given deals according to the deals list, scaling and max SO count
    /// </summary>
    /// <param name="dealsToUpdate">List of deals to modify</param>
    /// <param name="scaleTp">Scale modifier to use on the completed SO count when setting the TP %</param>
    /// <param name="maxSoCount">If more than 0, set TP based on a max SO count</param>
    /// <returns>Count of updated deals</returns>
    public async Task<int> UpdateDealsScalingTakeProfit(List<Deal> dealsToUpdate, decimal scaleTp, int maxSoCount)
    {
      if (dealsToUpdate.Count > 0)
      {
        foreach (Deal deal in dealsToUpdate)
        {
          DealUpdateData update = new DealUpdateData(deal.Id);
          // Determine the SO count to compare against - if a max SO count is specified, don't go higher than it
          var soCount = maxSoCount > 0 ? Math.Min(maxSoCount, deal.CompletedSafetyOrdersCount) : deal.CompletedSafetyOrdersCount;
          update.TakeProfit = soCount * scaleTp;
          var response = await XCommasClient.UpdateDealAsync(deal.Id, update);
          if (!response.IsSuccess)
          {
            Console.WriteLine($"Error: UpdateDealsScalingTakeProfit: UpdateDealAsync() - {response.Error}");
          }
        }
      }
      return dealsToUpdate.Count;
    }

    /// <summary>
    /// Get list of deals that match the criteria for ranges of safety orders with a take profit value.
    /// Only return deals that match the include terms, but not the exclude terms, and that need TP updating.
    /// </summary>
    /// <param name="includeTerms">String array of terms that a bot name must contain</param>
    /// <param name="excludeTerms">String array of terms that a bot name must NOT contain</param>
    /// <param name="ignoreTtpDeals">Ignore TTP deals if True</param>
    /// <param name="allowTpReduction">Allow new TP to be less than the current TP</param>
    /// <param name="soRangesDictionary">Lookup of Take Profits from dictionary of Safety Order ranges</param>
    /// <param name="userMode">Retrieve Real account deals (default) or Paper account deals</param>
    /// <returns>List of deals needing to be updated</returns>
    public async Task<List<Deal>> GetMatchingDealsSafetyOrderRanges(
      string[] includeTerms, string[] excludeTerms,
      bool ignoreTtpDeals, bool allowTpReduction,
      Dictionary<int, decimal> soRangesDictionary, UserMode userMode)
    {
      Console.WriteLine($"GetMatchingDealsSafetyOrderRanges()");
      List<Deal> dealsToUpdate = new List<Deal>();
      var deals = await GetCachedDeals(userMode);
      foreach (var deal in deals)
      {
        // Ignore TTP deals if configured to
        if (!deal.IsTrailingEnabled || (deal.IsTrailingEnabled && !ignoreTtpDeals))
        {
          // Include deals with botnames containing any of these terms 
          if (includeTerms[0].Length == 0 ||
            includeTerms.Any(s => deal.BotName.Contains(s, StringComparison.CurrentCultureIgnoreCase)))
          {
            // Exclude deals with botnames containing any of these terms 
            if (excludeTerms[0].Length == 0 ||
              !excludeTerms.Any(s => deal.BotName.Contains(s, StringComparison.CurrentCultureIgnoreCase)))
            {
              // Lookup the closest TP to use from the completed safety count
              var lookupResult = soRangesDictionary.Where(x => x.Key <= deal.CompletedSafetyOrdersCount)
                                                  .OrderByDescending(x => x.Key);
              // Console.WriteLine($"  ## '{deal.BotName}':'{deal.Pair}', SO {deal.CompletedSafetyOrdersCount}, lookupResult.Count {lookupResult.Count()}");

              if (lookupResult != null && lookupResult.Count() >= 1)
              {
                int closestSo = lookupResult.First().Key;
                decimal newTp = soRangesDictionary[closestSo];
                Console.WriteLine($"  ## '{deal.BotName}':'{deal.Pair}', SO {deal.CompletedSafetyOrdersCount}, closest SO lookup {closestSo}");

                // This deal needs updating, but only if the TP needs increasing
                // OR TP needs decreasing and we're allowed to reduce TP
                if (deal.TakeProfit < newTp || (newTp < deal.TakeProfit && allowTpReduction))
                {
                  Console.WriteLine($"Deal '{deal.BotName}':'{deal.Pair}', TP {deal.TakeProfit}% needs updating - SO {deal.CompletedSafetyOrdersCount} => new TP {newTp}%");
                  dealsToUpdate.Add(deal);
                }
              }
            }
          }
        }
      }
      return dealsToUpdate;
    }

    /// <summary>
    /// Updates the take profit of the given deals according to the deals list and lookup
    /// </summary>
    /// <param name="dealsToUpdate">List of deals to modify</param>
    /// <param name="soRangesDictionary">Lookup of Take Profits from dictionary of Safety Order ranges</param>
    /// <returns>Count of updated deals</returns>
    public async Task<int> UpdateDealsSafetyOrderRanges(List<Deal> dealsToUpdate, Dictionary<int, decimal> soRangesDictionary)
    {
      if (dealsToUpdate.Count > 0)
      {
        foreach (Deal deal in dealsToUpdate)
        {
          DealUpdateData update = new DealUpdateData(deal.Id);
          var lookupResult = soRangesDictionary.Where(x => x.Key <= deal.CompletedSafetyOrdersCount)
                                               .OrderByDescending(x => x.Key);
          update.TakeProfit = soRangesDictionary[lookupResult.First().Key];
          var response = await XCommasClient.UpdateDealAsync(deal.Id, update);
          if (!response.IsSuccess)
          {
            Console.WriteLine($"Error: UpdateDealsSafetyOrderRanges: UpdateDealAsync() - {response.Error}");
          }
        }
      }
      return dealsToUpdate.Count;
    }

    /// <summary>
    /// Get list of deals that match the criteria for ranges of safety orders with a Active Safety Orders Count (aka MASTC).
    /// Only return deals that match the include terms, but not the exclude terms, and that need updating.
    /// </summary>
    /// <param name="includeTerms">String array of terms that a bot name must contain</param>
    /// <param name="excludeTerms">String array of terms that a bot name must NOT contain</param>
    /// <param name="ignoreTtpDeals">Ignore TTP deals if True</param>
    /// <param name="soRangesDictionary">Lookup of Active Safety Orders Count from dictionary of Safety Order ranges</param>
    /// <param name="userMode">Retrieve Real account deals (default) or Paper account deals</param>
    /// <returns>List of deals needing to be updated</returns>
    public async Task<List<Deal>> GetMatchingDealsActiveSafetyOrdersCountRanges(
      string[] includeTerms, string[] excludeTerms,
      bool ignoreTtpDeals,
      Dictionary<int, int> soRangesDictionary, UserMode userMode)
    {
      Console.WriteLine($"GetMatchingDealsActiveSafetyOrdersCountRanges()");
      List<Deal> dealsToUpdate = new List<Deal>();
      var deals = await GetCachedDeals(userMode);
      foreach (var deal in deals)
      {
        // Ignore TTP deals if configured to
        if (!deal.IsTrailingEnabled || (deal.IsTrailingEnabled && !ignoreTtpDeals))
        {
          // Include deals with botnames containing any of these terms 
          if (includeTerms[0].Length == 0 ||
            includeTerms.Any(s => deal.BotName.Contains(s, StringComparison.CurrentCultureIgnoreCase)))
          {
            // Exclude deals with botnames containing any of these terms 
            if (excludeTerms[0].Length == 0 ||
              !excludeTerms.Any(s => deal.BotName.Contains(s, StringComparison.CurrentCultureIgnoreCase)))
            {
              // Lookup the closest MASTC to use from the completed safety count
              var lookupResult = soRangesDictionary.Where(x => x.Key <= deal.CompletedSafetyOrdersCount)
                                                   .OrderByDescending(x => x.Key);

              if (lookupResult != null && lookupResult.Count() >= 1)
              {
                int closestSo = lookupResult.First().Key;
                int newMastc = soRangesDictionary[closestSo];
                Console.WriteLine($"  ## '{deal.BotName}':'{deal.Pair}', SO {deal.CompletedSafetyOrdersCount}, closest SO lookup {closestSo}");

                // This deal needs updating
                if (deal.ActiveSafetyOrdersCount != newMastc)
                {
                  Console.WriteLine($"Deal '{deal.BotName}':'{deal.Pair}', current SO {deal.CompletedSafetyOrdersCount}, Active Safety Order Count {deal.ActiveSafetyOrdersCount} needs updating => new value {newMastc}");
                  dealsToUpdate.Add(deal);
                }
              }
            }
          }
        }
      }
      return dealsToUpdate;
    }

    /// <summary>
    /// Updates the Active Safety Order Count of the given deals according to the deals list and lookup
    /// </summary>
    /// <param name="dealsToUpdate">List of deals to modify</param>
    /// <param name="soRangesDictionary">Lookup of Active Safety Order Count from dictionary of Safety Order ranges</param>
    /// <returns>Count of updated deals</returns>
    public async Task<int> UpdateDealsActiveSafetyOrderCountRanges(List<Deal> dealsToUpdate, Dictionary<int, int> soRangesDictionary)
    {
      if (dealsToUpdate.Count > 0)
      {
        foreach (Deal deal in dealsToUpdate)
        {
          DealUpdateData update = new DealUpdateData(deal.Id);
          var lookupResult = soRangesDictionary.Where(x => x.Key <= deal.CompletedSafetyOrdersCount)
                                               .OrderByDescending(x => x.Key);
          update.MaxSafetyOrdersCount = soRangesDictionary[lookupResult.First().Key];
          var response = await XCommasClient.UpdateDealAsync(deal.Id, update);
          if (!response.IsSuccess)
          {
            Console.WriteLine($"Error: UpdateDealsActiveSafetyOrderCountRanges: UpdateDealAsync() - {response.Error}");
          }
        }
      }
      return dealsToUpdate.Count;
    }

    /// <summary>
    /// Returns a string containing a summary of the given deals 
    /// </summary>
    /// <param name="dealsToSummarise">List of deals to return a summary of</param>
    /// <returns>Summary of deals as a string</returns>
    public string GetDealSummariesText(List<Deal> dealsToSummarise)
    {
      var dealSummaries = new List<string>();
      foreach (Deal deal in dealsToSummarise)
      {
        var trailingTp = deal.IsTrailingEnabled ? $"TTP({deal.TrailingDeviation})" : "TP";
        dealSummaries.Add($"{deal.BotName}.{deal.Pair} = SO {deal.CompletedSafetyOrdersCount}, {trailingTp} {deal.TakeProfit}%, MASTC {deal.ActiveSafetyOrdersCount} ");
      }
      return string.Join("\r\n", dealSummaries);
    }

    /// <summary>
    /// Get cached deals for the specified usermode, fill the cache if it's null
    /// </summary>
    /// <param name="userMode">Retrieve Real account deals or Paper account deals</param>
    /// <returns>List of deals</returns>
    private async Task<List<Deal>> GetCachedDeals(UserMode userMode)
    {
      var deals = (userMode == UserMode.Real ? this.cachedRealDeals : this.cachedPaperDeals);
      if (deals == null)
      {
        // Retreive and cache deals for specified user mode
        this.XCommasClient.UserMode = userMode;
        var response = await XCommasClient.GetDealsAsync(limit: 100, dealScope: DealScope.Active, dealOrder: DealOrder.CreatedAt);
        if (response.IsSuccess)
        {
          deals = response.Data.ToList();
          Console.WriteLine($"Caching {deals.Count} {userMode} deals");
          switch (userMode)
          {
            case UserMode.Real: this.cachedRealDeals = deals; break;
            case UserMode.Paper: this.cachedPaperDeals = deals; break;
          }
          // NOTE: this loop is for DEBUG only
          foreach (var deal in response.Data)
          {
            var trailingTp = deal.IsTrailingEnabled ? $"TTP({deal.TrailingDeviation})" : "TP";
            Console.WriteLine($"DEBUG: {deal.BotName}.{deal.Pair} = SO {deal.CompletedSafetyOrdersCount}, {trailingTp} {deal.TakeProfit}%, MASTC {deal.ActiveSafetyOrdersCount}");
          }
        }
        else
        {
          Console.WriteLine($"Error: GetDealsAsync() - {response.Error}");
        }
      }
      return deals;
    }
  }
}