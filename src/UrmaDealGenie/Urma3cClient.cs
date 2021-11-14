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
    /// Get list of deals needing to be updated.
    /// Only return deals that match the include terms, but not the exclude terms, and haven't already been updated.
    /// Skip deals that haven't completed any SO yet.
    /// </summary>
    /// <param name="includeTerms">String array of terms that a bot name must contain</param>
    /// <param name="excludeTerms">String array of terms that a bot name must NOT contain</param>
    /// <param name="ignoreTtpDeals">Ignore TTP deals if True/param>
    /// <param name="scapeTp">Scale modifier to use on the completed SO count when checking the TP %</param>    
    /// <param name="maxSoCount">If more than 0, set TP based on a max SO count/param>
    /// <returns>List of deals needing to be updated</returns>
    public async Task<List<Deal>> GetDealsNeedingUpdate(string[] includeTerms, string[] excludeTerms, bool ignoreTtpDeals, decimal scapeTp, int maxSoCount)
    {
      var response = await client.GetDealsAsync(limit: 100, dealScope: DealScope.Active, dealOrder: DealOrder.CreatedAt);

      List<Deal> deals = new List<Deal>();
      foreach (var deal in response.Data)
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
              // Determine the SO count to compare against - if a max SO count is specified, don't go higher than it
              var soCount = maxSoCount > 0 ? Math.Min(maxSoCount, deal.CompletedSafetyOrdersCount) : deal.CompletedSafetyOrdersCount;
              if (deal.CompletedSafetyOrdersCount > 0 && deal.TakeProfit < soCount * scapeTp)
              {
                deals.Add(deal);
              }
            }
          }
        }
      }
      return deals;
    }

    /// <summary>
    /// Automatically updates the take profit of the given deals
    /// </summary>
    /// <param name="deals">List of deals to modify</param>
    /// <param name="scaleTp">Scale modifier to use on the completed SO count when setting the TP %</param>
    /// <param name="maxSoCount">If more than 0, set TP based on a max SO count/param>
    /// <returns>Count of updated deals</returns>
    public async Task<int> AutoUpdateTakeProfit(List<Deal> deals, decimal scaleTp, int maxSoCount)
    {
      if (deals.Count > 0)
      {
        foreach (Deal deal in deals)
        {
          DealUpdateData update = new DealUpdateData(deal.Id);
          // Determine the SO count to compare against - if a max SO count is specified, don't go higher than it
          var soCount = maxSoCount > 0 ? Math.Min(maxSoCount, deal.CompletedSafetyOrdersCount) : deal.CompletedSafetyOrdersCount;
          update.TakeProfit = soCount * scaleTp;
          var response = await client.UpdateDealAsync(deal.Id, update);
        }
      }
      return deals.Count;
    }

    /// <summary>
    /// Returns a string containing a summary of the given deals 
    /// </summary>
    /// <param name="deals">List of deals to return a summary of</param>
    /// <returns>Summary of deals as a string</returns>
    public string GetDealSummariesText(List<Deal> deals)
    {
      var dealSummaries = new List<string>();
      foreach (Deal deal in deals)
      {
        var trailingTp = deal.IsTrailingEnabled ? $"TTP({deal.TrailingDeviation})" : "TP";
        dealSummaries.Add($"{deal.BotName}.{deal.Pair} = SO {deal.CompletedSafetyOrdersCount}, {trailingTp} {deal.TakeProfit}%");
      }
      return string.Join("\r\n", dealSummaries);
    }
  }
}