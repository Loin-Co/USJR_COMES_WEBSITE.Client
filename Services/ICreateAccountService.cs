using USJR_COMES_WEBSITE.Models;

namespace USJR_COMES_WEBSITE.Services;

public interface ICreateAccountService
{
    Task<(bool Success, string? Error)> RegisterAsync(CreateUserRequest request);
}
