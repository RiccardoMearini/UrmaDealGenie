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
      Environment.SetEnvironmentVariable("APIKEY", "bc983f0d41b140cfb52ab20dce8465b92d566aa4d211430683720ea6e813137f");
      Environment.SetEnvironmentVariable("SECRET", "229b5155e5fd07ef7f32196001fe360f142705166464538faf44cb6f1cb9101a403aa55c0bb183c77007ed21ad30d99b353f1b299c29169768ff793b36a6aebbdc5995ba7714c54e148ea4bf720f7d7ea1154ca885c1a1241f4036f13e53a58b50e7e652");
      Root? input = JsonSerializer.Deserialize<Root>(jsonString);
      if (input != null)
      {
        Console.WriteLine(await lambda.FunctionHandler(input, null));
      }
    }
  }
}