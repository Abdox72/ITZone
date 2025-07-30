using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace platform_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AudioController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private const string AssemblyApiKey = "f72f2d7779924976acfbfbab7c6a3e4f";

        public AudioController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(15);
        }

        [HttpPost("transcribe-and-analyze")]
        public async Task<IActionResult> TranscribeAndAnalyze(IFormFile audioFile)
        {
            try
            {
                if (audioFile == null || audioFile.Length == 0)
                    return BadRequest("Please upload a valid audio file.");

                var validTypes = new[] { "audio/mpeg", "audio/wav", "audio/mp3", "audio/flac" };
                if (!validTypes.Contains(audioFile.ContentType))
                    return BadRequest("Unsupported audio file format. Supported formats: mp3, wav, flac.");

                // 1. Transcribe audio using AssemblyAI
                var audioUrl = await UploadToAssemblyAsync(audioFile);
                if (audioUrl.StartsWith("Error:"))
                    return BadRequest(audioUrl);

                var transcription = await GetTranscriptionAsync(audioUrl);
                if (transcription.StartsWith("Error:"))
                    return BadRequest(transcription);

                // 2. Analyze the transcribed text
                var cleanText = transcription?.Replace("\n", " ").Replace("\r", " ").Trim();
                if (string.IsNullOrWhiteSpace(cleanText))
                    return BadRequest("No text found after transcription.");

                // Prepare request body
                var requestObj = new AnalysisRequest { Text = cleanText };
                var requestContent = new StringContent(JsonSerializer.Serialize(requestObj), Encoding.UTF8, "application/json");

                // 3. Call summarize endpoint
                var summarizeResponse = await _httpClient.PostAsync("http://localhost:5211/api/audio/summarize", requestContent); // Adjust port if needed
                var summarizeResult = await summarizeResponse.Content.ReadAsStringAsync();

                // 4. Call tasks extraction endpoint
                var taskResponse = await _httpClient.PostAsync("http://localhost:5211/api/audio/tasks", requestContent);
                var taskResult = await taskResponse.Content.ReadAsStringAsync();

                return Ok(new
                {
                    success = true,
                    transcription = transcription,
                    summary = JsonDocument.Parse(summarizeResult).RootElement,
                    tasks = JsonDocument.Parse(taskResult).RootElement,
                    extractedText = cleanText
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        private async Task<string> UploadToAssemblyAsync(IFormFile audioFile)
        {
            try
            {
                var stream = audioFile.OpenReadStream();
                var content = new StreamContent(stream);
                content.Headers.ContentType = new MediaTypeHeaderValue(audioFile.ContentType);

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.assemblyai.com/v2/upload");
                request.Headers.Add("Authorization", AssemblyApiKey);
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Error: {response.StatusCode} - {errorContent}";
                }

                var json = await response.Content.ReadAsStringAsync();
                var root = JsonDocument.Parse(json).RootElement;
                if (!root.TryGetProperty("upload_url", out var uploadUrl))
                    return "Error: Upload URL not found in response.";

                return uploadUrl.GetString()!;
            }
            catch (Exception ex)
            {
                return $"Error uploading file: {ex.Message}";
            }
        }

        private async Task<string> GetTranscriptionAsync(string audioUrl)
        {
            try
            {
                var requestBody = new
                {
                    audio_url = audioUrl,
                    language_code = "ar"
                };
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.assemblyai.com/v2/transcript");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AssemblyApiKey);
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Error: {response.StatusCode} - {errorContent}";
                }

                var json = await response.Content.ReadAsStringAsync();
                var root = JsonDocument.Parse(json).RootElement;
                if (!root.TryGetProperty("id", out var idElement))
                    return "Error: Transcription ID not found in response.";

                var id = idElement.GetString();
                if (string.IsNullOrEmpty(id))
                    return "Error: Invalid transcription ID.";

                const int maxRetries = 30;
                int retries = 0;

                while (retries < maxRetries)
                {
                    var pollingRequest = new HttpRequestMessage(HttpMethod.Get, $"https://api.assemblyai.com/v2/transcript/{id}");
                    pollingRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AssemblyApiKey);

                    var pollingResponse = await _httpClient.SendAsync(pollingRequest);
                    var pollingJson = await pollingResponse.Content.ReadAsStringAsync();
                    var pollingRoot = JsonDocument.Parse(pollingJson).RootElement;

                    if (!pollingRoot.TryGetProperty("status", out var statusElement))
                        return "Error: Status not found in polling response.";

                    var status = statusElement.GetString();
                    if (status == "completed")
                    {
                        if (pollingRoot.TryGetProperty("text", out var textElement))
                            return textElement.GetString() ?? "Error: Transcription text is empty.";
                        return "Error: Transcription text not found.";
                    }
                    else if (status == "error")
                    {
                        var error = pollingRoot.TryGetProperty("error", out var errorElement)
                            ? errorElement.GetString()
                            : "Unknown error";
                        return $"Error: {error}";
                    }

                    await Task.Delay(5000);
                    retries++;
                }

                return "Error: Transcription timed out.";
            }
            catch (Exception ex)
            {
                return $"Error getting transcription: {ex.Message}";
            }
        }

        private async Task<IActionResult> CallOpenRouterModel(string prompt)
        {
            string apiKey = "sk-or-v1-Api"; // حط مفتاحك هنا أو اقرأه من الإعدادات
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
}