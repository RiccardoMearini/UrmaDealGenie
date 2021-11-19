using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace UrmaDealGenieApp
{
  sealed class Program
  {
    static Task Main(string[] args) => 
      CreateHostBuilder(args).Build().RunAsync();

    public static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
          services.AddHostedService<DealGenieWorkerService>();
        });
  }
}
