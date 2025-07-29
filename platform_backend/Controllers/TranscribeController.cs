using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace platform_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TranscribeController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private const string AssemblyApiKey = "f72f2d7779924976acfbfbab7c6a3e4f";

        public TranscribeController()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(15)
            };
        }

        [HttpPost("transcribe")]
        public async Task<IActionResult> TranscribeAudio(IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
                return BadRequest("Please upload a valid audio file.");

            var validTypes = new[] { "audio/mpeg", "audio/wav", "audio/mp3", "audio/flac" };
            if (!validTypes.Contains(audioFile.ContentType))
                return BadRequest("Unsupported audio file format. Supported formats: mp3, wav, flac.");

            var audioUrl = await UploadToAssemblyAsync(audioFile);
            if (audioUrl.StartsWith("Error:")) return BadRequest(audioUrl);

            var transcription = await GetTranscriptionAsync(audioUrl);
            if (transcription.StartsWith("Error:")) return BadRequest(transcription);

            return Ok(new
            {
                transcription
            });
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
    }
}
