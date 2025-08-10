//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using platform_backend.DTOs;
//using platform_backend.Models;
//using platform_backend.Services;
//using System.Security.Claims;

//namespace platform_backend.Controllers
//{
//    [Route("api/auth")]
//    [ApiController]
//    public class AuthController : ControllerBase
//    {
//        private readonly GoogleAuthService _googleAuthService;
//        private readonly IGoogleTokenValidator _googleTokenValidator;
//        private readonly ITokenService _tokenService;
//        private readonly ILogger<AuthController> _logger;

//        public AuthController(
//            GoogleAuthService googleAuthService,
//            IGoogleTokenValidator googleTokenValidator,
//            ITokenService tokenService,
//            ILogger<AuthController> logger)
//        {
//            _googleAuthService = googleAuthService;
//            _googleTokenValidator = googleTokenValidator;
//            _tokenService = tokenService;
//            _logger = logger;
//        }

//        /// <summary>
//        /// بدء عملية المصادقة مع Google - يعيد رابط المصادقة
//        /// </summary>
//        [HttpGet("google")]
//        public IActionResult InitiateGoogleAuth([FromQuery] string? returnUrl = null)
//        {
//            try
//            {
//                // إنشاء state parameter للحماية من CSRF
//                var state = Guid.NewGuid().ToString();
                
//                // حفظ state في session
//                HttpContext.Session.SetString("GoogleAuthState", state);
//                if (!string.IsNullOrEmpty(returnUrl))
//                {
//                    HttpContext.Session.SetString("ReturnUrl", returnUrl);
//                }

//                var authUrl = _googleAuthService.GetGoogleAuthUrl(state);

//                // للاستخدام مع popup window
//                if (Request.Headers.ContainsKey("X-Requested-With"))
//                {
//                    return Ok(new { authUrl });
//                }

//                // إعادة توجيه مباشرة
//                return Redirect(authUrl);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "خطأ في بدء المصادقة مع Google");
//                return BadRequest(new { error = "فشل في بدء المصادقة مع Google" });
//            }
//        }

//        /// <summary>
//        /// معالجة callback من Google OAuth
//        /// </summary>
//        [HttpGet("google/callback")]
//        public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string state, [FromQuery] string? error = null)
//        {
//            try
//            {
//                // التحقق من وجود خطأ من Google
//                if (!string.IsNullOrEmpty(error))
//                {
//                    _logger.LogWarning("خطأ من Google OAuth: {Error}", error);
//                    return BadRequest(new { error = $"خطأ من Google: {error}" });
//                }

//                // التحقق من وجود الكود
//                if (string.IsNullOrEmpty(code))
//                {
//                    return BadRequest(new { error = "لم يتم استلام كود المصادقة" });
//                }

//                // التحقق من state parameter (CSRF protection)
//                var expectedState = HttpContext.Session.GetString("GoogleAuthState");
//                if (string.IsNullOrEmpty(expectedState) || expectedState != state)
//                {
//                    _logger.LogWarning("State parameter غير صحيح. Expected: {Expected}, Received: {Received}", expectedState, state);
//                    return BadRequest(new { error = "طلب غير صالح - فشل التحقق من الأمان" });
//                }

//                // تبديل الكود للحصول على التوكن
//                var tokenResponse = await _googleAuthService.ExchangeCodeForTokenAsync(code);
//                if (tokenResponse == null)
//                {
//                    return BadRequest(new { error = "فشل في الحصول على التوكن من Google" });
//                }

//                // التحقق من صحة ID Token
//                var payload = await _googleAuthService.ValidateIdTokenAsync(tokenResponse.IdToken);
//                if (payload == null)
//                {
//                    return BadRequest(new { error = "فشل في التحقق من هوية المستخدم" });
//                }

//                // إنشاء أو تحديث المستخدم في قاعدة البيانات
//                var user = await _googleAuthService.CreateOrUpdateUserAsync(payload);

//                // إنشاء Access Token و Refresh Token
//                var accessToken = _tokenService.GenerateAccessToken(user);
//                var refreshToken = await _tokenService.GenerateRefreshTokenAsync(
//                    user, 
//                    Request.Headers.UserAgent.ToString(), 
//                    HttpContext.Connection.RemoteIpAddress?.ToString()
//                );

//                var authResponse = new AuthResponse
//                {
//                    AccessToken = accessToken,
//                    RefreshToken = refreshToken.Token,
//                    ExpiresIn = 15 * 60, // 15 minutes in seconds
//                    User = new UserInfo
//                    {
//                        Id = user.Id,
//                        Email = user.Email,
//                        DisplayName = user.DisplayName,
//                        PhotoUrl = user.PhotoUrl,
//                        EmailVerified = user.EmailVerified
//                    }
//                };

//                // إذا كان الطلب من popup window، نعيد صفحة HTML تقوم بإرسال النتيجة للنافذة الأب
//                if (Request.Headers.ContainsKey("X-Requested-With") || Request.Query.ContainsKey("popup"))
//                {
//                    var html = GeneratePopupCallbackHtml(authResponse);
//                    return Content(html, "text/html");
//                }

//                // للاستخدام العادي، نعيد JSON
//                return Ok(authResponse);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "خطأ في معالجة Google callback");
//                return BadRequest(new { error = "فشل في إتمام المصادقة" });
//            }
//        }

//        /// <summary>
//        /// تسجيل الدخول باستخدام Google ID Token (الطريقة الرئيسية)
//        /// </summary>
//        [HttpPost("google")]
//        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
//        {
//            try
//            {
//                if (string.IsNullOrEmpty(request.IdToken))
//                {
//                    return BadRequest(new { error = "ID Token is required" });
//                }

//                // التحقق من صحة Google ID Token
//                var payload = await _googleTokenValidator.ValidateAsync(request.IdToken);
//                if (payload == null)
//                {
//                    return Unauthorized(new { error = "Invalid Google token" });
//                }

//                // التحقق من أن البريد الإلكتروني مُتحقق منه
//                if (!_googleTokenValidator.IsEmailVerified(payload))
//                {
//                    return BadRequest(new { error = "Email not verified with Google" });
//                }

//                // إنشاء أو تحديث المستخدم
//                var user = await _googleAuthService.CreateOrUpdateUserAsync(payload);

//                // إنشاء Access Token و Refresh Token
//                var accessToken = _tokenService.GenerateAccessToken(user);
//                var refreshToken = await _tokenService.GenerateRefreshTokenAsync(
//                    user, 
//                    Request.Headers.UserAgent.ToString(), 
//                    HttpContext.Connection.RemoteIpAddress?.ToString()
//                );

//                var response = new AuthResponse
//                {
//                    AccessToken = accessToken,
//                    RefreshToken = refreshToken.Token,
//                    ExpiresIn = 15 * 60, // 15 minutes in seconds
//                    User = new UserInfo
//                    {
//                        Id = user.Id,
//                        Email = user.Email,
//                        DisplayName = user.DisplayName,
//                        PhotoUrl = user.PhotoUrl,
//                        EmailVerified = user.EmailVerified
//                    }
//                };

//                return Ok(response);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error during Google authentication");
//                return Unauthorized(new { error = "Authentication failed", detail = ex.Message });
//            }
//        }

//        /// <summary>
//        /// تجديد Access Token باستخدام Refresh Token
//        /// </summary>
//        [HttpPost("refresh")]
//        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
//        {
//            try
//            {
//                if (string.IsNullOrEmpty(request.RefreshToken))
//                {
//                    return BadRequest(new { error = "Refresh token is required" });
//                }

//                var response = await _tokenService.RefreshTokenAsync(
//                    request.RefreshToken,
//                    Request.Headers.UserAgent.ToString(),
//                    HttpContext.Connection.RemoteIpAddress?.ToString()
//                );

//                if (response == null)
//                {
//                    return Unauthorized(new { error = "Invalid or expired refresh token" });
//                }

//                return Ok(response);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error refreshing token");
//                return Unauthorized(new { error = "Token refresh failed" });
//            }
//        }

//        /// <summary>
//        /// تسجيل الخروج وإلغاء Refresh Token
//        /// </summary>
//        [HttpPost("logout")]
//        public async Task<IActionResult> Logout([FromBody] LogoutRequest? request = null)
//        {
//            try
//            {
//                if (!string.IsNullOrEmpty(request?.RefreshToken))
//                {
//                    await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);
//                }

//                // إذا كان المستخدم مسجل دخول، يمكننا إلغاء جميع التوكنات
//                if (User.Identity?.IsAuthenticated == true)
//                {
//                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//                    if (int.TryParse(userIdClaim, out var userId))
//                    {
//                        await _tokenService.RevokeAllUserTokensAsync(userId);
//                    }
//                }

//                return Ok(new { message = "Logged out successfully" });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error during logout");
//                return Ok(new { message = "Logged out successfully" }); // Always return success for logout
//            }
//        }

//        /// <summary>
//        /// الحصول على معلومات المستخدم الحالي
//        /// </summary>
//        [HttpGet("me")]
//        [Authorize]
//        public IActionResult GetCurrentUser()
//        {
//            try
//            {
//                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//                var email = User.FindFirst(ClaimTypes.Email)?.Value;
//                var name = User.FindFirst(ClaimTypes.Name)?.Value;
//                var picture = User.FindFirst("picture")?.Value;

//                return Ok(new
//                {
//                    id = userIdClaim,
//                    email,
//                    name,
//                    picture
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "خطأ في الحصول على معلومات المستخدم");
//                return BadRequest(new { error = "فشل في الحصول على معلومات المستخدم" });
//            }
//        }

//        /// <summary>
//        /// إنشاء HTML للـ popup callback
//        /// </summary>
//        private string GeneratePopupCallbackHtml(AuthResponse authResponse)
//        {
//            var json = System.Text.Json.JsonSerializer.Serialize(authResponse);
//            return $@"
//<!DOCTYPE html>
//<html>
//<head>
//    <title>تسجيل الدخول</title>
//</head>
//<body>
//    <script>
//        // إرسال النتيجة للنافذة الأب وإغلاق النافذة
//        if (window.opener) {{
//            window.opener.postMessage({{
//                type: 'GOOGLE_AUTH_SUCCESS',
//                data: {json}
//            }}, '*');
//            window.close();
//        }} else {{
//            // إذا لم تكن نافذة popup، نعيد توجيه للصفحة الرئيسية
//            window.location.href = '/';
//        }}
//    </script>
//    <p>جاري تسجيل الدخول...</p>
//</body>
//</html>";
//        }

//        [HttpOptions("google-login")]
//        public IActionResult GoogleLoginOptions()
//        {
//            return Ok();
//        }

//        [HttpOptions("google")]
//        public IActionResult GoogleOptions()
//        {
//            return Ok();
//        }

//        [HttpOptions("google/callback")]
//        public IActionResult GoogleCallbackOptions()
//        {
//            return Ok();
//        }
//    }
//}
