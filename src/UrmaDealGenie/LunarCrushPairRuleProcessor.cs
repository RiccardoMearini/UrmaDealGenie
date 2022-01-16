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
  public class LunarCrushPairRuleProcessor
  {
    private const int DEFAULT_MAX_METRIC_SCORE = 1500;
    private const int DEFAULT_MAX_CMC_RANK = 200; // higher than this has a credit cost on CMC account
    private XCommasApi xCommasClient = null;
    private HttpClient httpClient = null;
    private readonly string cmcApiKey = null;

    public LunarCrushPairRuleProcessor(XCommasApi client)
    {
      this.xCommasClient = client;
      this.httpClient = new HttpClient();
      this.httpClient.BaseAddress = new Uri("https://api.lunarcrush.com");
      this.cmcApiKey = Environment.GetEnvironmentVariable("CMCAPIKEY");
    }

    public async Task<List<BotPairResponse>> ProcessRules(List<LunarCrushPairRule> dealRuleSet, bool update)
    {
      // Get blacklist pairs
      var blacklistPairs = GetBlacklist().Result;
      CMC.Root cmcData = null;
      // Optionally get CoinMarketCap data with max rank based on thie highest rank across all rules
      if (!string.IsNullOrEmpty(this.cmcApiKey))
      {
        var maxCmcRank = dealRuleSet.Max(rule => rule.MaxCmcRank == 0 ? DEFAULT_MAX_CMC_RANK : rule.MaxCmcRank);
        var cmcClient = new CoinMarketCap(this.xCommasClient);
        cmcData = await cmcClient.GetCoinMarketCapData(this.cmcApiKey, maxCmcRank);
      }
      else
      {
        Console.WriteLine($"No CoinMarketCap API key specified'");
      }

      List<BotPairResponse> result = new List<BotPairResponse>();
      if (dealRuleSet.Any(rule => rule.Metric == LunarCrushMetric.Altrank))
      {
        // Process all LunarCrush Altrank data and rules
        var lunarCrushAltrankData = await GetLunarCrushData(LunarCrushMetric.Altrank);
        result.AddRange(ProcessLunarCrushData(lunarCrushAltrankData, dealRuleSet.Where(rule => rule.Metric == LunarCrushMetric.Altrank).ToList(), cmcData, blacklistPairs, update));
      }

      if (dealRuleSet.Any(rule => rule.Metric == LunarCrushMetric.GalaxyScore))
      {
        // Process all LunarCrush GalaxyScore data and rules
        var lunarCrushGalaxyData = await GetLunarCrushData(LunarCrushMetric.GalaxyScore);
        result.AddRange(ProcessLunarCrushData(lunarCrushGalaxyData, dealRuleSet.Where(rule => rule.Metric == LunarCrushMetric.GalaxyScore).ToList(), cmcData, blacklistPairs, update));
      }
      return result;
    }

    private List<BotPairResponse> ProcessLunarCrushData(Root lunarCrushData, List<LunarCrushPairRule> dealRuleSet, CMC.Root cmcData, string[] blacklistPairs, bool update)
    {
      List<BotPairResponse> result = new List<BotPairResponse>();
      if (lunarCrushData != null)
      {
        if (cmcData != null)
        {
          // Loop through each ruleset updating bots, taking CoinMarketCap data into account
          result.AddRange(dealRuleSet.Select(
            rule => UpdateBotWithBestPairs(rule, update, lunarCrushData, blacklistPairs, cmcData.Data
            .Select(cmcPair => cmcPair)
            .Take(rule.MaxCmcRank == 0 ? DEFAULT_MAX_CMC_RANK : rule.MaxCmcRank))));
        }
        else
        {
          // Loop through each ruleset updating bots
          result.AddRange(dealRuleSet.Select(
            rule => UpdateBotWithBestPairs(rule, update, lunarCrushData, blacklistPairs, null)));
        }
      }
      return result;
    }

    private BotPairResponse UpdateBotWithBestPairs(LunarCrushPairRule rule, bool update, LC.Root lunarCrushData, string[] blacklistPairs, IEnumerable<CMC.Datum> cmcData)
    {
      // Get the bot and current pairs
      var bot = this.xCommasClient.ShowBot(rule.BotId).Data;
      var pairs = bot.Pairs;
      var pairBase = bot.Pairs[0].Split('_')[0];
      var minVolBtc24h = bot.MinVolumeBtc24h;

      // Max Altcoin Rank Score for all pairs for this bot
      var maxMetricScore = rule.MaxMetricScore == 0 ? DEFAULT_MAX_METRIC_SCORE : rule.MaxMetricScore;

      Console.WriteLine($"==================================================");
      Console.WriteLine($"Bot {bot.Id} - Processing LunarCrushPairRule '{rule.Metric}' metric, for rule '{rule.Rule}'");
      Console.WriteLine($"Bot {bot.Id} - '{bot.Name}' currently has {bot.MaxActiveDeals} Max Active Deals, and these pairs:");
      Console.WriteLine($"Bot {bot.Id} -> {String.Join(", ", pairs)}");
      Console.WriteLine($"Bot {bot.Id} - Pair base currency: {pairBase}");
      Console.WriteLine($"Bot {bot.Id} - Minimum 24h Volume in BTC: {minVolBtc24h}");
      Console.WriteLine($@"Bot {bot.Id} - Max {(rule.Metric == LunarCrushMetric.Altrank ? "Altrank" : "Galaxy")} Score for found pairs: {maxMetricScore}");
      Console.WriteLine($"Bot {bot.Id} - Max CoinMarketCap Rank for found pairs : {(cmcData == null ? 0 : cmcData.Count())}");
      // Get supported pairs on the bot's exchange
      var exchange = GetExchange(bot.AccountId);
      var exchangePairs = this.xCommasClient.GetMarketPairs(exchange.MarketCode); // #### do this once per exchange and cache result
      Console.WriteLine($"Bot {bot.Id} - Found {exchangePairs.Data.Length} pairs on '{this.xCommasClient.UserMode}' mode account '{exchange.Name}' exchange {exchange.MarketCode}");
      // Add any additional blacklist pairs from this rule
      if (!string.IsNullOrEmpty(rule.BlacklistCoins))
      {
        var ruleBlacklist = rule.BlacklistCoins.Split(',').Select(coin => FormatPair(coin, pairBase, exchange.MarketCode));
        Console.WriteLine($"Bot {bot.Id} - Additional blacklist pairs from this rule:");
        Console.WriteLine($"Bot {bot.Id} -> {String.Join(", ", ruleBlacklist)}");
        blacklistPairs = ruleBlacklist.Concat(blacklistPairs).Distinct().ToArray();
      }

      // BTC price in USDT
      var btcUsdtPrice3C = this.xCommasClient.GetCurrencyRate("USDT_BTC", exchange.MarketCode).Data.Last;
      Console.WriteLine($"Bot {bot.Id} - BTC price on {exchange.MarketCode} exchange ${btcUsdtPrice3C}");

      // Find pairs up the max of either the rule's MaxPairCount, or the bot's MaxActiveDeals
      var maxPairCount = Math.Max(rule.MaxPairCount, bot.MaxActiveDeals);
      Console.WriteLine($"Bot {bot.Id} - looking for {maxPairCount} pairs in LunarCrush data");

      var newPairs = new List<string>();
      foreach (LC.Datum crushData in lunarCrushData.Data)
      {
        var volBTC = (decimal)crushData.V / btcUsdtPrice3C;
        var pair = FormatPair(crushData.S, pairBase, exchange.MarketCode);
        var stablecoin = crushData.Categories.Contains("stablecoin");

        // Only add pair if it meets all the approved criteria
        //  Coin's 24 vol more than bot's min vol
        //  Coin is not in the blacklists
        //  Coin is on the bot's exchange
        //  Coin is within the max CoinMarketCap rank (if CMC API available, default is 200)
        //  Coin rank (GalaxyScore or Altrank) is less then the max metric score
        if (!stablecoin
          && volBTC != 0 && volBTC >= minVolBtc24h
          && String.IsNullOrEmpty(Array.Find(blacklistPairs, blacklistPair => blacklistPair == pair))
          && !String.IsNullOrEmpty(Array.Find(exchangePairs.Data, exchangePair => exchangePair == pair))
          && (cmcData == null || (cmcData.Any(cmcPair => cmcPair.Symbol == crushData.S))
          && (rule.Metric == LunarCrushMetric.Altrank ? crushData.Acr : crushData.Gs) <= maxMetricScore)
        )
        {
          newPairs.Add(pair);
          if (newPairs.Count == maxPairCount) break;
        }
      }

      bool botUpdated = false;
      if (update)
      {
        var containSamePairs = new HashSet<string>(newPairs).SetEquals(bot.Pairs);
        if (containSamePairs)
        {
          Console.WriteLine($"Bot {bot.Id} - already has {newPairs.Count} best pairs, no change for bot '{bot.Name}'");
        }
        else
        {
          Console.WriteLine($"Bot {bot.Id} - replacing {newPairs.Count} best pairs for bot '{bot.Name}':");
          Console.WriteLine($"Bot {bot.Id} -> {String.Join(", ", newPairs)}");
          var updateData = new BotUpdateData(bot) { Pairs = newPairs.ToArray() };
          this.xCommasClient.UpdateBot(bot.Id, updateData);
          botUpdated = true;
        }
      }
      else
      {
        Console.WriteLine($"Bot {bot.Id} - Update mode DISABLED, but found {newPairs.Count} best pairs for bot '{bot.Name}");
        Console.WriteLine($"Bot {bot.Id} -> {String.Join(", ", newPairs)}");
      }
      return new BotPairResponse() { Rule = rule.Rule, PairCount = newPairs.Count, Updated = botUpdated };
    }

    private Account GetExchange(int accountId)
    {
      // Console.WriteLine($"DEBUG: GetExchange() - accountId: {accountId}");
      this.xCommasClient.UserMode = UserMode.Real;
      var accounts = this.xCommasClient.GetAccounts;
      Account exchange = null;
      if (accounts == null)
      {
        Console.WriteLine($"ERROR: 3Commas API key does not have AccountRead permission");
      }
      else
      {
        exchange = Array.Find(accounts.Data, account => account.Id == accountId);
        if (exchange == null)
        {
          this.xCommasClient.UserMode = UserMode.Paper;
          accounts = this.xCommasClient.GetAccounts;
          exchange = Array.Find(accounts.Data, account => account.Id == accountId);
        }
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