namespace USJR_COMES_WEBSITE.Models;

public class User
{
    public string SchoolId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string SchoolEmail { get; set; } = string.Empty;
    public string AccountRole { get; set; } = string.Empty;
    public UserRestriction? Restriction { get; set; }
    public string? ProfileImageData { get; set; }
    public string? ProfileImageMimeType { get; set; }

    public string Status => AccountRole;
}
