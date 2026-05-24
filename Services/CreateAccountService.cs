using System.Net;
using System.Net.Http.Json;
using USJR_COMES_WEBSITE.Models;

namespace USJR_COMES_WEBSITE.Services;

public class CreateAccountService : ICreateAccountService
{
    private readonly HttpClient _httpClient;

    public CreateAccountService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(CreateUserRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/users/register", request);

            if (response.IsSuccessStatusCode)
                return (true, null);

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                var body = await response.Content.ReadAsStringAsync();
                // Strip JSON quotes if the body is a JSON string ("message" → message)
                var message = body.Trim();
                if (message.StartsWith('"') && message.EndsWith('"'))
                    message = message[1..^1];
                return (false, message);
            }

            return (false, "An error occurred while creating your account.");
        }
        catch (TaskCanceledException)
        {
            return (false, "Request timed out. Please try again.");
        }
        catch (HttpRequestException)
        {
            return (false, "Cannot reach the server. Please check your connection.");
        }
        catch
        {
            return (false, "An unexpected error occurred. Please try again.");
        }
    }
}
