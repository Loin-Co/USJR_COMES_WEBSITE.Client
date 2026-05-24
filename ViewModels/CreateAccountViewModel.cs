using USJR_COMES_WEBSITE.Models;
using USJR_COMES_WEBSITE.Services;

namespace USJR_COMES_WEBSITE.ViewModels;

public class CreateAccountViewModel
{
    private readonly ICreateAccountService _createAccountService;

    // Form fields
    public string SchoolId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly Birthdate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public string YearLevel { get; set; } = string.Empty;
    public string SchoolEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public bool TermsAccepted { get; set; }

    // Per-field error messages
    public string? SchoolIdError { get; set; }
    public string? FirstNameError { get; set; }
    public string? LastNameError { get; set; }
    public string? BirthdateError { get; set; }
    public string? YearLevelError { get; set; }
    public string? SchoolEmailError { get; set; }
    public string? PasswordError { get; set; }
    public string? ConfirmPasswordError { get; set; }
    public string? TermsError { get; set; }

    // Global submission state
    public string? ErrorMessage { get; set; }
    public bool IsLoading { get; set; }
    public bool IsSuccess { get; set; }

    public CreateAccountViewModel(ICreateAccountService createAccountService)
    {
        _createAccountService = createAccountService;
    }

    public async Task ExecuteCreateAccountAsync()
    {
        ErrorMessage = null;
        IsSuccess = false;

        if (!Validate())
            return;

        IsLoading = true;

        var request = new CreateUserRequest
        {
            SchoolId      = SchoolId,
            FirstName     = FirstName,
            LastName      = LastName,
            Birthdate     = Birthdate,
            YearLevel     = YearLevel,
            SchoolEmail   = SchoolEmail,
            Password      = Password,
            TermsAccepted = TermsAccepted
        };

        var (success, error) = await _createAccountService.RegisterAsync(request);

        IsLoading = false;

        if (success)
        {
            IsSuccess = true;
        }
        else if (error is not null)
        {
            // Use fast string search without allocations
            var lowerError = error;
            if (lowerError.Contains("school ID", StringComparison.OrdinalIgnoreCase))
                SchoolIdError = error;
            else if (lowerError.Contains("email", StringComparison.OrdinalIgnoreCase))
                SchoolEmailError = error;
            else
                ErrorMessage = error;
        }
    }

    private bool Validate()
    {
        ClearFieldErrors();
        var isValid = true;

        // Validate required fields more efficiently
        if (string.IsNullOrWhiteSpace(SchoolId))
        {
            SchoolIdError = "School ID is required.";
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(FirstName))
        {
            FirstNameError = "First name is required.";
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(LastName))
        {
            LastNameError = "Last name is required.";
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(YearLevel))
        {
            YearLevelError = "Year level is required.";
            isValid = false;
        }

        // Check birthdate early to fail fast
        if (Birthdate >= DateOnly.FromDateTime(DateTime.Today))
        {
            BirthdateError = "Please enter a valid birthdate.";
            isValid = false;
        }

        // Validate email
        if (string.IsNullOrWhiteSpace(SchoolEmail))
        {
            SchoolEmailError = "School email is required.";
            isValid = false;
        }
        else if (!SchoolEmail.EndsWith("@usjr.edu.ph", StringComparison.OrdinalIgnoreCase))
        {
            SchoolEmailError = "Email must be a valid USJR email (@usjr.edu.ph).";
            isValid = false;
        }

        // Validate passwords
        if (string.IsNullOrWhiteSpace(Password))
        {
            PasswordError = "Password is required.";
            isValid = false;
        }
        else if (string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ConfirmPasswordError = "Please confirm your password.";
            isValid = false;
        }
        else if (Password != ConfirmPassword)
        {
            ConfirmPasswordError = "Passwords do not match.";
            isValid = false;
        }

        // Check terms last
        if (!TermsAccepted)
        {
            TermsError = "You must accept the Terms and Conditions.";
            isValid = false;
        }

        return isValid;
    }

    private void ClearFieldErrors()
    {
        SchoolIdError       = null;
        FirstNameError      = null;
        LastNameError       = null;
        BirthdateError      = null;
        YearLevelError      = null;
        SchoolEmailError    = null;
        PasswordError       = null;
        ConfirmPasswordError = null;
        TermsError          = null;
    }
}
