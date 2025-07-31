using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
            if (request == null || string.IsNullOrWhiteSpace(request.Text))
                return BadRequest(new { success = false, error = "Text is required." });

            var cleanText = request.Text.Replace("\n", " ").Replace("\r", " ").Trim();

            string apiKey = "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"; // 🔐 استخدم API Key صحيح وآمن
            if (string.IsNullOrEmpty(apiKey))
                return StatusCode(500, new { success = false, error = "Missing OpenRouter API key." });

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            // ✨ تعليمات للموديل تشمل التلخيص واستخراج المهام والمواعيد
            var prompt = $@"
أنت مساعد ذكي مهمتك تلخيص محادثة اجتماع مكتوبة باللغة العربية واستخراج:
1. ملخص الاجتماع بشكل موجز.
2. قائمة بالمهام المطلوب تنفيذها.
3. مواعيد نهائية (إن وجدت) مرتبطة بكل مهمة.

نص الاجتماع:
{cleanText}";

            var payload = new
            {
                model = "mistralai/mistral-7b-instruct",
                messages = new[]
                {
            new { role = "system", content = "أنت مساعد يحلل نصوص الاجتماعات العربية ويستخرج منها ملخصًا ومهامًا بمواعيد نهائية." },
            new { role = "user", content = prompt }
        },
                temperature = 0.3
            };

            string url = "https://openrouter.ai/api/v1/chat/completions";
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsync(url, content);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }

            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, new { success = false, error = body });

            using var doc = JsonDocument.Parse(body);
            var reply = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            return Ok(new
            {
                success = true,
                data = new
                {
                    analysis = reply,
                    originalText = request.Text
                }
            });
        }


        private async Task<IActionResult> CallOpenRouterModel(string prompt)
        {
            string apiKey = "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"; // حط مفتاحك هنا أو اقرأه من الإعدادات
            if (string.IsNullOrEmpty(apiKey))
                return StatusCode(500, new { success = false, error = "Missing OpenRouter API key." });

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var payload = new
            {
                model = "mistralai/mistral-7b-instruct",
                messages = new[]
                {
            new { role = "system", content = "أنت مساعد يحلل الاجتماعات المكتوبة باللغة العربية." },
            new { role = "user", content = prompt }
        },
                temperature = 0.3
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }

            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, new { success = false, error = body });

            using var doc = JsonDocument.Parse(body);
            var reply = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            return Ok(new
            {
                success = true,
                data = reply
            });
        }


        [HttpPost("summarize")]
        public async Task<IActionResult> SummarizeMeetingText([FromBody] AnalysisRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Text))
                return BadRequest(new { success = false, error = "Text is required." });

            var summaryPrompt = $@"
ألخص لك الاجتماع التالي باللغة العربية. من فضلك قدم ملخصاً موجزاً ومهماً وواضحاً:

النص:
{request.Text.Replace("\n", " ").Replace("\r", " ").Trim()}";

            var result = await CallOpenRouterModel(summaryPrompt);
            return result;
        }

        [HttpPost("tasks")]
        public async Task<IActionResult> ExtractTasksFromMeeting([FromBody] AnalysisRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Text))
                return BadRequest(new { success = false, error = "Text is required." });

            var taskPrompt = $@"
نص الاجتماع التالي مكتوب باللغة العربية. استخرج منه قائمة بالمهام المطلوب تنفيذها مع تحديد الموعد النهائي لكل مهمة إن وُجد. رجاءً استخدم تنسيقًا مرتبًا مثل:
- المهمة: ...
- الموعد النهائي: ... (إن وُجد)

النص:
{request.Text.Replace("\n", " ").Replace("\r", " ").Trim()}";

            var result = await CallOpenRouterModel(taskPrompt);
            return result;
        }

    }

    public class AnalysisRequest
    {
        public string Text { get; set; } = string.Empty;
    }
}