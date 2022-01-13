using CMC = CoinMarketCap.Objects;
using LC = LunarCrush.Objects;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using XCommas.Net;
using XCommas.Net.Objects;
using LunarCrush.Helpers;
using LunarCrush.Objects;

namespace UrmaDealGenie
{
  public class LunarCrushAltRank
  {
    private const int DEFAULT_MAX_ACR_SCORE = 1500;
    private const int DEFAULT_MAX_CMC_RANK = 1500;
    private XCommasApi xCommasClient = null;
    private HttpClient httpClient = null;

    public LunarCrushAltRank(XCommasApi client)
    {
      this.xCommasClient = client;
      this.httpClient = new HttpClient();
      this.httpClient.BaseAddress = new Uri("https://api.lunarcrush.com");
    }

    public async Task ProcessRules(List<LunarCrushAltRankPairRule> dealRuleSet, bool update)
    {
      // Get blacklist pairs
      var blacklistPairs = GetBlacklist().Result;

      // Get lunarcrush data
      var lunarCrushData = await GetLunarCrushData(LunarCrushMetric.GalaxyScore);
      if (lunarCrushData != null)
      {
        // Get CoinMarketCap data (call just once, so find out the highest rank specified across the rules)
        var cmcClient = new CoinMarketCap(this.xCommasClient);
        var maxCmcRank = dealRuleSet.Max(rule => rule.MaxCmcRank == 0 ? DEFAULT_MAX_CMC_RANK : rule.MaxCmcRank);
        var cmcData = await cmcClient.GetCoinMarketCapData(maxCmcRank);
        if (cmcData != null)
        {
          // Loop through each ruleset updating bots
          dealRuleSet.ForEach(rule => UpdateBotWithBestPairs(rule, update, lunarCrushData, blacklistPairs, cmcData.Data
                                        .Select(cmcPair => cmcPair)
                                        .Take(rule.MaxCmcRank == 0 ? DEFAULT_MAX_CMC_RANK : rule.MaxCmcRank)));
        }
        else
        {
          dealRuleSet.ForEach(rule => UpdateBotWithBestPairs(rule, update, lunarCrushData, blacklistPairs, null));            
        }
      }
    }

    public void UpdateBotWithBestPairs(LunarCrushAltRankPairRule rule, bool update, LC.Root lunarCrushData, string[] blacklistPairs, IEnumerable<CMC.Datum> cmcData)
    {
      // Get the bot and current pairs
      var bot = this.xCommasClient.ShowBot(rule.BotId).Data;
      var pairs = bot.Pairs;
      var pairBase = bot.Pairs[0].Split('_')[0];
      var minVolBtc24h = bot.MinVolumeBtc24h;

      // Max Altcoin Rank Score for all pairs for this bot
      var maxAcrScore = rule.MaxAcrScore == 0 ? DEFAULT_MAX_ACR_SCORE : rule.MaxAcrScore;

      Console.WriteLine($"==================================================");
      Console.WriteLine($"Bot {bot.Id} - Processing LunarCrushAltRankPairRule '{rule.Rule}'");
      Console.WriteLine($"Bot {bot.Id} - '{bot.Name}' current pairs:");
      Console.WriteLine($"Bot {bot.Id} - {String.Join(", ", pairs)}");
      Console.WriteLine($"Bot {bot.Id} - Pair base currency: {pairBase}");
      Console.WriteLine($"Bot {bot.Id} - Minimum 24h Volume in BTC: {minVolBtc24h}");
      Console.WriteLine($"Bot {bot.Id} - Max Altrank Score for found pairs: {maxAcrScore}");
      Console.WriteLine($"Bot {bot.Id} - Max CoinMarketCap Rank for found pairs : {(cmcData == null ? 0 : cmcData.Count())}");

      // Get supported pairs on the bot's exchange
      var exchange = GetExchange(bot.AccountId);
      var exchangePairs = this.xCommasClient.GetMarketPairs(exchange.MarketCode); // #### do this once per exchange and cache result
      Console.WriteLine($"Bot {bot.Id} - Found {exchangePairs.Data.Length} pairs on '{this.xCommasClient.UserMode}' mode account '{exchange.Name}' exchange {exchange.MarketCode}");

      // BTC price in USDT
      var btcUsdtPrice3C = this.xCommasClient.GetCurrencyRate("USDT_BTC", exchange.MarketCode).Data.Last;
      Console.WriteLine($"Bot {bot.Id} - BTC price on {exchange.MarketCode} exchange ${btcUsdtPrice3C}");

      var newPairs = new List<string>();
      foreach (LC.Datum crushData in lunarCrushData.Data)
      {
        var volBTC = (decimal)crushData.V / btcUsdtPrice3C;
        var pair = FormatPair(crushData.S, pairBase, exchange.MarketCode);
        var stablecoin = crushData.Categories.Contains("stablecoin");

        // Only add pair if it meets all the approved criteria
        if (!stablecoin && crushData.Acr <= maxAcrScore
          && volBTC != 0 && volBTC >= minVolBtc24h
          && String.IsNullOrEmpty(Array.Find(blacklistPairs, blacklistPair => blacklistPair == pair))
          && !String.IsNullOrEmpty(Array.Find(exchangePairs.Data, exchangePair => exchangePair == pair))
          && (cmcData == null || (cmcData.Any(cmcPair => cmcPair.Symbol == crushData.S)))
        )
        {
          newPairs.Add(pair);
          if (newPairs.Count == rule.MaxPairCount) break;
        }
      }

      // #### update only if config allows update
      // otherwise just return what would get changed, just like dealrules
      var containSamePairs = new HashSet<string>(newPairs).SetEquals(bot.Pairs);
      if (containSamePairs)
      {
        Console.WriteLine($"Bot {bot.Id} - already has best pairs, no changes for bot '{bot.Name}'");
      }
      else
      {
        Console.WriteLine($"Bot {bot.Id} - NEW PAIRS for bot '{bot.Name}':");
        Console.WriteLine($"Bot {bot.Id} -> {String.Join(", ", newPairs)}");
        if (update)
        {
          UpdateBot(bot, newPairs.ToArray());
        }
        Console.WriteLine($"Bot {bot.Id} - {(update ? "Bot updated" : "Update mode disabled - no changes made")}");
      }
    }

    private XCommasResponse<Bot> UpdateBot(Bot bot, string[] newPairs)
    {
      var updateData = new BotUpdateData(bot)
      {
        MaxActiveDeals = newPairs.Length,
        Pairs = newPairs,
      };
      return this.xCommasClient.UpdateBot(bot.Id, updateData);
    }

    private Account GetExchange(int accountId)
    {
      // Console.WriteLine($"DEBUG: GetExchange() - accountId: {accountId}");
      this.xCommasClient.UserMode = UserMode.Real;
      var accounts = this.xCommasClient.GetAccounts;
      var exchange = Array.Find(accounts.Data, account => account.Id == accountId);
      if (exchange == null)
      {
        this.xCommasClient.UserMode = UserMode.Paper;
        accounts = this.xCommasClient.GetAccounts;
        exchange = Array.Find(accounts.Data, account => account.Id == accountId);
      }
      // Console.WriteLine($"DEBUG: GetExchange() - exchange: {exchange.ExchangeName}");

      return exchange;
    }

    private async Task<string[]> GetBlacklist()
    {
      // #### get blacklist from the dealrule, and combine with the 3C blacklist (or override as an option?)
      var response = (await this.xCommasClient.GetBotPairsBlackListAsync()).Data;
      Console.WriteLine($"Blacklist pairs: {String.Join(", ", response.Pairs)}");

      return response.Pairs;
    }

    private async Task<LC.Root> GetLunarCrushData(LunarCrushMetric metric)
    {
      LC.Root lunarCrushData = null;
      var request = BuildLunarCrushHttpRequest(await LunarCrushHelper.GetApiKey(), metric);
      // Console.WriteLine($"DEBUG: {this.GetType().Name} - RequestUri: {httpClient.BaseAddress}{request.RequestUri}");

      var result = httpClient.SendAsync(request);
      var response = await result.Result.Content.ReadAsStringAsync();
      if (result.Result.StatusCode == System.Net.HttpStatusCode.OK)
      {
        try
        {
          lunarCrushData = JsonSerializer.Deserialize<LC.Root>(response);
          Console.WriteLine($"Retrieved '{metric}' LunarCrush data, top {lunarCrushData.Data.Count} pairs");
        }
        catch
        {
          Console.WriteLine($"GetLunarCrushData() - FAILED TO DESERIALISE: Data:");
          Console.WriteLine(response);
        }
      }
      else
      {
        Console.WriteLine($"GetLunarCrushData() - FAILED TO RETRIEVE: {result.Result.StatusCode} - {result.Result.ReasonPhrase}");
      }
      return lunarCrushData;
    }

    private static string FormatPair(string coin, string pairBase, string marketcode)
    {
      string pair;
      if (marketcode == "binance_futures")
        pair = $"{pairBase}_{coin}{pairBase}";
      else if (marketcode == "ftx_futures")
        pair = $"{pairBase}_{coin}-PERP";
      else
        pair = $"{pairBase}_{coin}";
      //Console.WriteLine($"DEBUG: Formatted pair: {pair}");
      return pair;
    }

    private static HttpRequestMessage BuildLunarCrushHttpRequest(string apiKey, LunarCrushMetric metric)
    {
      string metricSort = "";
      switch (metric)
      {
        case LunarCrushMetric.Altrank: metricSort = "acr"; break;
        case LunarCrushMetric.GalaxyScore: metricSort = "gs"; break;
      };
      var queryString = new Dictionary<string, string>()
      {
        { "data", "market" },
        { "type", "fast" },
        { "sort", metricSort },
        { "limit", "100" },
        { "key", apiKey },
      };
      if (metric == LunarCrushMetric.GalaxyScore) 
      {
        queryString["desc"] = "true";
      }

      var request = new HttpRequestMessage(HttpMethod.Get, QueryHelpers.AddQueryString("v2", queryString));
      request.Headers.Add("Accept", "application/json");
      //Console.WriteLine($"DEBUG: BuildLunarCrushHttpRequest: {request.RequestUri}");
      return request;
    }


  }
}