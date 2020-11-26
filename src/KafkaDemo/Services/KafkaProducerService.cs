using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KafkaDemo.Services
{
    public class KafkaProducerService : BaseService<KafkaProducerService>
    {
        private readonly IProducer<Null, string> _producer;

        public KafkaProducerService(ILogger<KafkaProducerService> logger, IConfiguration cfg) : base(logger, cfg)
        {
            var config = new ProducerConfig()
            {
                BootstrapServers = _bootstrapServers
            };
            _producer = new ProducerBuilder<Null, string>(config).Build();
        }
        
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            for (var i = 0; i < 100; ++i)
            {
                var value = $"Hello World {i}";
                _logger.LogInformation(value);
                await _producer.ProduceAsync(_topic, new Message<Null, string>()
                {
                    Value = value
                }, cancellationToken);
                Thread.Sleep(100);
            }

            _producer.Flush(TimeSpan.FromSeconds(10));
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _producer?.Dispose();
            return Task.CompletedTask;
        }
    }
}