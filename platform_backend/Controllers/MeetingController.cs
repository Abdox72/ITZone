using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace platform_backend.Controllers
{
    [Authorize]
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

        /// <summary>
        /// Helper method to get current user's email from JWT claims
        /// </summary>
        private string GetCurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value ?? "unknown@example.com";
        }

        /// <summary>
        /// Helper method to get current user's ID from JWT claims
        /// </summary>
        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
        }

        /// <summary>
        /// Check if current user has access to the meeting (organizer or participant)
        /// </summary>
        private bool HasMeetingAccess(MeetingResponse meeting)
        {
            var userEmail = GetCurrentUserEmail();
            return meeting.OrganizerEmail == userEmail || 
                   meeting.ParticipantEmails.Contains(userEmail);
        }

        [HttpGet]
        public IActionResult GetAllMeetings()
        {
            try
            {
                var userEmail = GetCurrentUserEmail();
                // Only return meetings where user is organizer or participant
                var userMeetings = _meetings.Where(m => 
                    m.OrganizerEmail == userEmail || 
                    m.ParticipantEmails.Contains(userEmail)
                ).OrderByDescending(m => m.CreatedAt).ToList();
                
                return Ok(userMeetings);
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

                // Check if user has access to this meeting
                if (!HasMeetingAccess(meeting))
                {
                    return Forbid("You don't have access to this meeting");
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
                var currentUserEmail = GetCurrentUserEmail();
                
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
                    OrganizerEmail = currentUserEmail, // Use authenticated user's email
                    JitsiRoomName = $"meeting-{_nextId - 1}",
                    IsRecordingEnabled = request.IsRecordingEnabled,
                    RecordingUrl = "",
                    MeetingNotes = "",
                    Tasks = new List<MeetingTask>()
                };

                _meetings.Add(meeting);
                _logger.LogInformation("Created meeting: {Title} by user: {Email}", request.Title, currentUserEmail);
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

                var currentUserEmail = GetCurrentUserEmail();
                // Only organizer can update meeting
                if (meeting.OrganizerEmail != currentUserEmail)
                {
                    return Forbid("Only the meeting organizer can update this meeting");
                }

                meeting.Title = request.Title;
                meeting.Description = request.Description;
                meeting.StartTime = request.StartTime;
                meeting.EndTime = request.EndTime;
                meeting.ParticipantEmails = request.ParticipantEmails ?? meeting.ParticipantEmails;
                meeting.IsRecordingEnabled = request.IsRecordingEnabled;

                _logger.LogInformation("Updated meeting: {Title} by user: {Email}", request.Title, currentUserEmail);
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

                var currentUserEmail = GetCurrentUserEmail();
                // Only organizer can delete meeting
                if (meeting.OrganizerEmail != currentUserEmail)
                {
                    return Forbid("Only the meeting organizer can delete this meeting");
                }

                _meetings.Remove(meeting);
                _logger.LogInformation("Deleted meeting: {Id} by user: {Email}", id, currentUserEmail);
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

                // Check if user has access to this meeting
                if (!HasMeetingAccess(meeting))
                {
                    return Forbid("You don't have access to this meeting");
                }

                meeting.Status = "completed";
                meeting.CompletedAt = DateTime.UtcNow;

                var currentUserEmail = GetCurrentUserEmail();
                _logger.LogInformation("Completed meeting: {Id} by user: {Email}", id, currentUserEmail);
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

                // Check if user has access to this meeting
                if (!HasMeetingAccess(meeting))
                {
                    return Forbid("You don't have access to this meeting");
                }

                meeting.Status = "in-progress";
                meeting.StartedAt = DateTime.UtcNow;

                var currentUserEmail = GetCurrentUserEmail();
                _logger.LogInformation("Started meeting: {Id} by user: {Email}", id, currentUserEmail);
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
                var currentUserEmail = GetCurrentUserEmail();
                var currentUserId = GetCurrentUserId();
                
                // Users can only access their own meetings
                if (userId != currentUserEmail && userId != currentUserId)
                {
                    return Forbid("You can only access your own meetings");
                }

                var userMeetings = _meetings.Where(m => 
                    m.ParticipantEmails.Contains(userId) || 
                    m.OrganizerEmail == userId ||
                    m.ParticipantEmails.Contains(currentUserEmail) || 
                    m.OrganizerEmail == currentUserEmail
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

                var currentUserEmail = GetCurrentUserEmail();
                // Only organizer can add participants
                if (meeting.OrganizerEmail != currentUserEmail)
                {
                    return Forbid("Only the meeting organizer can add participants");
                }

                if (!meeting.ParticipantEmails.Contains(request.Email))
                {
                    meeting.ParticipantEmails.Add(request.Email);
                }

                _logger.LogInformation("Added participant {Email} to meeting {Id} by user: {CurrentUser}", 
                    request.Email, meetingId, currentUserEmail);
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

                var currentUserEmail = GetCurrentUserEmail();
                // Only organizer can remove participants, or users can remove themselves
                if (meeting.OrganizerEmail != currentUserEmail && request.Email != currentUserEmail)
                {
                    return Forbid("Only the meeting organizer can remove participants, or you can remove yourself");
                }

                meeting.ParticipantEmails.Remove(request.Email);

                _logger.LogInformation("Removed participant {Email} from meeting {Id} by user: {CurrentUser}", 
                    request.Email, meetingId, currentUserEmail);
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

                // Check if user has access to this meeting
                if (!HasMeetingAccess(meeting))
                {
                    return Forbid("You don't have access to this meeting");
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

                var currentUserEmail = GetCurrentUserEmail();
                _logger.LogInformation("Added task to meeting {Id} by user: {Email}", meetingId, currentUserEmail);
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

                // Check if user has access to this meeting
                if (!HasMeetingAccess(meeting))
                {
                    return Forbid("You don't have access to this meeting");
                }

                var task = meeting.Tasks.FirstOrDefault(t => t.Id == taskId);
                if (task == null)
                {
                    return NotFound(new { message = "Task not found" });
                }

                var currentUserEmail = GetCurrentUserEmail();
                // Users can update tasks assigned to them, or organizer can update any task
                if (task.AssignedTo != currentUserEmail && meeting.OrganizerEmail != currentUserEmail)
                {
                    return Forbid("You can only update tasks assigned to you, or you must be the meeting organizer");
                }

                task.Title = request.Title;
                task.Description = request.Description;
                task.AssignedTo = request.AssignedTo;
                task.Priority = request.Priority;
                task.Status = request.Status;

                _logger.LogInformation("Updated task {TaskId} in meeting {Id} by user: {Email}", taskId, meetingId, currentUserEmail);
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

                // Check if user has access to this meeting
                if (!HasMeetingAccess(meeting))
                {
                    return Forbid("You don't have access to this meeting");
                }

                meeting.MeetingNotes = request.Notes;

                var currentUserEmail = GetCurrentUserEmail();
                _logger.LogInformation("Updated notes for meeting {Id} by user: {Email}", meetingId, currentUserEmail);
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