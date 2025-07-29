using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace platform_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalysisController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public AnalysisController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeMeetingText([FromBody] AnalysisRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("Text is required.");

            string apiUrl = "https://api-inference.huggingface.co/models/mistralai/Mistral-7B-Instruct";
            string apiKey = "API";

            string prompt = $"Extract all tasks and deadlines from the following Arabic meeting transcript and summarize them in bullet points:\n\n{request.Text}";

            var body = new
            {
                inputs = prompt,
                parameters = new
                {
                    temperature = 0.3,
                    max_new_tokens = 300
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.PostAsync(apiUrl, jsonContent);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, error);
            }

            var result = await response.Content.ReadAsStringAsync();
            return Ok(result);
        }
    }

    public class AnalysisRequest
    {
        public string Text { get; set; }
    }
}
