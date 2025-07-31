using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text.Json;
using System.Threading.Tasks;

namespace platform_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IntegrationController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        private const string TrelloApiKey = "api";
        private const string TrelloToken = "token";

        private const string GmailFrom = "ahmed.khaled.sayed2000@gmail.com";
        private const string GmailPassword = "pass"; // App Password من Gmail

        public IntegrationController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        // ✅ 1. إنشاء كرت Trello فقط
        [HttpPost("trello")]
        public async Task<IActionResult> CreateTrelloCardOnly([FromBody] TaskIntegrationRequest request)
        {
            var url = $"https://api.trello.com/1/cards?key={TrelloApiKey}&token={TrelloToken}&idList={request.TrelloListId}&name={request.TaskTitle}&desc={request.TaskDescription}";
            var response = await _httpClient.PostAsync(url, null);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode(500, new { success = false, error = "Trello integration failed", details = result });

            return Ok(new { success = true, card = JsonDocument.Parse(result).RootElement });
        }

        // ✅ 2. إرسال بريد Gmail فقط
        [HttpPost("gmail")]
        public async Task<IActionResult> SendGmailOnly([FromBody] TaskIntegrationRequest request)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new System.Net.NetworkCredential(GmailFrom, GmailPassword),
                    EnableSsl = true,
                };

                var mail = new MailMessage(GmailFrom, request.GmailTo, request.GmailSubject, request.GmailBody)
                {
                    IsBodyHtml = true // في حالة إرسال HTML
                };

                await smtpClient.SendMailAsync(mail);
                return Ok(new { success = true, message = "Email sent successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "Gmail sending failed", details = ex.Message });
            }
        }
    }

    // ✅ موديل الطلب
    public class TaskIntegrationRequest
    {
        public string TaskTitle { get; set; }
        public string TaskDescription { get; set; }
        public string TrelloListId { get; set; }
        public string GmailTo { get; set; }
        public string GmailSubject { get; set; }
        public string GmailBody { get; set; }
    }
}
