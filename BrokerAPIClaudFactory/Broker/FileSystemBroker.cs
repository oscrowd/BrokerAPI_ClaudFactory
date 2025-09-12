using BrokerAPIClaudFactory.Contracts;
using System.Text;
using System.Threading;

namespace BrokerAPIClaudFactory.Broker
{
    public class FileSystemBroker : BaseMessageBroker
    {
        public FileSystemBroker(string baseDirectory) : base(baseDirectory) { }

        public override async Task<string> ProcessRequestAsync(IBrokerRequestContract request)
        {
            var key = GenerateKey(String.Concat(request.Method, request.Path));
            await SaveRequestToFileAsync(request, key);
            return key;
        }
        public override async Task<FileSystemResponseContract> ProcessRequestUnionAsync(IBrokerRequestContract request)
        {
            var key = GenerateKey(String.Concat(request.Method, request.Path));

            lock (_lockObject)
            {
                // Проверяем, есть ли уже активный запрос с таким ключом
                if (_activeRequests.TryGetValue(key, out var existingRequest))
                {
                    // Добавляем клиента в список ожидающих ответа
                    existingRequest.WaitingClients.Add(GetClientIdentifier());
                    // Удалить лог
                    Console.WriteLine($"Request union. Waiting ... {existingRequest.WaitingClients.Count} clients waiting.");
                    return new FileSystemResponseContract
                    {
                        Key = key,
                        IsNewRequest = false,
                        Message = $"Request union. Waiting ... {existingRequest.WaitingClients.Count} clients waiting."
                    };

                }

                // Создаем новый запрос
                var requestInfo = new RequestInfo
                {
                    Key = key,
                    Method = request.Method,
                    Path = request.Path,
                    CreatedAt = DateTime.UtcNow,
                    WaitingClients = new List<string> { GetClientIdentifier() }
                };

                _activeRequests[key] = requestInfo;
            }

            // Для нового запроса - сохраняем в файл
            await SaveRequestToFileAsync(request, key);

            return new FileSystemResponseContract
            {
                Key = key,
                IsNewRequest = true,
                Message = "Request create and send to broker"
            };

        }

        public override async Task<string> WaitForResponseAsync(string key, int timeoutMs = 90000)
        {
            var responseFile = Path.Combine(_baseDirectory, $"{key}.resp");
            var cts = new CancellationTokenSource(timeoutMs);
            try
            {
               // Ожидаем появления файла ответа
                while (!cts.Token.IsCancellationRequested)
                {
                    if (File.Exists(responseFile))
                    {
                        var responseContent = await File.ReadAllTextAsync(responseFile);
                        return responseContent;
                    }
                    await Task.Delay(100, cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("Response timeout exceeded");
            }

            throw new TimeoutException("Response not received");
        }
        public override async Task<string> WaitForResponseUnionAsync(string key, int timeoutMs = 180000)
        {
            var responseFile = Path.Combine(_baseDirectory, $"{key}.resp");
            var cts = new CancellationTokenSource(timeoutMs);

            try
            {
                // Ожидаем появления файла ответа
                while (!cts.Token.IsCancellationRequested)
                {
                    if (File.Exists(responseFile))
                    {
                        var responseContent = await File.ReadAllTextAsync(responseFile);
                        return responseContent;
                    }
                    await Task.Delay(100, cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("Response timeout exceeded");
            }

            throw new TimeoutException("Response not received");
        }

        public override async Task CleanupUnionAsync(string key)
        {
            lock (_lockObject)
            {
                if (_activeRequests.TryGetValue(key, out var existingRequest))
                {
                
                existingRequest.WaitingClients--;
                var requestFile = Path.Combine(_baseDirectory, $"{key}.req");
                var responseFile = Path.Combine(_baseDirectory, $"{key}.resp");

               if (existingRequest.WaitingClients  <=0)
               {
               _activeRequests.TryRemove(key, out _);
               try
        try     {
                    if (File.Exists(requestFile))
                        File.Delete(requestFile);

                    if (File.Exists(responseFile))
                        File.Delete(responseFile);
                }
                catch (IOException)
                {
                    // Игнорируем ошибки удаления файлов
                }
                }
            }}

            
        }
        public override async Task CleanupAsync(string key)
        {
            var requestFile = Path.Combine(_baseDirectory, $"{key}.req");
            var responseFile = Path.Combine(_baseDirectory, $"{key}.resp");

            try
            {
                if (File.Exists(requestFile))
                    File.Delete(requestFile);

                if (File.Exists(responseFile))
                    File.Delete(responseFile);
            }
            catch (IOException ex)
            {
                // ТЗ невозможность удаления ответа и/или запроса.
                Console.WriteLine($"Cleanup error: {ex.Message}");
            }
        }

        private string GetClientIdentifier()
        {
            // Используем комбинацию из времени и GUID для идентификации клиента
            return $"{DateTime.UtcNow:HHmmssfff}_{Guid.NewGuid():N}";
        }
        private async Task SaveRequestToFileAsync(IBrokerRequestContract request, string key, int timeoutMs = 180000)
        {
            var cts = new CancellationTokenSource(timeoutMs);
            var requestFile = Path.Combine(_baseDirectory, $"{key}.req");
            var requestContent = new StringBuilder();
            requestContent.AppendLine($"method:{request.Method}\npath:{request.Path}");
            //ТЗ Обработка конфликта разных запросов 
            //if (File.Exists(requestFile)) Thread.Sleep(60000);  // ТЗ таймаут конфликта вызывающих для примитивной реализации - 1 минута
            // Ожидаем удаления  файла предыдущего запроса 
            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (!File.Exists(requestFile))
                    {
                        try
                        {
                            await File.WriteAllTextAsync(requestFile, requestContent.ToString());
                            return;
                        }
                        catch
                        {
                            throw new FileLoadException("В данный момент создание запроса невозможно. Попробуйте повторить запрос позже");
                        }
                    }
                    await Task.Delay(100, cts.Token);
                }
            }
            catch
            {
                throw new FileLoadException("Сервер по данному запросу занят. Попробуйте повторить попытку позже");
            }
            throw new FileLoadException("Сервер по данному запросу занят. Попробуйте повторить попытку позже");


        }
    }
}
