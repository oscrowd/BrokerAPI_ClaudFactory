using BrokerAPIClaudFactory.Broker;
using BrokerAPIClaudFactory.Contracts;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BrokerAPIClaudFactory.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrokerController : ControllerBase
    {
        private readonly IMessageBroker _broker;
        private ILogger _logger;

        public BrokerController(IMessageBroker broker, ILogger logger)
        {
            _broker = broker;
            _logger = logger;
            
        }

        [HttpPost("primitive")]
        public async Task<IActionResult> PrimitiveRequest([FromBody] FileSystemRequestContract addFileRequest)
        {
            try
            {
                var key = await _broker.ProcessRequestAsync(addFileRequest);
                // Ждем ответа от брокера
                var response = await _broker.WaitForResponseAsync(key);

                // Очищаем файлы
                await _broker.CleanupAsync(key);

                // Парсим ответ
                var lines = response.Split('\n');
                if (lines.Length < 1 || !int.TryParse(lines[0], out var statusCode))
                {
                    _logger.WriteError("500 Invalid response format");
                    return StatusCode(500, "Invalid response format");
                }

                var responseBody = lines.Length > 1 ? string.Join("\n", lines[1..]) : string.Empty;

                return StatusCode(statusCode, responseBody);
            }
            catch (TimeoutException ex)
            {
                _logger.WriteError("500 Invalid response format");
                return StatusCode(408, ex.Message); // Request Timeout
            }
            catch (FileLoadException ex)
            {
                _logger.WriteError(ex.Message);
                return StatusCode(500, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

       
        [HttpPost("advanced")]
        public async Task<IActionResult> AdvancedRequest2(FileSystemRequestContract request)
        {
            string key = string.Empty;
            Object obj;
            try
            {
                var result = await _broker.ProcessRequestUnionAsync(request);
                key = result.Key;
                if (!result.IsNewRequest)
                {
                    // Для схлопнутого запроса просто ждем ответа
                    _logger.WriteError($"Request uinion {result.Key} {result.Message}");
                }
                else
                {
                    _logger.WriteError($"New request: {result.Key}");
                }

                // Все клиенты ждут ответа
                var response = await _broker.WaitForResponseUnionAsync(result.Key);

                // Парсим ответ
                var lines = response.Split('\n');
                if (lines.Length < 1 || !int.TryParse(lines[0], out var statusCode))
                {
                    return StatusCode(500, "Invalid response format");
                }

                var responseBody = lines.Length > 1 ? string.Join("\n", lines[1..]) : string.Empty;
    
                obj= new
                {
                    Response = responseBody,
                    WasCollapsed = !result.IsNewRequest,
                    RequestKey = result.Key
                };

                
            }
            catch (TimeoutException ex)
            {
                return StatusCode(408, new { Error = "Request timeout", Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Internal server error", Message = ex.Message });
            }
            finally 
            {
                await _broker.CleanupUnionAsync(key);
            }
            if (obj != null) return StatusCode(200, obj);
            else return StatusCode(500, "Error broker algorithm");
        }
    }
}
