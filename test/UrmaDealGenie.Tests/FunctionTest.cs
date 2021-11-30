using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using UrmaDealGenie;
using System.IO;

namespace UrmaDealGenie.Tests
{
  public class FunctionTest
  {
    [Fact]
    public async void Test_Function_ProcessDeals()
    {
      Console.WriteLine($"TEST: Test_Function_ProcessDeals ...");
      // Invoke the lambda function and confirm it loads and processes the deal rules.
      var function = new Function();
      var context = new TestLambdaContext();
      var text = File.ReadAllText("dealrules.json");
      Console.WriteLine($"{text}");

      var apiKey = Environment.GetEnvironmentVariable("APIKEY");
      Console.WriteLine($"apiKey = {apiKey.Substring(0, 10)}... ");

      Console.WriteLine($"Calling function handler...");
      var input = JsonSerializer.Deserialize<DealRuleSet>(text);
      var dealResponses = await function.FunctionHandler(input, context);
      Assert.Equal(5, dealResponses.Count);
    }

    [Fact]
    public async void Test_Function_CallNoEnvVarsSet()
    {
      Console.WriteLine($"TEST: Test_Function_CallNoEnvVarsSet ...");

      // Invoke the lambda function without environment variables set for the apikey and secret.
      var function = new Function();
      var context = new TestLambdaContext();
      var text = File.ReadAllText("dealrules.json");

      var apiKey = Environment.GetEnvironmentVariable("APIKEY");
      var secret = Environment.GetEnvironmentVariable("SECRET");

      Environment.SetEnvironmentVariable("APIKEY", null);
      Environment.SetEnvironmentVariable("SECRET", null);

      Console.WriteLine($"Calling function handler...");
      var input = JsonSerializer.Deserialize<DealRuleSet>(text);
      var dealResponses = await function.FunctionHandler(input, context);
      Assert.Equal(0, dealResponses.Count);

      // Reset environment variables back to what they were before this test
      Environment.SetEnvironmentVariable("APIKEY", apiKey);
      Environment.SetEnvironmentVariable("SECRET", secret);

    }
  }
}
