namespace USJR_COMES_WEBSITE.ViewModels;

public class UserLookupViewModel
{
    public string SchoolId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string SchoolEmail { get; set; } = string.Empty;
    public string AccountRole { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}".Trim();
}
