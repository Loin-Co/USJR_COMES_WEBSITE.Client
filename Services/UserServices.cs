using System.Net;
using System.Net.Http.Json;
using USJR_COMES_WEBSITE.APIs;
using USJR_COMES_WEBSITE.Models;

namespace USJR_COMES_WEBSITE.Services;

public class UserServices : IUserServices
{
    private readonly HttpClient _httpClient;

    public UserServices(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(User? User, string? Error, bool RequiresPasswordSetup)> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.Users.Login, request);

            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<User>();
                return (user, null, false);
            }

            // 428 = student has no password yet
            if ((int)response.StatusCode == 428)
                return (null, null, true);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return (null, "Student ID not found.", false);

            return (null, "Invalid password.", false);
        }
        catch
        {
            return (null, "Server error: Unable to log in.", false);
        }
    }

    public async Task<bool> RequestPasswordSetupAsync(string schoolId)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                ApiEndpoints.Users.RequestPasswordSetup,
                new { schoolId });
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> RequestPasswordResetAsync(string email)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                ApiEndpoints.Users.RequestPasswordReset,
                new { email });
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<(bool Success, string? Error)> CompletePasswordSetupAsync(string token, string newPassword)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                ApiEndpoints.Users.CompletePasswordSetup,
                new { token, newPassword });

            if (response.IsSuccessStatusCode) return (true, null);

            var body = await response.Content.ReadAsStringAsync();
            return (false, body.Length < 200 ? body : "Invalid or expired link.");
        }
        catch
        {
            return (false, "Server error. Please try again.");
        }
    }

    public async Task<bool> UpdateAvatarAsync(string schoolId, string imageData, string mimeType)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/Users/{schoolId}/avatar",
                new { imageData, mimeType });
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}
