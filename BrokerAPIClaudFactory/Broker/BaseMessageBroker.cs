using BrokerAPIClaudFactory.Contracts;
using System.Collections.Concurrent;

namespace BrokerAPIClaudFactory.Broker
{
    public abstract class BaseMessageBroker : IMessageBroker
    {
        protected readonly string _baseDirectory;
        protected readonly object _lockObject = new object();
        protected readonly TimeSpan _requestTimeout = TimeSpan.FromSeconds(200000);
        protected readonly ConcurrentDictionary<string, RequestInfo> _activeRequests;
        public BaseMessageBroker(string baseDirectory="broker")
        {
            _baseDirectory = baseDirectory;
            Directory.CreateDirectory(_baseDirectory);
            _activeRequests = new ConcurrentDictionary<string, RequestInfo>();
            // Запускаем очистку старых запросов
            StartCleanupTask();
        }
        // ТЗ Primitive
        public abstract Task<string> ProcessRequestAsync(IBrokerRequestContract brokerContract);
        public abstract Task<string> WaitForResponseAsync(string key, int timeoutMs = 30000);
        public abstract Task CleanupAsync(string key);
        //ТЗ Advanced

        public abstract Task<FileSystemResponseContract> ProcessRequestUnionAsync(IBrokerRequestContract brokerContract);
        public abstract Task<string> WaitForResponseUnionAsync(string key, int timeoutMs = 30000);
        public abstract Task CleanupUnionAsync(string key);

        //ТЗ Возможность расширения/замены функций ключей
        public virtual string GenerateKey(string str) 
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(str);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return Convert.ToHexString(hashBytes); 
            }
        }
        private void StartCleanupTask()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromMinutes(1));

                    lock (_lockObject)
                    {
                        var now = DateTime.UtcNow;
                        var oldKeys = _activeRequests
                            .Where(kv => now - kv.Value.CreatedAt > _requestTimeout)
                            .Select(kv => kv.Key)
                            .ToList();

                        foreach (var key in oldKeys)
                        {
                            _activeRequests.TryRemove(key, out _);
                        }
                    }
                }
            });
        }
    }
}
