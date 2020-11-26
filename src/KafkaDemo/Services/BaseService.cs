using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace KafkaDemo.Services
{
    public abstract class BaseService<T> : IHostedService
    {
        protected readonly ILogger<T> _logger;
        protected readonly string _topic;

        protected readonly string _bootstrapServers;

        protected BaseService(ILogger<T> logger, IConfiguration cfg)
        {
            _logger = logger;
            _topic = cfg.GetValue<string>("TOPIC_NAME");
            _bootstrapServers = cfg.GetValue<string>("BOOTSTRAP_SERVERS");
        }

        public abstract Task StartAsync(CancellationToken ct);
        public abstract Task StopAsync(CancellationToken ct);
    }
}