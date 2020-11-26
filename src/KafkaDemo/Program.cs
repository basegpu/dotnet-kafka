using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using KafkaDemo.Services;

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
                    collection.AddHostedService<KafkaAdminService>();
                    collection.AddHostedService<KafkaConsumerService>();
                    collection.AddHostedService<KafkaProducerService>();
                });
    }
}