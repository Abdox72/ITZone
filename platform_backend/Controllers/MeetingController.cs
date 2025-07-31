using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace platform_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeetingController : ControllerBase
    {
        private readonly ILogger<MeetingController> _logger;
        private static List<MeetingResponse> _meetings = new List<MeetingResponse>();
        private static int _nextId = 1;

        public MeetingController(ILogger<MeetingController> logger)
        {
            _logger = logger;
            
            // Initialize with sample data if empty
            if (_meetings.Count == 0)
            {
                _meetings.Add(new MeetingResponse
                {
                    Id = _nextId++,
                    Title = "ميتنج تجريبي",
                    Description = "ميتنج للاختبار والتطوير",
                    StartTime = DateTime.UtcNow.AddHours(1),
                    EndTime = DateTime.UtcNow.AddHours(2),
                    Status = "scheduled",
                    ParticipantEmails = new List<string> { "test@example.com", "user@example.com" },
                    CreatedAt = DateTime.UtcNow,
                    OrganizerEmail = "organizer@example.com",
                    JitsiRoomName = $"meeting-{_nextId - 1}",
                    IsRecordingEnabled = true,
                    RecordingUrl = "",
                    MeetingNotes = "",
                    Tasks = new List<MeetingTask>()
                });
            }
        }

        [HttpGet]
        public IActionResult GetAllMeetings()
        {
            try
            {
                var meetings = _meetings.OrderByDescending(m => m.CreatedAt).ToList();
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
                var meeting = _meetings.FirstOrDefault(m => m.Id == id);
                if (meeting == null)
                {
                    return NotFound(new { message = "Meeting not found" });
                }

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
                var meeting = new MeetingResponse
                {
                    Id = _nextId++,
                    Title = request.Title,
                    Description = request.Description,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Status = "scheduled",
                    ParticipantEmails = request.ParticipantEmails ?? new List<string>(),
                    CreatedAt = DateTime.UtcNow,
                    OrganizerEmail = request.OrganizerEmail ?? "organizer@example.com",
                    JitsiRoomName = $"meeting-{_nextId - 1}",
                    IsRecordingEnabled = request.IsRecordingEnabled,
                    RecordingUrl = "",
                    MeetingNotes = "",
                    Tasks = new List<MeetingTask>()
                };

                _meetings.Add(meeting);
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
                var meeting = _meetings.FirstOrDefault(m => m.Id == id);
                if (meeting == null)
                {
                    return NotFound(new { message = "Meeting not found" });
                }

                meeting.Title = request.Title;
                meeting.Description = request.Description;
                meeting.StartTime = request.StartTime;
                meeting.EndTime = request.EndTime;
                meeting.ParticipantEmails = request.ParticipantEmails ?? meeting.ParticipantEmails;
                meeting.IsRecordingEnabled = request.IsRecordingEnabled;

                _logger.LogInformation("Updated meeting: {Title}", request.Title);
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
                var meeting = _meetings.FirstOrDefault(m => m.Id == id);
                if (meeting == null)
                {
                    return NotFound(new { message = "Meeting not found" });
                }

                _meetings.Remove(meeting);
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
                var meeting = _meetings.FirstOrDefault(m => m.Id == id);
                if (meeting == null)
                {
                    return NotFound(new { message = "Meeting not found" });
                }

                meeting.Status = "completed";
                meeting.CompletedAt = DateTime.UtcNow;

                _logger.LogInformation("Completed meeting: {Id}", id);
                return Ok(meeting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing meeting {Id}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("{id}/start")]
        public IActionResult StartMeeting(int id)
        {
            try
            {
                var meeting = _meetings.FirstOrDefault(m => m.Id == id);
                if (meeting == null)
                {
                    return NotFound(new { message = "Meeting not found" });
                }

                meeting.Status = "in-progress";
                meeting.StartedAt = DateTime.UtcNow;

                _logger.LogInformation("Started meeting: {Id}", id);
                return Ok(meeting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting meeting {Id}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetUserMeetings(string userId)
        {
            try
            {
                var userMeetings = _meetings.Where(m => 
                    m.ParticipantEmails.Contains(userId) || 
                    m.OrganizerEmail == userId
                ).OrderByDescending(m => m.CreatedAt).ToList();

                return Ok(userMeetings);
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
                var meeting = _meetings.FirstOrDefault(m => m.Id == meetingId);
                if (meeting == null)
                {
                    return NotFound(new { message = "Meeting not found" });
                }

                if (!meeting.ParticipantEmails.Contains(request.Email))
                {
                    meeting.ParticipantEmails.Add(request.Email);
                }

                _logger.LogInformation("Added participant {Email} to meeting {Id}", request.Email, meetingId);
                return Ok(meeting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding participant to meeting {Id}", meetingId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{meetingId}/participants")]
        public IActionResult RemoveParticipant(int meetingId, [FromBody] RemoveParticipantRequest request)
        {
            try
            {
                var meeting = _meetings.FirstOrDefault(m => m.Id == meetingId);
                if (meeting == null)
                {
                    return NotFound(new { message = "Meeting not found" });
                }

                meeting.ParticipantEmails.Remove(request.Email);

                _logger.LogInformation("Removed participant {Email} from meeting {Id}", request.Email, meetingId);
                return Ok(meeting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing participant from meeting {Id}", meetingId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("{meetingId}/tasks")]
        public IActionResult AddTask(int meetingId, [FromBody] AddTaskRequest request)
        {
            try
            {
                var meeting = _meetings.FirstOrDefault(m => m.Id == meetingId);
                if (meeting == null)
                {
                    return NotFound(new { message = "Meeting not found" });
                }

                var task = new MeetingTask
                {
                    Id = meeting.Tasks.Count + 1,
                    Title = request.Title,
                    Description = request.Description,
                    AssignedTo = request.AssignedTo,
                    Priority = request.Priority,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow
                };

                meeting.Tasks.Add(task);

                _logger.LogInformation("Added task to meeting {Id}", meetingId);
                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding task to meeting {Id}", meetingId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{meetingId}/tasks/{taskId}")]
        public IActionResult UpdateTask(int meetingId, int taskId, [FromBody] UpdateTaskRequest request)
        {
            try
            {
                var meeting = _meetings.FirstOrDefault(m => m.Id == meetingId);
                if (meeting == null)
                {
                    return NotFound(new { message = "Meeting not found" });
                }

                var task = meeting.Tasks.FirstOrDefault(t => t.Id == taskId);
                if (task == null)
                {
                    return NotFound(new { message = "Task not found" });
                }

                task.Title = request.Title;
                task.Description = request.Description;
                task.AssignedTo = request.AssignedTo;
                task.Priority = request.Priority;
                task.Status = request.Status;

                _logger.LogInformation("Updated task {TaskId} in meeting {Id}", taskId, meetingId);
                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {TaskId} in meeting {Id}", taskId, meetingId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("{meetingId}/notes")]
        public IActionResult UpdateMeetingNotes(int meetingId, [FromBody] UpdateNotesRequest request)
        {
            try
            {
                var meeting = _meetings.FirstOrDefault(m => m.Id == meetingId);
                if (meeting == null)
                {
                    return NotFound(new { message = "Meeting not found" });
                }

                meeting.MeetingNotes = request.Notes;

                _logger.LogInformation("Updated notes for meeting {Id}", meetingId);
                return Ok(meeting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notes for meeting {Id}", meetingId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("{meetingId}/recording")]
        public IActionResult UpdateRecordingUrl(int meetingId, [FromBody] UpdateRecordingRequest request)
        {
            try
            {
                var meeting = _meetings.FirstOrDefault(m => m.Id == meetingId);
                if (meeting == null)
                {
                    return NotFound(new { message = "Meeting not found" });
                }

                meeting.RecordingUrl = request.RecordingUrl;

                _logger.LogInformation("Updated recording URL for meeting {Id}", meetingId);
                return Ok(meeting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating recording URL for meeting {Id}", meetingId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // Response Models
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
            public string OrganizerEmail { get; set; } = "";
            public string JitsiRoomName { get; set; } = "";
            public bool IsRecordingEnabled { get; set; }
            public string RecordingUrl { get; set; } = "";
            public string MeetingNotes { get; set; } = "";
            public List<MeetingTask> Tasks { get; set; } = new();
            public DateTime? StartedAt { get; set; }
            public DateTime? CompletedAt { get; set; }
        }

        public class MeetingTask
        {
            public int Id { get; set; }
            public string Title { get; set; } = "";
            public string Description { get; set; } = "";
            public string AssignedTo { get; set; } = "";
            public string Priority { get; set; } = "medium";
            public string Status { get; set; } = "pending";
            public DateTime CreatedAt { get; set; }
        }

        // Request Models
        public class CreateMeetingRequest
        {
            public string Title { get; set; } = "";
            public string Description { get; set; } = "";
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public List<string>? ParticipantEmails { get; set; }
            public string? OrganizerEmail { get; set; }
            public bool IsRecordingEnabled { get; set; } = true;
        }

        public class UpdateMeetingRequest
        {
            public string Title { get; set; } = "";
            public string Description { get; set; } = "";
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public List<string>? ParticipantEmails { get; set; }
            public bool IsRecordingEnabled { get; set; } = true;
        }

        public class AddParticipantRequest
        {
            public string Email { get; set; } = "";
        }

        public class RemoveParticipantRequest
        {
            public string Email { get; set; } = "";
        }

        public class AddTaskRequest
        {
            public string Title { get; set; } = "";
            public string Description { get; set; } = "";
            public string AssignedTo { get; set; } = "";
            public string Priority { get; set; } = "medium";
        }

        public class UpdateTaskRequest
        {
            public string Title { get; set; } = "";
            public string Description { get; set; } = "";
            public string AssignedTo { get; set; } = "";
            public string Priority { get; set; } = "medium";
            public string Status { get; set; } = "pending";
        }

        public class UpdateNotesRequest
        {
            public string Notes { get; set; } = "";
        }

        public class UpdateRecordingRequest
        {
            public string RecordingUrl { get; set; } = "";
        }
    }
} 