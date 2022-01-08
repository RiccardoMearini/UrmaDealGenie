
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using XCommas.Net;
using XCommas.Net.Objects;

namespace UrmaDealGenie
{
  public class LunarcrushAltrankPairRule
  {
    public int BotId {get; set;}
    public int MaxPairCount {get; set;}
  }
 
  public class LunarcrushAltrank
  {
    private XCommasApi client = null;
    private HttpClient httpClient = null;

    public LunarcrushAltrank(XCommasApi client)
    {
      this.client = client;
      this.httpClient = new HttpClient();
      this.httpClient.BaseAddress = new Uri("https://api.lunarcrush.com");      
    }

    public async Task ProcessRule(LunarcrushAltrankPairRule pairRule)
    {
      var data = await GetLunarCrushData();
      Console.WriteLine($"Data: {data}");
    }

    public async Task<string> GetLunarCrushData() // #### move to a separate helper class? Maybe there's gitlab wrapper project?
    {
      var request = GetHttpRequest();
      Console.WriteLine($"{this.GetType().Name} - RequestUri: {request.RequestUri}");

      var result = httpClient.SendAsync(request);
      return await result.Result.Content.ReadAsStringAsync();
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