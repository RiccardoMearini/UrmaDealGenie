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

namespace UrmaDealGenie
{
  public class LunarCrushAltRank
  {
    private const int DEFAULT_MAX_ACR_SCORE = 1500;
    private const int DEFAULT_MAX_CMC_RANK = 250;
    private XCommasApi xCommasClient = null;
    private HttpClient httpClient = null;

    public LunarCrushAltRank(XCommasApi client)
    {
      this.xCommasClient = client;
      this.httpClient = new HttpClient();
      this.httpClient.BaseAddress = new Uri("https://api.lunarcrush.com");
    }

    public async Task ProcessRules(List<LunarCrushAltRankPairRule> dealRuleSet)
    {
      // Get blacklist pairs
      var blacklistPairs = GetBlacklist().Result;

      // Get lunarcrush data
      var lunarCrushData = await GetLunarCrushData(); // #### enum param for altrank/gs etc?
      if (lunarCrushData != null)
      {
        // Get CoinMarketCap data (call just once, so find out the highest rank specified across the rules)
        var cmcClient = new CoinMarketCap(this.xCommasClient);
        var maxCmcRank = dealRuleSet.Max(rule => rule.MaxCmcRank == 0 ? DEFAULT_MAX_CMC_RANK : rule.MaxCmcRank);
        var cmcData = await cmcClient.GetCoinMarketCapData(maxCmcRank);
        if (cmcData != null)
        {
          // Loop through each ruleset updating bots
          dealRuleSet.ForEach(async rule => 
            await UpdateBotWithBestPairs(rule, lunarCrushData, blacklistPairs, cmcData.Data
                                         .Select(cmcPair => cmcPair)
                                         .Take(rule.MaxCmcRank == 0 ? DEFAULT_MAX_CMC_RANK : rule.MaxCmcRank)));
        }
      }
    }

    public async Task UpdateBotWithBestPairs(LunarCrushAltRankPairRule dealRule, LC.Root lunarCrushData, string[] blacklistPairs, IEnumerable<CMC.Datum> cmcData)
    {
      // Get the bot and current pairs
      var bot = this.xCommasClient.ShowBot(dealRule.BotId).Data;
      var pairs = bot.Pairs;
      var pairBase = bot.Pairs[0].Split('_')[0];
      var minVolBtc24h = bot.MinVolumeBtc24h;

      // Max Altcoin Rank Score for all pairs for this bot
      var maxAcrScore = dealRule.MaxAcrScore == 0 ? DEFAULT_MAX_ACR_SCORE : dealRule.MaxAcrScore;        

      Console.WriteLine($"Bot {bot.Id} ({bot.Name}) current pairs:");
      Console.WriteLine($"  {String.Join(", ", pairs)}");
      Console.WriteLine($"Pair base currency: {pairBase}");
      Console.WriteLine($"Minimum 24h Volume in BTC: {minVolBtc24h}");
      Console.WriteLine($"Max Altrank Score for found pairs: {maxAcrScore}");
      Console.WriteLine($"Max CoinMarketCap Rank for found pairs : {cmcData.Count()}");

      // Get supported pairs on the bot's exchange
      var exchange = await GetExchange(bot.AccountId);
      var exchangePairs = (await this.xCommasClient.GetMarketPairsAsync(exchange.MarketCode)).Data;
      Console.WriteLine($"Found {exchangePairs.Length} pairs on {exchange.MarketCode} exchange");

      // BTC price in USDT
      var btcUsdtPrice3C = (await this.xCommasClient.GetCurrencyRateAsync("USDT_BTC", exchange.MarketCode)).Data.Last;
      Console.WriteLine($"BTC price on {exchange.MarketCode} exchange ${btcUsdtPrice3C}");

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
          && !String.IsNullOrEmpty(Array.Find(exchangePairs, exchangePair => exchangePair == pair))
          && cmcData.Any(cmcPair => cmcPair.Symbol == crushData.S)
        )
        {
          newPairs.Add(pair);
          if (newPairs.Count == dealRule.MaxPairCount) break;
        }
      }
      // #### update only if config allows update
      // otherwise just return what would get changed, just like dealrules
      await UpdateBot(bot, newPairs.ToArray());
    }

    private async Task<XCommasResponse<Bot>> UpdateBot(Bot bot, string[] newPairs)
    {
      XCommasResponse<Bot> response = null;
      var containSamePairs = new HashSet<string>(newPairs).SetEquals(bot.Pairs);
      if (containSamePairs)
      {
        Console.WriteLine($"Bot {bot.Id} ({bot.Name}) already has best pair selection, no action");
      }
      else
      {
        Console.WriteLine($"Bot {bot.Id} ({bot.Name}) update new pairs:");
        Console.WriteLine($"  {String.Join(", ", newPairs)}");
        var updateData = new BotUpdateData(bot)
        {
          MaxActiveDeals = newPairs.Length,
          Pairs = newPairs,
        };
        response = await this.xCommasClient.UpdateBotAsync(bot.Id, updateData);
      }
      return response;
    }

    private async Task<Account> GetExchange(int accountId)
    {
      this.xCommasClient.UserMode = UserMode.Real;
      var accounts = await this.xCommasClient.GetAccountsAsync();
      var exchange = Array.Find(accounts.Data, account => account.Id == accountId);
      if (exchange == null)
      {
        this.xCommasClient.UserMode = UserMode.Paper;
        accounts = await this.xCommasClient.GetAccountsAsync();
        exchange = Array.Find(accounts.Data, account => account.Id == accountId);
      }
      Console.WriteLine($"Found {this.xCommasClient.UserMode} account '{exchange.Name}' ({exchange.MarketCode})");

      return exchange;
    }

    private async Task<string[]> GetBlacklist()
    {
      // #### get blacklist from the dealrule, and combine with the 3C blacklist (or override as an option?)
      var response = (await this.xCommasClient.GetBotPairsBlackListAsync()).Data;
      Console.WriteLine($"Blacklist pairs: {String.Join(", ", response.Pairs)}");

      return response.Pairs;
    }

    private async Task<LC.Root> GetLunarCrushData()
    {
      LC.Root lunarCrushData = null;
      var request = BuildLunarCrushHttpRequest();
      // Console.WriteLine($"DEBUG: {this.GetType().Name} - RequestUri: {httpClient.BaseAddress}{request.RequestUri}");

      var result = httpClient.SendAsync(request);
      var response = await result.Result.Content.ReadAsStringAsync();
      try
      {
        lunarCrushData = JsonSerializer.Deserialize<LC.Root>(response);
        Console.WriteLine($"Retrieved '{lunarCrushData.Config.Sort}' LunarCrush data, top {lunarCrushData.Data.Count} pairs");
      }
      catch
      {
        Console.WriteLine($"FAILED TO DESERIALISE: Data:");
        Console.WriteLine(response);
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

    private static HttpRequestMessage BuildLunarCrushHttpRequest()
    {
      var queryString = new Dictionary<string, string>()
      {
        { "data", "market" },
        { "type", "fast" },
        { "sort", "acr" }, // acr = altrank, gs = galaxyscore
        { "limit", "100" },
        { "key", "" },
        // { "desc", True}, #### Param only applicable for galaxyscore
      };
      var request = new HttpRequestMessage(HttpMethod.Get, QueryHelpers.AddQueryString("v2", queryString));
      request.Headers.Add("Accept", "application/json");
      //Console.WriteLine($"DEBUG: BuildLunarCrushHttpRequest: {request.RequestUri}");
      return request;
    }

  }
}