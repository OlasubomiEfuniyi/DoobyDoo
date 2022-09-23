using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Host.Program
{
    using Host.HostedService;
    using Host.ScopedService;
    public class Program
    {
        public static async Task Main()
        {
            HostBuilder hostBuilder = new HostBuilder();


            hostBuilder.ConfigureWebJobs(webJobsBuilder =>
            {
                webJobsBuilder.AddAzureStorageCoreServices();
                webJobsBuilder.AddTimers();
            });

            //Add this otherwise the logs will not be printed to the console
            hostBuilder.ConfigureLogging(logging =>
            {
                logging.AddConsole();
            });

            //Let the host know that Service is to be executed as a HostedService i.e it should be managed (started and stopped) by the host
            hostBuilder.ConfigureServices(serviceCollection =>
            {
                //serviceCollection.AddHostedService<HostedService>();
                serviceCollection.AddSingleton<ScopedService>();
            });

            IHost host = hostBuilder.Build();

            await host.RunAsync();
        }
    }
}

namespace Host.ScopedService
{
    using Microsoft.Azure.WebJobs;

    public class ScopedService
    {
        private readonly ILogger<ScopedService> _logger;
        private static int countInstance = 0;
        public ScopedService(ILogger<ScopedService> logger)
        {
            _logger = logger;
            countInstance++;

            _logger.LogInformation($"[ScopedService]: {countInstance} instances created");
        }

        [FunctionName("TellTime")]
        public void TellTime([TimerTrigger("0 * * * * *", RunOnStartup = true)] TimerInfo myTimer)
        {
            _logger.LogInformation($"[TellTime]: Triggered with TimerInfo {myTimer.ToString()}");
        }
    }
}

namespace Host.HostedService
{
    public class HostedService: IHostedService
    {
        private readonly ILogger<HostedService> _logger;
        private static int countCreated = 0;
        private static int countStarted = 0;
        private static int countStopped = 0;

        public HostedService(ILogger<HostedService> logger)
        {
            _logger = logger;
            countCreated++;
            _logger.LogInformation($"[Service]: An instance of service has been created");
            _logger.LogInformation($"[Service]: {countCreated} created");
        }

        public Task StartAsync(CancellationToken token)
        {
            if(!token.IsCancellationRequested)
            {
                countStarted++;
                _logger.LogInformation($"[Service]: An instance of service has been started");
                _logger.LogInformation($"[Service]: {countStarted} started");
            }

            return Task.CompletedTask; 
        }

        public Task StopAsync(CancellationToken token)
        {
            if(!token.IsCancellationRequested)
            {
                countStopped++;
                _logger.LogInformation($"[Service]: An instance of service has been stopped");
                _logger.LogInformation($"[Service]: {countStopped} stopped");
            }

            return Task.CompletedTask;
        }


    }
}
