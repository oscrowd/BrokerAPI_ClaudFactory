using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BrokerAPIClaudFactory.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

class Program
{
    private static readonly HttpClient _client = new HttpClient();
    private const int _totalRequests = 5;
private const string _apiUrl = "http://localhost:5288/api/Broker/advanced";

    static async Task Main(string[] args)
    {
        // Инициализация тестового клиента
        using var application = new WebApplicationFactory<BrokerAPIClaudFactory.Controllers.BrokerController>();
        
        // Подготовка тестовых данных
        var testRequest = new FileSystemRequestContract
        {
            Method = "GET",
            Path = _apiUrl,
        };

        // Запуск нагрузочного тестирования
        var tasks = new Task[_totalRequests];
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < _totalRequests; i++)
        {
            tasks[i] = SendRequestAsync(testRequest);
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Вывод результатов
        Console.WriteLine($"Выполнено запросов: {_totalRequests}");
        Console.WriteLine($"Общее время: {stopwatch.Elapsed.TotalSeconds:F2} секунд");
        Console.WriteLine($"RPS: {(int)(_totalRequests / stopwatch.Elapsed.TotalSeconds)}");
    }

    static async Task SendRequestAsync(FileSystemRequestContract request)
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(request), 
                Encoding.UTF8, 
                "application/json"
            );

            var response = await _client.PostAsync(_apiUrl, content);
            response.EnsureSuccessStatusCode();
            
            // Логирование успешных ответов
            Console.WriteLine($"Успешный запрос: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            // Логирование ошибок
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
}
