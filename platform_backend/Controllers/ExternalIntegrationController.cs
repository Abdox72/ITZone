using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace platform_backend.Controllers
{
    [Route("api/external-integration")]
    [ApiController]
    public class ExternalIntegrationController : ControllerBase
    {
        private readonly ILogger<ExternalIntegrationController> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        // حذف كل المتغيرات الخاصة بـ Jitsi REST API

        public ExternalIntegrationController(ILogger<ExternalIntegrationController> logger, IConfiguration configuration, HttpClient httpClient)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "ExternalIntegrationController is working!", timestamp = DateTime.UtcNow });
        }
        // ... existing code ...
    }

    public class CreateJitsiMeetingRequest
    {
        public string RoomName { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<string>? ParticipantEmails { get; set; }
        public bool EnableRecording { get; set; }
    }

    public class JitsiMeetingResponse
    {
        public int Id { get; set; }
        public string MeetingId { get; set; } = "";
        public string RoomName { get; set; } = "";
        public string JitsiUrl { get; set; } = "";
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = "";
        public bool IsRecording { get; set; }
        public string RecordingUrl { get; set; } = "";
        public List<string> ParticipantEmails { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class JitsiMeetingInfo
    {
        public string Id { get; set; } = "";
        public string RoomName { get; set; } = "";
        public string JitsiUrl { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<string>? Participants { get; set; }
        public bool IsRecording { get; set; }
        public string? RecordingUrl { get; set; }
        public string? TranscriptionUrl { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class JitsiRecordingInfo
    {
        public string RecordingId { get; set; } = "";
        public string FileName { get; set; } = "";
        public long FileSize { get; set; }
        public int Duration { get; set; }
        public string DownloadUrl { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class JitsiTranscriptionInfo
    {
        public string TranscriptionId { get; set; } = "";
        public string Text { get; set; } = "";
        public string Language { get; set; } = "";
        public double Confidence { get; set; }
        public List<TranscriptionSegment> Segments { get; set; } = new();
    }

    public class TranscriptionSegment
    {
        public double Start { get; set; }
        public double End { get; set; }
        public string Text { get; set; } = "";
        public string? Speaker { get; set; }
    }
} 