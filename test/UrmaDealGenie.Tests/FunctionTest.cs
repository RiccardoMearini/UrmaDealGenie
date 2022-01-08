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
      Urma3cClient client = new Urma3cClient();
      var text = File.ReadAllText("dealrules.json");
      Console.WriteLine($"{text}");

      var apiKey = Environment.GetEnvironmentVariable("APIKEY");
      Console.WriteLine($"apiKey = {apiKey.Substring(0, 10)}... ");

      Console.WriteLine($"Calling function handler...");
      var dealRuleSet = JsonSerializer.Deserialize<DealRuleSet>(text);
      List<DealResponse> response = await client.ProcessRules(dealRuleSet);
      Assert.Equal(5, response.Count);
    }

    [Fact]
    public async void Test_Function_CallNoEnvVarsSet()
    {
      Console.WriteLine($"TEST: Test_Function_CallNoEnvVarsSet ...");

      // Invoke the lambda function without environment variables set for the apikey and secret.
      Urma3cClient client = new Urma3cClient();
      var text = File.ReadAllText("dealrules.json");

      var apiKey = Environment.GetEnvironmentVariable("APIKEY");
      var secret = Environment.GetEnvironmentVariable("SECRET");

      Environment.SetEnvironmentVariable("APIKEY", null);
      Environment.SetEnvironmentVariable("SECRET", null);

      Console.WriteLine($"Calling function handler...");
      var dealRuleSet = JsonSerializer.Deserialize<DealRuleSet>(text);
      List<DealResponse> response = await client.ProcessRules(dealRuleSet);
      Assert.Equal(0, response.Count);

      // Reset environment variables back to what they were before this test
      Environment.SetEnvironmentVariable("APIKEY", apiKey);
      Environment.SetEnvironmentVariable("SECRET", secret);

    }
  }
}
