
using CoinMarketCap.Objects;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using XCommas.Net;

namespace UrmaDealGenie
{
  public class CoinMarketCap
  {
    private XCommasApi xCommasClient = null;
    private HttpClient httpClient = null;

    public CoinMarketCap(XCommasApi client)
    {
      this.xCommasClient = client;
      this.httpClient = new HttpClient();
      this.httpClient.BaseAddress = new Uri("https://pro-api.coinmarketcap.com");
    }

    public async Task<Root> GetCoinMarketCapData(string cmcApiKey, int maxRank)
    {
      Root cmcData = null;

      var request = BuildCoinMarketCapHttpRequest(1, maxRank);
      request.Headers.Add("X-CMC_PRO_API_KEY", cmcApiKey);
      // Console.WriteLine($"DEBUG: {this.GetType().Name} - RequestUri: {httpClient.BaseAddress}{request.RequestUri}");

      var result = httpClient.SendAsync(request);
      var response = await result.Result.Content.ReadAsStringAsync();
      if (result.Result.StatusCode == System.Net.HttpStatusCode.OK)
      {
        try
        {
          cmcData = JsonSerializer.Deserialize<Root>(response);
          Console.WriteLine($"Retrieved CoinMarketCap data, top {cmcData.Data.Count} pairs");
        }
        catch
        {
          Console.WriteLine($"GetCoinMarketCapData() - FAILED TO DESERIALISE: Data:");
          Console.WriteLine(response);
        }
      }
      else
      {
        Console.WriteLine($"GetCoinMarketCapData() - FAILED TO RETRIEVE: {result.Result.StatusCode} - {result.Result.ReasonPhrase}");          
        Console.WriteLine($"GetCoinMarketCapData() - cmcApiKey: {cmcApiKey}");
      }
      return cmcData;
    }

    private static HttpRequestMessage BuildCoinMarketCapHttpRequest(int start, int limit)
    {
      var queryString = new Dictionary<string, string>()
      {
        { "start", $"{start}" },
        { "limit", $"{limit}" },
        { "convert", "BTC" },
        { "aux", "cmc_rank" },
      };
      var request = new HttpRequestMessage(HttpMethod.Get, QueryHelpers.AddQueryString("v1/cryptocurrency/listings/latest", queryString));
      request.Headers.Add("Accept", "application/json");

      // Console.WriteLine($"DEBUG: BuildCoinMarketCapHttpRequest: {request.RequestUri}");
      return request;
    }

  }
}