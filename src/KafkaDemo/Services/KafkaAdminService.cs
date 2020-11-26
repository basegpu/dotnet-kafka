using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KafkaDemo.Services
{
    public class KafkaAdminService : BaseService<KafkaAdminService>
    {
        private readonly IAdminClient _client;

        public KafkaAdminService(ILogger<KafkaAdminService> logger, IConfiguration cfg) : base(logger, cfg)
        {
            _client = new AdminClientBuilder(new AdminClientConfig
            {
                BootstrapServers = _bootstrapServers
            }).Build();
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _client.CreateTopicsAsync(new TopicSpecification[] {
                    new TopicSpecification { Name = _topic, ReplicationFactor = 1, NumPartitions = 1 } });
            }
            catch (CreateTopicsException e)
            {
                _logger.LogError($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
            }

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _client?.Dispose();
            return Task.CompletedTask;
        }
    }
}