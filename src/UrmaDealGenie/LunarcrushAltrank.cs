
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
    public int BotId {get; set;}
    public int MaxPairCount {get; set;}
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

    public async Task ProcessRule(LunarCrushAltRankPairRule pairRule)
    {
      // Get the bot and current pairs
      var bot = this.xCommasClient.ShowBot(pairRule.BotId).Data;
      var pairs = bot.Pairs;
      var pairBase = bot.Pairs[0].Split('_')[0];

      Console.WriteLine($"Bot {bot.Id} ({bot.Name}) current pairs: {String.Join(", ", pairs)}");
      
      // Get supported pairs on the bot's exchange
      var exchange = await GetExchange(bot.AccountId);
      var exchangePairs = (await this.xCommasClient.GetMarketPairsAsync(exchange.MarketCode)).Data;
      Console.WriteLine($"Found {exchangePairs.Length} pairs on {exchange.MarketCode} exchange");

      // BTC price in USDT
      var btcUsdtPrice3C = (await this.xCommasClient.GetCurrencyRateAsync("USDT_BTC", exchange.MarketCode)).Data.Last;
      Console.WriteLine($"BTC price on {exchange.MarketCode} exchange ${btcUsdtPrice3C}");

      // Get lunarcrush data
      var lunarCrushData = await GetLunarCrushData(); // #### enum param for altrank/gs etc?
      
      var newPairs = new List<string>();
      int rank = 1;
      foreach(Datum crushData in lunarCrushData.Data)
      {
        crushData.Rank = rank++;
        crushData.VolBTC = crushData.V / (double)btcUsdtPrice3C;

        var coin = crushData.S;
        var pair = FormatPair(coin, pairBase, exchange.MarketCode);
        var acrScore = crushData.Acr;
        var volBtc = crushData.VolBTC;
        if (volBtc != 0)
        {
          if (acrScore <= 1500) // #### make 1500 a maxAcrScore config variable
          {
            if (Array.Find(exchangePairs, exchangePair => exchangePair == pair).Length > 0)
            {
              newPairs.Add(pair);
            }
          }
        }
      }
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

    private async Task<Root> GetLunarCrushData()
    {
      var request = BuildLunarCrushHttpRequest();
      Console.WriteLine($"{this.GetType().Name} - RequestUri: {httpClient.BaseAddress}{request.RequestUri}");

      var result = httpClient.SendAsync(request);
      var response = await result.Result.Content.ReadAsStringAsync();
      Console.WriteLine($"DEBUG: Data: {response}");
      Root lunarCrushData = JsonSerializer.Deserialize<Root>(response);
      Console.WriteLine($"Retrieved '{lunarCrushData.Config.Sort}' LunarCrush data, top {lunarCrushData.Data.Count} pairs, BTC price ${lunarCrushData.Config.Btc.P:F2}");
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
      Console.WriteLine($"Formatted pair: {pair}");
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