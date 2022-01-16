using System.Text.Json;
using UrmaDealGenie;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

public class DealGenieWorkerService : IntervalWorkerService
{
  private DealRuleSet? dealRuleSet;
  private string dealRulesFilename = "";
  private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions { 
    WriteIndented = true,
    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) } 
  };
  
  public DealGenieWorkerService(ILogger<IntervalWorkerService> logger, IConfiguration configuration)
    : base(logger, configuration)
  {
  }

  public override void Configure()
  {
    this.dealRulesFilename = this.Configuration["DealGenieWorkerService:DealConfigurationFile"];
    this.Logger.LogInformation($"Deal rules file: {this.dealRulesFilename}");
  }

  public override async void DoWork(object? state)
  {
    Console.WriteLine("Run Deal Genie...");
    if (this.dealRulesFilename != null)
    {
      // Deal settings can be changed and used without restarting worker
      this.dealRuleSet = JsonSerializer.Deserialize<DealRuleSet>(File.ReadAllText(this.dealRulesFilename), this.jsonOptions);
      if (this.dealRuleSet != null)
      {
        var apiKey = Environment.GetEnvironmentVariable("APIKEY");
        var secret = Environment.GetEnvironmentVariable("SECRET");
        Urma3cClient client = new Urma3cClient(apiKey, secret);
        DealGenieResponse response = await client.ProcessRules(dealRuleSet);
        Console.WriteLine(JsonSerializer.Serialize<DealGenieResponse>(response, this.jsonOptions));
      }
    }
    Console.WriteLine("Deal Genie completed");
  }
}