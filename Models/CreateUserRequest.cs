namespace USJR_COMES_WEBSITE.Models;

public class CreateUserRequest
{
    public string SchoolId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly Birthdate { get; set; }
    public string YearLevel { get; set; } = string.Empty;
    public string SchoolEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool TermsAccepted { get; set; }
}
