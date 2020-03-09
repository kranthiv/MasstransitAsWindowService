using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MasstransitHost
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IBusControl _busControl;
        public Worker(ILogger<Worker> logger, IBusControl busControl)
        {
            _logger = logger;
            _busControl = busControl;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Bus stopping");
            await _busControl.StopAsync(cancellationToken).ConfigureAwait(false);

            await base.StopAsync(cancellationToken);
            _logger.LogInformation("Bus stopped");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Bus starting");
            await _busControl.StartAsync(stoppingToken);
            _logger.LogInformation("Bus started");
        }
    }
}
