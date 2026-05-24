using Microsoft.AspNetCore.Components;
using USJR_COMES_WEBSITE.Models;
using USJR_COMES_WEBSITE.Services;

namespace USJR_COMES_WEBSITE.ViewModels;

public class LoginViewModel
{
    private readonly IUserServices _userServices;
    private readonly NavigationManager _navigationManager;
    private readonly UserStateService _userStateService;

    public string SchoolId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public bool IsLoading { get; set; }
    public bool IsSuccess { get; set; }
    public bool ShowNoPasswordPopup { get; set; }

    // Forgot password form state
    public string ForgotPasswordEmail { get; set; } = string.Empty;
    public bool ShowForgotPasswordModal { get; set; }
    public bool ForgotPasswordSent { get; set; }
    public string? ForgotPasswordError { get; set; }
    public bool ForgotPasswordSending { get; set; }

    public LoginViewModel(IUserServices userServices, NavigationManager navigationManager, UserStateService userStateService)
    {
        _userServices = userServices;
        _navigationManager = navigationManager;
        _userStateService = userStateService;
    }

    public async Task ExecuteLoginAsync()
    {
        ErrorMessage = null;
        IsLoading = true;
        IsSuccess = false;
        ShowNoPasswordPopup = false;

        var loginRequest = new LoginRequest
        {
            SchoolId = SchoolId,
            Password = Password
        };

        var (user, errorMessage, requiresPasswordSetup) = await _userServices.LoginAsync(loginRequest);

        IsLoading = false;

        if (requiresPasswordSetup)
        {
            // Trigger setup email automatically, then show popup
            await _userServices.RequestPasswordSetupAsync(SchoolId);
            ShowNoPasswordPopup = true;
            return;
        }

        if (user is not null)
        {
            if (user.Status == "Invalid")
            {
                ErrorMessage = "Invalid User";
            }
            else
            {
                _userStateService.SetUser(user);
                IsSuccess = true;
            }
        }
        else
        {
            ErrorMessage = errorMessage;
        }
    }

    public void OpenForgotPassword()
    {
        ForgotPasswordEmail = string.Empty;
        ForgotPasswordSent = false;
        ForgotPasswordError = null;
        ShowForgotPasswordModal = true;
    }

    public async Task SendForgotPasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(ForgotPasswordEmail))
        {
            ForgotPasswordError = "Please enter your school email.";
            return;
        }

        ForgotPasswordSending = true;
        ForgotPasswordError = null;

        await _userServices.RequestPasswordResetAsync(ForgotPasswordEmail.Trim());

        ForgotPasswordSending = false;
        // Always show success — email is sent regardless (reset link or not-a-member notice)
        ForgotPasswordSent = true;
    }
}
