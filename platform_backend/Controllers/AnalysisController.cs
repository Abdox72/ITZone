using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace platform_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalysisController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public AnalysisController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeMeetingText([FromBody] AnalysisRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required.");

                if (string.IsNullOrWhiteSpace(request.Text))
                    return BadRequest("Text is required.");

                // Clean the text by removing problematic characters
                var cleanText = request.Text?.Replace("\n", " ").Replace("\r", " ").Trim();
                if (string.IsNullOrWhiteSpace(cleanText))
                    return BadRequest("Text is required after cleaning.");

                // Extract tasks and deadlines from the provided text
                var extractedTasks = ExtractTasksFromText(cleanText);
                var summary = GenerateSummary(extractedTasks);

                var analysis = new
                {
                    tasks = extractedTasks,
                    summary = summary,
                    extractedText = cleanText,
                    originalText = request.Text,
                    totalTasks = extractedTasks.Count
                };

                return Ok(new { success = true, data = analysis });

                // Uncomment the code below when the external API is working
                /*
                string apiUrl = "https://api-inference.huggingface.co/models/mistralai/Mistral-7B-Instruct";
                string apiKey = "APi Key";

                string prompt = $"Extract all tasks and deadlines from the following Arabic meeting transcript and summarize them in bullet points:\n\n{request.Text}";

                var body = new
                {
                    inputs = prompt,
                    parameters = new
                    {
                        temperature = 0.3,
                        max_new_tokens = 300
                    }
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var response = await _httpClient.PostAsync(apiUrl, jsonContent);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"External API error: {error}");
                }

                var result = await response.Content.ReadAsStringAsync();
                return Ok(new { success = true, data = result });
                */
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
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

    public class AnalysisRequest
    {
        public string Text { get; set; } = string.Empty;
    }
}