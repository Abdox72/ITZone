using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace platform_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeetingController : ControllerBase
    {
        private readonly ILogger<MeetingController> _logger;

        public MeetingController(ILogger<MeetingController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAllMeetings()
        {
            try
            {
                // Mock response - in real implementation, fetch from database
                var meetings = new List<MeetingResponse>
                {
                    new MeetingResponse
                    {
                        Id = 1,
                        Title = "ميتنج تجريبي",
                        Description = "ميتنج للاختبار",
                        StartTime = DateTime.UtcNow.AddHours(1),
                        EndTime = DateTime.UtcNow.AddHours(2),
                        Status = "scheduled",
                        ParticipantEmails = new List<string> { "test@example.com" },
                        CreatedAt = DateTime.UtcNow
                    }
                };

                return Ok(meetings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all meetings");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetMeetingById(int id)
        {
            try
            {
                // Mock response - in real implementation, fetch from database
                var meeting = new MeetingResponse
                {
                    Id = id,
                    Title = $"ميتنج رقم {id}",
                    Description = "وصف الميتنج",
                    StartTime = DateTime.UtcNow.AddHours(1),
                    EndTime = DateTime.UtcNow.AddHours(2),
                    Status = "scheduled",
                    ParticipantEmails = new List<string> { "test@example.com" },
                    CreatedAt = DateTime.UtcNow
                };

                return Ok(meeting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting meeting {Id}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost]
        public IActionResult CreateMeeting([FromBody] CreateMeetingRequest request)
        {
            try
            {
                // Mock response - in real implementation, save to database
                var meeting = new MeetingResponse
                {
                    Id = Random.Shared.Next(1, 10000),
                    Title = request.Title,
                    Description = request.Description,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Status = "scheduled",
                    ParticipantEmails = request.ParticipantEmails ?? new List<string>(),
                    CreatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Created meeting: {Title}", request.Title);
                return Ok(meeting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating meeting");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateMeeting(int id, [FromBody] UpdateMeetingRequest request)
        {
            try
            {
                // Mock response - in real implementation, update in database
                var meeting = new MeetingResponse
                {
                    Id = id,
                    Title = request.Title,
                    Description = request.Description,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Status = "updated",
                    ParticipantEmails = request.ParticipantEmails ?? new List<string>(),
                    CreatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Updated meeting: {Id}", id);
                return Ok(meeting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating meeting {Id}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteMeeting(int id)
        {
            try
            {
                _logger.LogInformation("Deleted meeting: {Id}", id);
                return Ok(new { message = "Meeting deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting meeting {Id}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("{id}/complete")]
        public IActionResult CompleteMeeting(int id)
        {
            try
            {
                _logger.LogInformation("Completed meeting: {Id}", id);
                return Ok(new { message = "Meeting completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing meeting {Id}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetUserMeetings(string userId)
        {
            try
            {
                // Mock response - in real implementation, fetch from database
                var meetings = new List<MeetingResponse>
                {
                    new MeetingResponse
                    {
                        Id = 1,
                        Title = $"ميتنج للمستخدم {userId}",
                        Description = "ميتنج خاص بالمستخدم",
                        StartTime = DateTime.UtcNow.AddHours(1),
                        EndTime = DateTime.UtcNow.AddHours(2),
                        Status = "scheduled",
                        ParticipantEmails = new List<string> { userId },
                        CreatedAt = DateTime.UtcNow
                    }
                };

                return Ok(meetings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting meetings for user {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("{meetingId}/participants")]
        public IActionResult AddParticipant(int meetingId, [FromBody] AddParticipantRequest request)
        {
            try
            {
                _logger.LogInformation("Added participant {Email} to meeting {MeetingId}", request.Email, meetingId);
                return Ok(new { message = "Participant added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding participant to meeting {MeetingId}", meetingId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{meetingId}/participants")]
        public IActionResult RemoveParticipant(int meetingId, [FromBody] RemoveParticipantRequest request)
        {
            try
            {
                _logger.LogInformation("Removed participant {Email} from meeting {MeetingId}", request.Email, meetingId);
                return Ok(new { message = "Participant removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing participant from meeting {MeetingId}", meetingId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }

    public class MeetingResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = "";
        public List<string> ParticipantEmails { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class CreateMeetingRequest
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<string>? ParticipantEmails { get; set; }
    }

    public class UpdateMeetingRequest
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<string>? ParticipantEmails { get; set; }
    }

    public class AddParticipantRequest
    {
        public string Email { get; set; } = "";
    }

    public class RemoveParticipantRequest
    {
        public string Email { get; set; } = "";
    }
} 