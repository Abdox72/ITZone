using Google.Apis.Auth;

namespace platform_backend.Services
{
    /// <summary>
    /// واجهة خدمة التحقق من Google ID Tokens
    /// </summary>
    public interface IGoogleTokenValidator
    {
        /// <summary>
        /// التحقق من صحة Google ID Token
        /// </summary>
        /// <param name="idToken">Google ID Token</param>
        /// <returns>معلومات المستخدم من Google أو null إذا كان التوكن غير صالح</returns>
        Task<GoogleJsonWebSignature.Payload?> ValidateAsync(string idToken);

        /// <summary>
        /// التحقق من أن البريد الإلكتروني مُتحقق منه
        /// </summary>
        /// <param name="payload">معلومات المستخدم من Google</param>
        /// <returns>true إذا كان البريد مُتحقق منه</returns>
        bool IsEmailVerified(GoogleJsonWebSignature.Payload payload);
    }
}