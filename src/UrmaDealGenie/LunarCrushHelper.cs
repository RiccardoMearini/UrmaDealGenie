using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace UrmaDealGenie
{
  public class LunarCrushHelper
  {
    private readonly static string version = "lunar-20211013";
    private readonly static List<char> versionID = "GtlZn1NfoVuhQ4p9mdveb26zFPBrwyTMXRCJUIAY7giqc3SLOWD80xHKE5sjka".ToCharArray().ToList();
    private readonly static List<char> letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456790".ToCharArray().ToList();

    private static string GenerateToken(string deviceID)
    {
      return deviceID
          .Split("-")
          .Skip(1)
          .Aggregate("", (acc, part) => acc + part)
          .ToCharArray()
          .Aggregate("", (acc, character) => acc + letters[versionID.IndexOf(character)]);
    }

    public async static Task<string> GetApiKey()
    {
      var deviceID = $"LDID-{Guid.NewGuid()}";
      var token = GenerateToken(deviceID);
      HttpClient httpClient = new HttpClient();
      httpClient.BaseAddress = new Uri("https://api.lunarcrush.com");
      var queryString = new Dictionary<string, string>()
      {
        { "requestAccess", "lunar" },
        { "platform", "web" },
        { "device", "Firefox" },
        { "deviceId", deviceID },
        { "validator", token },
        { "clientVersion", version}, 
      };
      var request = new HttpRequestMessage(HttpMethod.Get, QueryHelpers.AddQueryString("v2", queryString));
      request.Headers.Add("Accept", "application/json");
      var result = httpClient.SendAsync(request);
      return (string)JObject.Parse(await result.Result.Content.ReadAsStringAsync())["token"];
    }
  }
}