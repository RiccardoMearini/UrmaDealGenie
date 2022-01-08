using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Xunit;

namespace UrmaDealGenie.Tests
{
  public class UrmaDealGenieTest
  {
    [Fact]
    public async void Test_Function_ProcessDeals()
    {
      Console.WriteLine($"TEST: Test_Function_ProcessDeals ...");
      // Invoke the lambda function and confirm it loads and processes the deal rules.
      var text = File.ReadAllText("dealrules.json");
      Console.WriteLine($"{text}");

      var apiKey = Environment.GetEnvironmentVariable("APIKEY");
      var secret = Environment.GetEnvironmentVariable("SECRET");
      Urma3cClient client = new Urma3cClient(apiKey, secret);
      Console.WriteLine($"apiKey = {apiKey.Substring(0, 10)}... ");
      Console.WriteLine($"secret = {secret.Substring(0, 10)}... ");

      Console.WriteLine($"Calling function handler...");
      var dealRuleSet = JsonSerializer.Deserialize<DealRuleSet>(text);
      List<DealResponse> response = await client.ProcessRules(dealRuleSet);
      Assert.Equal(5, response.Count);
    }
  }
}
