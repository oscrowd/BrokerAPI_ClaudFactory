using Microsoft.Extensions.DependencyInjection;
namespace BrokerAPIClaudFactory.Broker
{
    public enum BrokerType
    {
        FileSystem,
        RabbitMQ,
        AzureServiceBus,
        Kafka
    }
    public static class BrokerFactory
    {
        public static IMessageBroker CreateBroker(BrokerType type, string connectionString = null)
        {
            return type switch
            {
                BrokerType.FileSystem => new FileSystemBroker("c:\\temp\\temp"),
                //BrokerType.RabbitMQ => new RabbitMQBroker(connectionString),
                //BrokerType.AzureServiceBus => new AzureServiceBusBroker(connectionString),
                //BrokerType.Kafka => new KafkaBroker(connectionString),
                _ => throw new ArgumentException("Unsupported broker type")
            };
        }
        /*
        public static class ServiceCollectionExtensions
        {
            public static IServiceCollection AddMessageBroker(this IServiceCollection services,
                BrokerType brokerType, string connectionString = null)
            {
                services.AddSingleton<IMessageBroker>(provider =>
                    BrokerFactory.CreateBroker(brokerType, connectionString));

                return services;
            }
        }
        */
    }
}
