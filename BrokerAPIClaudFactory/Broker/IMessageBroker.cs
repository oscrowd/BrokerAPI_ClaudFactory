using BrokerAPIClaudFactory.Contracts;

namespace BrokerAPIClaudFactory.Broker
{
    public interface IMessageBroker
    {
        //Primitive
        Task<string> ProcessRequestAsync(IBrokerRequestContract brokerContracts);
        Task<string> WaitForResponseAsync(string key, int timeoutMs = 180000);
        Task CleanupAsync(string key);
        
        //Advanced
        Task<FileSystemResponseContract> ProcessRequestUnionAsync(IBrokerRequestContract brokerContracts);
        Task<string> WaitForResponseUnionAsync(string key, int timeoutMs = 180000);
        Task CleanupUnionAsync(string key);
    }
}
