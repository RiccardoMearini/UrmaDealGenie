using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using UrmaDealGenie;

namespace UrmaDealGenieApp
{
  class Program
  {
    static async Task Main(string[] args)
    {
      Function lambda = new Function();
      string jsonString = File.ReadAllText($"../UrmaDealGenie/.lambda-test-tool/SavedRequests/test-config-update-false.json");
      Root? input = JsonSerializer.Deserialize<Root>(jsonString);
      if (input != null)
      {
        Console.WriteLine(await lambda.FunctionHandler(input, null));
      }
    }
  }
}