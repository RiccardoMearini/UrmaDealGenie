using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public abstract class IntervalWorkerService : IHostedService
{
  protected readonly ILogger Logger;
  protected readonly IConfiguration Configuration;
  private Timer? timer;
  private int intervalMinutes;


  public IntervalWorkerService(
    ILogger<IntervalWorkerService> logger,
    IConfiguration configuration)
  {
    this.Logger = logger;
    this.Configuration = configuration;
    this.intervalMinutes = int.Parse(this.Configuration["IntervalWorkerService:IntervalMinutes"]);
    this.Logger.LogInformation($"Worker interval: {this.intervalMinutes} minutes");
    Configure();
  }

  public Task StartAsync(CancellationToken cancellationToken)
  {
    Logger.LogInformation("Starting worker...");
    timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(this.intervalMinutes));
    return Task.CompletedTask;
  }

  public abstract void Configure();

  public abstract void DoWork(object? state);

  public Task StopAsync(CancellationToken cancellationToken)
  {
    Logger.LogInformation("Stopping worker...");

    return Task.CompletedTask;
  }

  public void Dispose()
  {
    timer?.Dispose();
  }
}
