using USJR_COMES_WEBSITE.Models;

namespace USJR_COMES_WEBSITE.Services;

public interface IUserServices
{
    Task<(User? User, string? Error, bool RequiresPasswordSetup)> LoginAsync(LoginRequest request);
    Task<bool> UpdateAvatarAsync(string schoolId, string imageData, string mimeType);
    Task<bool> RequestPasswordSetupAsync(string schoolId);
    Task<bool> RequestPasswordResetAsync(string email);
    Task<(bool Success, string? Error)> CompletePasswordSetupAsync(string token, string newPassword);
}
