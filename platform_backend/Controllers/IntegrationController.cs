//using Microsoft.AspNetCore.Mvc;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Net.Mail;
//using System.Text.Json;
//using System.Threading.Tasks;

//namespace platform_backend.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class IntegrationController : ControllerBase
//    {
//        private readonly HttpClient _httpClient;

//        private const string TrelloApiKey = "api";
//        private const string TrelloToken = "token";

//        private const string GmailFrom = "ahmed.khaled.sayed2000@gmail.com";
//        private const string GmailPassword = "pass"; // App Password من Gmail

//        public IntegrationController(IHttpClientFactory httpClientFactory)
//        {
//            _httpClient = httpClientFactory.CreateClient();
//        }

//        // ✅ 1. إنشاء كرت Trello فقط
//        [HttpPost("trello")]
//        public async Task<IActionResult> CreateTrelloCardOnly([FromBody] TaskIntegrationRequest request)
//        {
//            var url = $"https://api.trello.com/1/cards?key={TrelloApiKey}&token={TrelloToken}&idList={request.TrelloListId}&name={request.TaskTitle}&desc={request.TaskDescription}";
//            var response = await _httpClient.PostAsync(url, null);
//            var result = await response.Content.ReadAsStringAsync();

//            if (!response.IsSuccessStatusCode)
//                return StatusCode(500, new { success = false, error = "Trello integration failed", details = result });

//            return Ok(new { success = true, card = JsonDocument.Parse(result).RootElement });
//        }

//        // ✅ 2. إرسال بريد Gmail فقط
//        [HttpPost("gmail")]
//        public async Task<IActionResult> SendGmailOnly([FromBody] TaskIntegrationRequest request)
//        {
//            try
//            {
//                var smtpClient = new SmtpClient("smtp.gmail.com")
//                {
//                    Port = 587,
//                    Credentials = new System.Net.NetworkCredential(GmailFrom, GmailPassword),
//                    EnableSsl = true,
//                };

//                var mail = new MailMessage(GmailFrom, request.GmailTo, request.GmailSubject, request.GmailBody)
//                {
//                    IsBodyHtml = true // في حالة إرسال HTML
//                };

//                await smtpClient.SendMailAsync(mail);
//                return Ok(new { success = true, message = "Email sent successfully" });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { success = false, error = "Gmail sending failed", details = ex.Message });
//            }
//        }
//    }

//    // ✅ موديل الطلب
//    public class TaskIntegrationRequest
//    {
//        public string TaskTitle { get; set; }
//        public string TaskDescription { get; set; }
//        public string TrelloListId { get; set; }
//        public string GmailTo { get; set; }
//        public string GmailSubject { get; set; }
//        public string GmailBody { get; set; }
//    }
//}
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text.Json;
using System.Threading.Tasks;

namespace platform_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IntegrationController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        private const string TrelloApiKey = "9afc69e1f50921dd3c8b6624af88b141";      // 🔁 Replace with your real API key
        private const string TrelloToken = "ATTA79ed1f691a0873aba87045a4dec6e7d305d907c90ca7b48c7ec295849c77024eAC857D14";     // 🔁 Replace with your real token

        private const string GmailFrom = "ahmed.khaled.sayed2000@gmail.com";
        private const string GmailPassword = "gzzx pcpk luig uwsd";    // 🔁 Replace with your Gmail App Password

        public IntegrationController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpPost("send-all")]
        public async Task<IActionResult> CreateTrelloCardAndSendEmail([FromBody] TaskIntegrationRequest request)
        {
            try
            {
                // ✅ 1. إنشاء كرت Trello
                var trelloUrl = $"https://api.trello.com/1/cards" +
                                $"?key={TrelloApiKey}" +
                                $"&token={TrelloToken}" +
                                $"&idList={request.TrelloListId}" +
                                $"&name={Uri.EscapeDataString(request.TaskTitle)}" +
                                $"&desc={Uri.EscapeDataString(request.TaskDescription)}";

                var response = await _httpClient.PostAsync(trelloUrl, null);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return StatusCode(500, new { success = false, error = "Trello integration failed", details = result });

                var json = JsonDocument.Parse(result);
                var cardUrl = json.RootElement.GetProperty("shortUrl").GetString();

                // ✅ 2. بناء HTML Email Body
                string htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                  <meta charset='UTF-8'>
                  <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
                    .header {{ background-color: #007bff; color: white; padding: 10px; text-align: center; }}
                    .section {{ margin: 20px 0; }}
                    .action {{ background-color: #f8f9fa; padding: 10px; border-left: 4px solid #007bff; }}
                    a {{ color: #007bff; text-decoration: none; }}
                  </style>
                </head>
                <body>
                  <div class='header'>
                    <h2>📋 Task Created: {request.TaskTitle}</h2>
                    <p>{DateTime.Now:MMMM dd, yyyy}</p>
                  </div>

                  <div class='section'>
                    <h3>🔍 Description</h3>
                    <p>{request.TaskDescription}</p>
                  </div>

                  <div class='section'>
                    <h3>✅ Trello Card</h3>
                    <div class='action'>
                      <p>تم إنشاء المهمة في Trello تحت القائمة ID: <strong>{request.TrelloListId}</strong></p>
                      <p><a href='{cardUrl}'>📎 Open Card in Trello</a></p>
                    </div>
                  </div>
                </body>
                </html>";

                // ✅ 3. إرسال الإيميل عبر Gmail
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(GmailFrom, GmailPassword),
                    EnableSsl = true,
                };

                var mail = new MailMessage(GmailFrom, request.GmailTo)
                {
                    Subject = string.IsNullOrEmpty(request.GmailSubject) ? $"📋 Task Update: {request.TaskTitle}" : request.GmailSubject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                await smtpClient.SendMailAsync(mail);

                return Ok(new
                {
                    success = true,
                    cardUrl,
                    message = "Trello card created and email sent successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "Operation failed", details = ex.Message });
            }
        }
    }

    // ✅ الموديل
    public class TaskIntegrationRequest
    {
        public string TaskTitle { get; set; }
        public string TaskDescription { get; set; }
        public string TrelloListId { get; set; } = "688a9aa0be4c80cab3967cac";

        public string GmailTo { get; set; }
        public string GmailSubject { get; set; }
    }
}
