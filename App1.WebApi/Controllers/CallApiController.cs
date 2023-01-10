using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry;

namespace App1.WebApi.Controllers
{
    [ApiController]
    [Route("http")]
    public class CallApiController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CallApiController> _logger;
        private static readonly ActivitySource ActivitySource = new(nameof(CallApiController));

        public CallApiController(
            IHttpClientFactory httpClientFactory, 
            IConfiguration configuration, 
            ILogger<CallApiController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public async Task<string> Get()
        {

            using (var parentActivity = ActivitySource.StartActivity("Parent Span"))
            {
                _logger.LogInformation($"Logging current activity: {JsonSerializer.Serialize(Activity.Current)}");

                using (var childActivity = ActivitySource.StartActivity("Child Span"))
                {
                    _logger.LogInformation($"Logging current activity: {JsonSerializer.Serialize(Activity.Current)}");
                    var childResponse = await _httpClientFactory
                        .CreateClient()
                        .GetStringAsync(_configuration["App3Endpoint"]);
                }
                using (var childActivity = ActivitySource.StartActivity("Child Span"))
                {
                    _logger.LogInformation($"Logging current activity: {JsonSerializer.Serialize(Activity.Current)}");
                }
            }

                Baggage.SetBaggage("App1", "CallApiController");
            _logger.LogInformation($"Calling App3: {_configuration["App3Endpoint"]}");
            var response  = await _httpClientFactory
                .CreateClient()
                .GetStringAsync(_configuration["App3Endpoint"]);

            return response;

        }
    }
}
