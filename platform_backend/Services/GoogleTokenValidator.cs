using Google.Apis.Auth;

namespace platform_backend.Services
{
    /// <summary>
    /// خدمة التحقق من Google ID Tokens
    /// </summary>
    public class GoogleTokenValidator : IGoogleTokenValidator
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleTokenValidator> _logger;

        public GoogleTokenValidator(
            IConfiguration configuration,
            ILogger<GoogleTokenValidator> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// التحقق من صحة Google ID Token
        /// </summary>
        public async Task<GoogleJsonWebSignature.Payload?> ValidateAsync(string idToken)
        {
            try
            {
                var clientId = _configuration["Authentication:Google:ClientId"] ?? 
                              throw new InvalidOperationException("Google ClientId not configured");

                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                
                // Additional validation
                if (payload == null)
                {
                    _logger.LogWarning("Google token validation returned null payload");
                    return null;
                }

                // Verify issuer
                if (!string.Equals(payload.Issuer, "https://accounts.google.com", StringComparison.Ordinal) && 
                    !string.Equals(payload.Issuer, "accounts.google.com", StringComparison.Ordinal))
                {
                    _logger.LogWarning("Invalid token issuer: {Issuer}", payload.Issuer);
                    return null;
                }

                // Verify audience
                if (payload.Audience != clientId)
                {
                    _logger.LogWarning("Invalid token audience: {Audience}", payload.Audience);
                    return null;
                }

                // Verify expiration
                if (payload.ExpirationTimeSeconds < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    _logger.LogWarning("Token has expired");
                    return null;
                }

                // Verify email is present
                if (string.IsNullOrEmpty(payload.Email))
                {
                    _logger.LogWarning("Token does not contain email");
                    return null;
                }

                return payload;
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning(ex, "Invalid JWT token");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Google token");
                return null;
            }
        }

        /// <summary>
        /// التحقق من أن البريد الإلكتروني مُتحقق منه
        /// </summary>
        public bool IsEmailVerified(GoogleJsonWebSignature.Payload payload)
        {
            return payload.EmailVerified;
        }
    }
}