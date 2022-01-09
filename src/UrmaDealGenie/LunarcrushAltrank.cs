
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using XCommas.Net;

namespace UrmaDealGenie
{
  public class LunarCrushAltRankPairRule
  {
    public int BotId {get; set;}
    public int MaxPairCount {get; set;}
  }
 
  public class LunarCrushAltRank
  {
    private XCommasApi xcommasClient = null;
    private HttpClient httpClient = null;

    public LunarCrushAltRank(XCommasApi client)
    {
      this.xcommasClient = client;
      this.httpClient = new HttpClient();
      this.httpClient.BaseAddress = new Uri("https://api.lunarcrush.com");      
    }

    public async Task ProcessRule(LunarCrushAltRankPairRule pairRule)
    {
      var bot = this.xcommasClient.ShowBot(pairRule.BotId);
      var pairs = bot.Data.Pairs;
      Console.WriteLine($"Pairs: {pairs}");
      var data = await GetLunarCrushData();
    }

    public async Task<Root> GetLunarCrushData() // #### move to a separate helper class? Maybe there's gitlab wrapper project?
    {
      var request = GetHttpRequest();
      Console.WriteLine($"{this.GetType().Name} - RequestUri: {request.RequestUri}");

      var result = httpClient.SendAsync(request);
      var data = await result.Result.Content.ReadAsStringAsync();
      Console.WriteLine($"Data: {data}");
      Root myDeserializedClass = JsonSerializer.Deserialize<Root>(data);
      return myDeserializedClass;
    }
    
    private static HttpRequestMessage GetHttpRequest()
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