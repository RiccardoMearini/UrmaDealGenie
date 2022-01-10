
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using XCommas.Net;
using XCommas.Net.Objects;

namespace UrmaDealGenie
{
  public class LunarCrushAltRankPairRule
  {
    public int BotId { get; set; }
    public int MaxPairCount { get; set; }
  }

  public class LunarCrushAltRank
  {
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

      dealRuleSet.ForEach(async rule => await UpdateBotWithBestPairs(rule, lunarCrushData, blacklistPairs));
    }

    public async Task UpdateBotWithBestPairs(LunarCrushAltRankPairRule dealRule, Root lunarCrushData, string[] blacklistPairs)
    {
      // Get the bot and current pairs
      var bot = this.xCommasClient.ShowBot(dealRule.BotId).Data;
      var pairs = bot.Pairs;
      var pairBase = bot.Pairs[0].Split('_')[0];
      var minVolBtc24h = bot.MinVolumeBtc24h;

      Console.WriteLine($"Bot {bot.Id} ({bot.Name}) current pairs:");
      Console.WriteLine($"  {String.Join(", ", pairs)}");
      Console.WriteLine($"Pair base currency: {pairBase}");
      Console.WriteLine($"Minimum 24h Volume in BTC: {minVolBtc24h}");

      // Get supported pairs on the bot's exchange
      var exchange = await GetExchange(bot.AccountId);
      var exchangePairs = (await this.xCommasClient.GetMarketPairsAsync(exchange.MarketCode)).Data;
      Console.WriteLine($"Found {exchangePairs.Length} pairs on {exchange.MarketCode} exchange");

      // BTC price in USDT
      var btcUsdtPrice3C = (await this.xCommasClient.GetCurrencyRateAsync("USDT_BTC", exchange.MarketCode)).Data.Last;
      Console.WriteLine($"BTC price on {exchange.MarketCode} exchange ${btcUsdtPrice3C}");

      var newPairs = new List<string>();
      int rank = 1;
      foreach (Datum crushData in lunarCrushData.Data)
      {
        crushData.Rank = rank++;
        crushData.VolBTC = crushData.V / (double)btcUsdtPrice3C;

        var coin = crushData.S;
        var pair = FormatPair(coin, pairBase, exchange.MarketCode);
        var acrScore = crushData.Acr;
        var volBtc = crushData.VolBTC;

        // #### make 1500 a maxAcrScore config variable
        if (volBtc != 0 && acrScore <= 1500 &&
          !String.IsNullOrEmpty(Array.Find(exchangePairs, exchangePair => exchangePair == pair)))
        {
          if (String.IsNullOrEmpty(Array.Find(blacklistPairs, blacklistPair => blacklistPair == pair)))
          {
            newPairs.Add(pair);
            if (newPairs.Count == dealRule.MaxPairCount) break;
          }
        }
      }
      await UpdateBot(bot, newPairs.ToArray());
    }

    private async Task<XCommasResponse<Bot>> UpdateBot(Bot bot, string[] newPairs)
    {
      XCommasResponse<Bot> response = null;
      var containSamePairs = new HashSet<string>(newPairs).SetEquals(bot.Pairs);
      if (containSamePairs)
      {
        Console.WriteLine($"Bot already has best pair selection, no action");
      }
      else
      {
        Console.WriteLine($"New pairs:");
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

    private async Task<Root> GetLunarCrushData()
    {
      var request = BuildLunarCrushHttpRequest();
      // Console.WriteLine($"DEBUG: {this.GetType().Name} - RequestUri: {httpClient.BaseAddress}{request.RequestUri}");

      var result = httpClient.SendAsync(request);
      var response = await result.Result.Content.ReadAsStringAsync();
      //Console.WriteLine($"DEBUG: Data: {response}");
      Root lunarCrushData = JsonSerializer.Deserialize<Root>(response);
      Console.WriteLine($"Retrieved '{lunarCrushData.Config.Sort}' LunarCrush data, top {lunarCrushData.Data.Count} pairs");
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
      { // Altrank
        { "data", "market" },
        { "type", "fast" },
        { "sort", "acr" }, // acr = altrank, gs = galaxyscore
        { "limit", "100" },
        { "key", "" },
        // { "desc", True}, #### Param only applicable for galaxyscore
      };
      var request = new HttpRequestMessage(HttpMethod.Post, QueryHelpers.AddQueryString("v2", queryString));
      request.Headers.Add("Accept", "application/json");
      //Console.WriteLine($"DEBUG: GetHttpRequest: {request.RequestUri}");
      return request;
    }
  }
}