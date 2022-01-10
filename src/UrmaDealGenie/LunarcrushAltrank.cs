
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
      Console.WriteLine($"Pairs: {pairs}");
      
      // Get the bot's exchange from account
      var exchange = await GetExchange(bot.AccountId);

      // Get pairs on this exchange
      var exchangePairs = await this.xCommasClient.GetMarketPairsAsync(exchange.MarketCode);

      // tickerlist = get_threecommas_market(logger, api, marketcode)
      // logger.info("Bot exchange: %s (%s)" % (exchange, marketcode))

      // Get lunarcrush data
      var data = await GetLunarCrushData(); // #### enum param for altrank/gs etc?


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
      Console.WriteLine($"GetExchange - found {this.xCommasClient.UserMode} exchange '{exchange.Name}'");

      return exchange;
    }  

    private async Task<Root> GetLunarCrushData() // #### move to a separate helper class? Maybe there's gitlab wrapper project?
    {
      var request = BuildLunarCrushHttpRequest();
      Console.WriteLine($"{this.GetType().Name} - RequestUri: {request.RequestUri}");

      var result = httpClient.SendAsync(request);
      var data = await result.Result.Content.ReadAsStringAsync();
      Console.WriteLine($"Data: {data}");
      Root myDeserializedClass = JsonSerializer.Deserialize<Root>(data);
      return myDeserializedClass;
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
        { "desc", "False"}, // false for altrank, true for galaxyscore
      };      
      var request = new HttpRequestMessage(HttpMethod.Post, QueryHelpers.AddQueryString("v2", queryString));
      request.Headers.Add("Accept", "application/json");
      Console.WriteLine($"GetHttpRequest: {request.RequestUri}");
      return request;
    }
  }
}