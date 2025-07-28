using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace platform_backend.Controllers
{
    [Route("api/external-integration")]
    [ApiController]
    public class ExternalIntegrationController : ControllerBase
    {
        private readonly ILogger<ExternalIntegrationController> _logger;

        public ExternalIntegrationController(ILogger<ExternalIntegrationController> logger)
        {
            _logger = logger;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "ExternalIntegrationController is working!", timestamp = DateTime.UtcNow });
        }

        [HttpPost("jitsi/create")]
        public IActionResult CreateJitsiMeeting([FromBody] CreateJitsiMeetingRequest request)
        {
            try
            {
                // Generate a unique meeting ID
                var meetingId = Guid.NewGuid().ToString("N").Substring(0, 8);
                var roomName = string.IsNullOrEmpty(request.RoomName) ? $"meeting-{meetingId}" : request.RoomName;
                
                // Create Jitsi URL
                var jitsiUrl = $"https://meet.jit.si/{roomName}";

                var meeting = new JitsiMeetingResponse
                {
                    Id = Random.Shared.Next(1, 10000),
                    MeetingId = meetingId,
                    RoomName = roomName,
                    JitsiUrl = jitsiUrl,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Status = "scheduled",
                    IsRecording = false,
                    RecordingUrl = "",
                    ParticipantEmails = request.ParticipantEmails ?? new List<string>(),
                    CreatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Created Jitsi meeting: {MeetingId}", meetingId);
                return Ok(meeting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Jitsi meeting");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("jitsi/{meetingId}")]
        public IActionResult GetJitsiMeeting(int meetingId)
        {
            try
            {
                // Mock response - in real implementation, fetch from database
                var meeting = new JitsiMeetingResponse
                {
                    Id = meetingId,
                    MeetingId = $"meeting-{meetingId}",
                    RoomName = $"Meeting {meetingId}",
                    JitsiUrl = $"https://meet.jit.si/meeting-{meetingId}",
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(1),
                    Status = "active",
                    IsRecording = false,
                    RecordingUrl = "",
                    ParticipantEmails = new List<string>(),
                    CreatedAt = DateTime.UtcNow
                };

                return Ok(meeting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Jitsi meeting {MeetingId}", meetingId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("jitsi/{meetingId}/start-recording")]
        public IActionResult StartRecording(int meetingId)
        {
            try
            {
                _logger.LogInformation("Starting recording for meeting: {MeetingId}", meetingId);
                
                // Mock response - in real implementation, call Jitsi API
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting recording for meeting {MeetingId}", meetingId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("jitsi/{meetingId}/stop-recording")]
        public IActionResult StopRecording(int meetingId)
        {
            try
            {
                _logger.LogInformation("Stopping recording for meeting: {MeetingId}", meetingId);
                
                // Mock response - in real implementation, call Jitsi API
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping recording for meeting {MeetingId}", meetingId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("jitsi/{meetingId}/recording-url")]
        public IActionResult GetRecordingUrl(int meetingId)
        {
            try
            {
                // Mock recording URL - in real implementation, get from Jitsi
                var recordingUrl = $"https://recordings.jit.si/meeting-{meetingId}.mp4";
                return Ok(recordingUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recording URL for meeting {MeetingId}", meetingId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("recordings/{recordingId}/process")]
        public IActionResult ProcessRecordingAutomatically(int recordingId)
        {
            try
            {
                _logger.LogInformation("Processing recording automatically: {RecordingId}", recordingId);
                
                // Mock response - in real implementation, process the recording
                var summary = new
                {
                    summary = "تم تحليل التسجيل بنجاح",
                    actionItems = new[] { "متابعة المهام المطلوبة", "إرسال الملخص للمشاركين" },
                    keyDecisions = new[] { "قرار مهم 1", "قرار مهم 2" }
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recording {RecordingId}", recordingId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
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
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = "";
        public bool IsRecording { get; set; }
        public string RecordingUrl { get; set; } = "";
        public List<string> ParticipantEmails { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
} 