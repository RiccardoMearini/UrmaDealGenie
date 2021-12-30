using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using XCommas.Net;
using XCommas.Net.Objects;

namespace UrmaDealGenie
{
  public class Urma3cClient
  {
    private XCommasApi client = null;
    private List<Deal> cachedDeals = new List<Deal>();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="apiKey">3commas account API key</param>
    /// <param name="secret">3commas account API secret</param>
    public Urma3cClient(string apiKey, string secret)
    {
      client = new XCommasApi(apiKey, secret);
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
    /// <returns>List of deals needing to be updated</returns>
    public List<Deal> GetMatchingDealsScalingTakeProfit(
      string[] includeTerms, string[] excludeTerms,
      bool ignoreTtpDeals, bool allowTpReduction,
      decimal scaleTp, int maxSoCount)
    {
      Console.WriteLine($"GetMatchingDealsScalingTakeProfit()");
      List<Deal> dealsToUpdate = new List<Deal>();
      foreach (var deal in cachedDeals)
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
          var response = await client.UpdateDealAsync(deal.Id, update);
          if (!response.IsSuccess)
          {
            Console.WriteLine($"Error: UpdateDealsScalingTakeProfit: client.UpdateDealAsync() - {response.Error}");
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
    /// <returns>List of deals needing to be updated</returns>
    public List<Deal> GetMatchingDealsSafetyOrderRanges(
      string[] includeTerms, string[] excludeTerms,
      bool ignoreTtpDeals, bool allowTpReduction,
      Dictionary<int, decimal> soRangesDictionary)
    {
      Console.WriteLine($"GetMatchingDealsSafetyOrderRanges()");
      List<Deal> dealsToUpdate = new List<Deal>();
      foreach (var deal in cachedDeals)
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
          var response = await client.UpdateDealAsync(deal.Id, update);
          if (!response.IsSuccess)
          {
            Console.WriteLine($"Error: UpdateDealsSafetyOrderRanges: client.UpdateDealAsync() - {response.Error}");
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
    /// <returns>List of deals needing to be updated</returns>
    public List<Deal> GetMatchingDealsActiveSafetyOrdersCountRanges(
      string[] includeTerms, string[] excludeTerms,
      bool ignoreTtpDeals,
      Dictionary<int, int> soRangesDictionary)
    {
      Console.WriteLine($"GetMatchingDealsActiveSafetyOrdersCountRanges()");
      List<Deal> dealsToUpdate = new List<Deal>();
      foreach (var deal in cachedDeals)
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
          var response = await client.UpdateDealAsync(deal.Id, update);
          if (!response.IsSuccess)
          {
            Console.WriteLine($"Error: UpdateDealsActiveSafetyOrderCountRanges: client.UpdateDealAsync() - {response.Error}");
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
    /// Get deals if they haven't been retrieved already and cache them
    /// </summary>
    public async Task RetrieveAllDeals()
    {
      if (this.cachedDeals == null || this.cachedDeals.Count == 0)
      {
        var response = await client.GetDealsAsync(limit: 100, dealScope: DealScope.Active, dealOrder: DealOrder.CreatedAt);
        if (response.IsSuccess)
        {
          Console.WriteLine($"Retrieved {response.Data.Length} deals from 3Commas");
          foreach (var deal in response.Data)
          {
            var trailingTp = deal.IsTrailingEnabled ? $"TTP({deal.TrailingDeviation})" : "TP";
            Console.WriteLine($"{deal.BotName}.{deal.Pair} = SO {deal.CompletedSafetyOrdersCount}, {trailingTp} {deal.TakeProfit}%, MASTC {deal.ActiveSafetyOrdersCount}");
            this.cachedDeals.Add(deal);
          }
          Console.WriteLine($"Cached {cachedDeals.Count} deals");
        }
        else
        {
          Console.WriteLine($"Error: GetDeals(): client.GetDealsAsync() - {response.Error}");
        }
      }
    }
  }
}