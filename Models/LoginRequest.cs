namespace USJR_COMES_WEBSITE.Models;

public class LoginRequest
{
    public string SchoolId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class PasswordSetupRequest
{
    public string SchoolId { get; set; } = string.Empty;
}

public class CompletePasswordSetupRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
