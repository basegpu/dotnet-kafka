using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kafka.Public;
using Kafka.Public.Loggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KafkaDemo.Services
{
    public class KafkaConsumerService : BaseService<KafkaConsumerService>
    {
        private readonly ClusterClient _cluster;

        public KafkaConsumerService(ILogger<KafkaConsumerService> logger, IConfiguration cfg) : base(logger, cfg)
        {
            _cluster = new ClusterClient(new Configuration
            {
                Seeds = _bootstrapServers
            }, new ConsoleLogger());
        }
        
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _cluster.ConsumeFromLatest(_topic);
            _cluster.MessageReceived += record =>
            {
                _logger.LogInformation($"Received: {Encoding.UTF8.GetString(record.Value as byte[])}");
            };

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _cluster?.Dispose();
            return Task.CompletedTask;
        }
    }
}