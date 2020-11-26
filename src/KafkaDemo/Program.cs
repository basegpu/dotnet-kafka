using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Kafka.Public;
using Kafka.Public.Loggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KafkaDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddEnvironmentVariables(prefix: "KAFKA_");
                })
                .ConfigureServices((context, collection) =>
                {
                    collection.AddHostedService<KafkaAdminHostedService>();
                    collection.AddHostedService<KafkaConsumerHostedService>();
                    collection.AddHostedService<KafkaProducerHostedService>();
                });
    }

    public class KafkaAdminHostedService : IHostedService
    {
        private readonly ILogger<KafkaAdminHostedService> _logger;
        private readonly IAdminClient _client;

        public KafkaAdminHostedService(ILogger<KafkaAdminHostedService> logger, IConfiguration cfg)
        {
            _logger = logger;
            _client = new AdminClientBuilder(new AdminClientConfig
            {
                BootstrapServers = cfg.GetValue<string>("BOOTSTRAP_SERVERS")
            }).Build();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _client.CreateTopicsAsync(new TopicSpecification[] {
                    new TopicSpecification { Name = "demo", ReplicationFactor = 1, NumPartitions = 1 } });
            }
            catch (CreateTopicsException e)
            {
                _logger.LogError($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _client?.Dispose();
            return Task.CompletedTask;
        }
    }

    public class KafkaConsumerHostedService : IHostedService
    {
        private readonly ILogger<KafkaConsumerHostedService> _logger;
        private readonly ClusterClient _cluster;

        public KafkaConsumerHostedService(ILogger<KafkaConsumerHostedService> logger, IConfiguration cfg)
        {
            _logger = logger;
            _cluster = new ClusterClient(new Configuration
            {
                Seeds = cfg.GetValue<string>("BOOTSTRAP_SERVERS")
            }, new ConsoleLogger());
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cluster.ConsumeFromLatest("demo");
            _cluster.MessageReceived += record =>
            {
                _logger.LogInformation($"Received: {Encoding.UTF8.GetString(record.Value as byte[])}");
            };

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cluster?.Dispose();
            return Task.CompletedTask;
        }
    }

    public class KafkaProducerHostedService : IHostedService
    {
        private readonly ILogger<KafkaProducerHostedService> _logger;
        private readonly IProducer<Null, string> _producer;

        public KafkaProducerHostedService(ILogger<KafkaProducerHostedService> logger, IConfiguration cfg)
        {
            _logger = logger;
            var config = new ProducerConfig()
            {
                BootstrapServers = cfg.GetValue<string>("BOOTSTRAP_SERVERS")
            };
            _producer = new ProducerBuilder<Null, string>(config).Build();
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            for (var i = 0; i < 100; ++i)
            {
                var value = $"Hello World {i}";
                _logger.LogInformation(value);
                await _producer.ProduceAsync("demo", new Message<Null, string>()
                {
                    Value = value
                }, cancellationToken);
                Thread.Sleep(100);
            }

            _producer.Flush(TimeSpan.FromSeconds(10));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _producer?.Dispose();
            return Task.CompletedTask;
        }
    }
}