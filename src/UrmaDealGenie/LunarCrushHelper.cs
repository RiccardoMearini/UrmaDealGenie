using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net;

namespace LunarCrush.Helpers
{
  public class LunarCrushHelper
  {
    private const string LunarCrushVersion = "lunar-20211013";
    private const string VersionId = "GtlZn1NfoVuhQ4p9mdveb26zFPBrwyTMXRCJUIAY7giqc3SLOWD80xHKE5sjka";
    private readonly static List<char> letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456790".ToCharArray().ToList();

    private static string GenerateToken(string versionId, string deviceID)
    {
      return deviceID
          .Split("-")
          .Skip(1)
          .Aggregate("", (acc, part) => acc + part)
          .ToCharArray()
          .Aggregate("", (acc, character) => acc + letters[versionId.IndexOf(character)]);
    }

    /// <summary>
    /// Get a free LunarCrush API key based on your device.
    /// If this call doesn't work with default params, then LunarCrush has changed versions.
    /// </summary>
    /// <param name="version">Optional LunarCrush version e.g. "lunar-20211013"</param>
    /// <param name="versionId">Optional versionId form LunarCrush website e.g. "GtlZn1NfoVuhQ4p9mdveb26zFPBrwyTMXRCJUIAY7giqc3SLOWD80xHKE5sjka"</param>
    /// <param name="deviceId">Optional device ID in format "LDID-{guid}"</param>
    /// <returns>API key to use in the "key" query string param of the LunarCrush API URL</returns>    
    public async static Task<string> GetApiKey(
      string version = LunarCrushVersion, 
      string versionId = VersionId, 
      string deviceId = null)
    {
      deviceId = deviceId ?? ("LDID-" + Guid.NewGuid().ToString());
      var token = GenerateToken(versionId, deviceId);

      HttpClient httpClient = new HttpClient();
      httpClient.BaseAddress = new Uri("https://api.lunarcrush.com");
      var queryString = new Dictionary<string, string>()
      {
        { "requestAccess", "lunar" },
        { "platform", "web" },
        { "device", "Firefox" },
        { "deviceId", deviceId },
        { "validator", token },
        { "clientVersion", version}, 
      };
      var request = new HttpRequestMessage(HttpMethod.Get, QueryHelpers.AddQueryString("v2", queryString));
      request.Headers.Add("Accept", "application/json");
      var response = httpClient.SendAsync(request);
      string result = null;
      if (response.Result.StatusCode == HttpStatusCode.OK)
      { 
        result = (string)JObject.Parse(await response.Result.Content.ReadAsStringAsync())["token"];
        Console.WriteLine($"LunarCrushHelpers.GetApiKey() - Key: {result}");
      }
      else
      {
        Console.WriteLine($"LunarCrushHelpers.GetApiKey() - deviceId: {deviceId}, version: {version}, versionId: {versionId}, token: {token}");        
        Console.WriteLine($"FAILED: response: {response.Result.StatusCode}, requestUrl:'{request.RequestUri}'");
      }
      return result;
    }
  }
}