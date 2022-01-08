using System.Text.Json;
using UrmaDealGenie;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class DealGenieWorkerService : IntervalWorkerService
{
  private DealRuleSet? dealRuleSet;
  private string dealRulesFilename = "";
  private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions { WriteIndented = true };

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
    Logger.LogInformation("Run Deal Genie...");
    if (this.dealRulesFilename != null)
    {
      // Deal settings can be changed and used without restarting worker
      this.dealRuleSet = JsonSerializer.Deserialize<DealRuleSet>(File.ReadAllText(this.dealRulesFilename));
      if (this.dealRuleSet != null)
      {
        var apiKey = Environment.GetEnvironmentVariable("APIKEY");
        var secret = Environment.GetEnvironmentVariable("SECRET");
        Urma3cClient client = new Urma3cClient(apiKey, secret);
        List<DealResponse> response = await client.ProcessRules(dealRuleSet);
        Logger.LogInformation(JsonSerializer.Serialize<List<DealResponse>>(response, this.jsonOptions));
      }
    }
    Logger.LogInformation("Deal Genie completed");
  }
}