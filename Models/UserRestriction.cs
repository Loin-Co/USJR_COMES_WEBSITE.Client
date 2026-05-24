namespace USJR_COMES_WEBSITE.Models;

public class UserRestriction
{
    public string SchoolId { get; set; } = string.Empty;
    public bool IsMember { get; set; }
    public bool IsOfficer { get; set; }
    public bool IsProfessor { get; set; }
    public bool IsAdmin { get; set; }
}
