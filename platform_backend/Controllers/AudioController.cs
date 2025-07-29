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

                // 3. Extract tasks from the transcribed text
                var extractedTasks = ExtractTasksFromText(cleanText);
                var summary = GenerateSummary(extractedTasks);

                var analysis = new
                {
                    tasks = extractedTasks,
                    summary = summary,
                    extractedText = cleanText,
                    originalTranscription = transcription,
                    totalTasks = extractedTasks.Count
                };

                return Ok(new
                {
                    success = true,
                    transcription = transcription,
                    analysis = analysis
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

        private List<string> ExtractTasksFromText(string text)
        {
            var tasks = new List<string>();
            var lines = text.Split(new[] { '\n', '\r', '.', '،', ';' }, StringSplitOptions.RemoveEmptyEntries);

            // Keywords that indicate tasks (Egyptian Arabic)
            var taskKeywords = new[] {
                "مهمة", "عمل", "تطوير", "إعداد", "تحديث", "إنشاء", "تصميم", "برمجة", "كتابة", "تحليل", "دراسة", "بحث", "اجتماع", "لقاء", "عرض", "تقرير", "وثيقة", "نظام", "تطبيق", "موقع", "قاعدة بيانات",
                // Egyptian dialect additions
                "شغل", "مشروع", "خطة", "برنامج", "فكرة", "حل", "مشكلة", "حاجة", "شيء", "موضوع", "قضية", "أمر", "مسألة", "نقطة", "فكرة", "مقترح", "اقتراح", "طلب", "حاجة", "ضرورة",
                "تطوير", "تحسين", "تحديث", "تعديل", "تغيير", "إصلاح", "حل", "معالجة", "دراسة", "فحص", "مراجعة", "تحقق", "تأكد", "فحص", "اختبار", "تجربة", "عمل", "تنفيذ", "إنجاز", "إتمام",
                "اجتماع", "لقاء", "مقابلة", "جلسة", "محادثة", "كلام", "حديث", "نقاش", "حوار", "مشاورة", "استشارة", "رأي", "فكرة", "اقتراح", "مقترح", "خطة", "برنامج", "جدول", "موعد",
                "تقرير", "وثيقة", "مستند", "ملف", "ورقة", "كتاب", "مذكرة", "ملاحظة", "تسجيل", "دليل", "دليل", "كتيب", "كتالوج", "قائمة", "جدول", "إحصائيات", "أرقام", "بيانات", "معلومات",
                "نظام", "تطبيق", "برنامج", "سوفت وير", "هارد وير", "تكنولوجيا", "تقنية", "أجهزة", "معدات", "أدوات", "أجهزة", "كمبيوتر", "لابتوب", "موبايل", "هاتف", "إنترنت", "شبكة", "ويب", "موقع", "صفحة",
                "قاعدة بيانات", "داتا", "معلومات", "بيانات", "سجلات", "أرشيف", "ملفات", "مجلدات", "مجلد", "فولدر", "فايل", "ملف", "صورة", "فيديو", "أوديو", "صوت", "موسيقى", "نص", "كتابة", "قراءة",
                "مبيعات", "تسويق", "إعلان", "دعاية", "ترويج", "بيع", "شراء", "تجارة", "عمل", "تجارة", "شركة", "مؤسسة", "منظمة", "جمعية", "مؤسسة", "شركة", "مكتب", "فرع", "قسم", "إدارة",
                "موظف", "عامل", "مدير", "رئيس", "مدير", "مسؤول", "مشرف", "مراقب", "متابع", "منسق", "منظم", "مخطط", "مصمم", "مبرمج", "كاتب", "محلل", "باحث", "مدرس", "أستاذ", "دكتور",
                "عميل", "زبون", "مستخدم", "شخص", "فرد", "مجموعة", "فريق", "طاقم", "عائلة", "أصدقاء", "زملاء", "شركاء", "شريك", "شريكة", "صديق", "صديقة", "زميل", "زميلة", "جار", "جارة",
                "ميزانية", "مال", "فلوس", "دولار", "جنيه", "ريال", "يورو", "تكلفة", "سعر", "ثمن", "قيمة", "مبلغ", "مصاريف", "مصروفات", "إيرادات", "أرباح", "خسائر", "ربح", "خسارة", "كسب",
                "وقت", "زمن", "مدة", "فترة", "أسبوع", "شهر", "سنة", "يوم", "ساعة", "دقيقة", "ثانية", "صباح", "مساء", "ليل", "نهار", "صباح", "ظهر", "عصر", "مغرب", "عشاء", "فجر"
            };
            var deadlineKeywords = new[] {
                "الموعد النهائي", "آخر موعد", "نهاية", "الأسبوع", "الشهر", "اليوم", "غداً", "بعد غد", "الخميس", "الجمعة", "السبت", "الأحد", "الاثنين", "الثلاثاء", "الأربعاء",
                // Egyptian dialect additions
                "غداً", "بكرا", "بعد بكرة", "بعد غد", "بعد بكرة", "النهاردة", "دلوقتي", "الحين", "الوقت ده", "الوقت دا", "الوقت ده", "الوقت دا", "الوقت ده", "الوقت دا",
                "الخميس", "الجمعة", "السبت", "الأحد", "الاثنين", "الثلاثاء", "الأربعاء", "يوم الخميس", "يوم الجمعة", "يوم السبت", "يوم الأحد", "يوم الاثنين", "يوم الثلاثاء", "يوم الأربعاء",
                "الأسبوع", "الأسبوع الجاي", "الأسبوع ده", "الأسبوع دا", "الأسبوع ده", "الأسبوع دا", "الأسبوع ده", "الأسبوع دا", "الأسبوع ده", "الأسبوع دا", "الأسبوع ده", "الأسبوع دا",
                "الشهر", "الشهر الجاي", "الشهر ده", "الشهر دا", "الشهر ده", "الشهر دا", "الشهر ده", "الشهر دا", "الشهر ده", "الشهر دا", "الشهر ده", "الشهر دا",
                "السنة", "السنة الجاية", "السنة ده", "السنة دا", "السنة ده", "السنة دا", "السنة ده", "السنة دا", "السنة ده", "السنة دا", "السنة ده", "السنة دا",
                "الموعد", "الموعد النهائي", "آخر موعد", "آخر تاريخ", "آخر يوم", "آخر أسبوع", "آخر شهر", "آخر سنة", "آخر وقت", "آخر فرصة", "آخر مرة", "آخر مرة",
                "قبل", "بعد", "خلال", "في", "على", "حول", "حوالي", "تقريباً", "تقريبا", "تقريباً", "تقريبا", "تقريباً", "تقريبا", "تقريباً", "تقريبا",
                "الساعة", "الوقت", "الزمن", "المدة", "الفترة", "المرة", "المرة دي", "المرة دي", "المرة دي", "المرة دي", "المرة دي", "المرة دي", "المرة دي", "المرة دي",
                "الصبح", "الظهر", "العصر", "المغرب", "العشاء", "الليل", "النهار", "النهاردة", "النهاردة", "النهاردة", "النهاردة", "النهاردة", "النهاردة", "النهاردة",
                "النهاردة", "النهاردة", "النهاردة", "النهاردة", "النهاردة", "النهاردة", "النهاردة", "النهاردة", "النهاردة", "النهاردة", "النهاردة", "النهاردة"
            };

            foreach (var line in lines)
            {
                var cleanLine = line.Trim();
                if (string.IsNullOrWhiteSpace(cleanLine)) continue;

                // Check if line contains task keywords
                var hasTaskKeyword = taskKeywords.Any(keyword => cleanLine.Contains(keyword, StringComparison.OrdinalIgnoreCase));
                var hasDeadlineKeyword = deadlineKeywords.Any(keyword => cleanLine.Contains(keyword, StringComparison.OrdinalIgnoreCase));

                if (hasTaskKeyword || hasDeadlineKeyword)
                {
                    // Extract deadline if present
                    var deadline = ExtractDeadline(cleanLine);
                    var task = cleanLine;

                    if (!string.IsNullOrEmpty(deadline))
                    {
                        task = $"{cleanLine} - الموعد النهائي: {deadline}";
                    }

                    if (!tasks.Contains(task))
                    {
                        tasks.Add(task);
                    }
                }
            }

            // If no specific tasks found, try to extract general tasks
            if (tasks.Count == 0)
            {
                var sentences = text.Split(new[] { '.', '،', ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var sentence in sentences.Take(5)) // Take first 5 sentences
                {
                    var cleanSentence = sentence.Trim();
                    if (cleanSentence.Length > 10 && cleanSentence.Length < 200)
                    {
                        tasks.Add(cleanSentence);
                    }
                }
            }

            return tasks;
        }

        private string ExtractDeadline(string text)
        {
            var deadlineKeywords = new[] {
                "الموعد النهائي", "آخر موعد", "نهاية", "الأسبوع", "الشهر", "اليوم", "غداً", "بعد غد",
                // Egyptian dialect additions
                "غداً", "بكرا", "بعد بكرة", "بعد غد", "بعد بكرة", "النهاردة", "دلوقتي", "الحين", "الوقت ده", "الوقت دا",
                "الموعد", "الموعد النهائي", "آخر موعد", "آخر تاريخ", "آخر يوم", "آخر أسبوع", "آخر شهر", "آخر سنة", "آخر وقت", "آخر فرصة", "آخر مرة",
                "قبل", "بعد", "خلال", "في", "على", "حول", "حوالي", "تقريباً", "تقريبا",
                "الساعة", "الوقت", "الزمن", "المدة", "الفترة", "المرة", "المرة دي",
                "الصبح", "الظهر", "العصر", "المغرب", "العشاء", "الليل", "النهار", "النهاردة"
            };
            var days = new[] {
                "الخميس", "الجمعة", "السبت", "الأحد", "الاثنين", "الثلاثاء", "الأربعاء",
                "يوم الخميس", "يوم الجمعة", "يوم السبت", "يوم الأحد", "يوم الاثنين", "يوم الثلاثاء", "يوم الأربعاء"
            };

            foreach (var keyword in deadlineKeywords)
            {
                if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    var index = text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
                    var afterKeyword = text.Substring(index + keyword.Length).Trim();
                    var words = afterKeyword.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length > 0)
                    {
                        return $"{keyword} {words[0]}";
                    }
                }
            }

            foreach (var day in days)
            {
                if (text.Contains(day, StringComparison.OrdinalIgnoreCase))
                {
                    return day;
                }
            }

            return "";
        }

        private string GenerateSummary(List<string> tasks)
        {
            if (tasks.Count == 0)
                return "لم يتم العثور على مهام محددة في النص.";

            var taskCount = tasks.Count;
            var summary = $"تم تحديد {taskCount} مهام رئيسية";

            if (tasks.Any(t => t.Contains("الموعد النهائي")))
            {
                summary += " مع مواعيد نهائية محددة";
            }

            summary += ".";
            return summary;
        }
    }
}