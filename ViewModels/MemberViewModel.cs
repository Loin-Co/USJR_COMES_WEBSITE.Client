namespace USJR_COMES_WEBSITE.ViewModels;

public class MemberRestrictionViewModel
{
    public bool IsMember { get; set; }
    public bool IsOfficer { get; set; }
    public bool IsProfessor { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsAdviser { get; set; }
    public bool IsDepartmentChairman { get; set; }
}

public class MembershipRecordViewModel
{
    public int Id { get; set; }
    public bool IsMember { get; set; }
    public bool HasReceipt { get; set; }
    public DateTime? PromotedAt { get; set; }
    public string? PromotedBySchoolId { get; set; }
    public string? PromotedByName { get; set; }
}

public class MemberViewModel
{
    public string SchoolId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string YearLevel { get; set; } = string.Empty;
    public string SchoolEmail { get; set; } = string.Empty;
    public string AccountRole { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsOfficerOfYear { get; set; }
    public MemberRestrictionViewModel? Restriction { get; set; }

    // Populated when fetching users per academic year
    public MembershipRecordViewModel? MembershipRecord { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsYearMember => MembershipRecord?.IsMember ?? false;
    // Officers of the year bypass the non-member restriction
    public bool HasYearAccess => IsYearMember || IsOfficerOfYear;
    public string JoinedYear => CreatedAt.Year.ToString();

    public string Initials
    {
        get
        {
            var f = string.IsNullOrEmpty(FirstName) ? "" : FirstName[0].ToString();
            var l = string.IsNullOrEmpty(LastName) ? "" : LastName[0].ToString();
            return (f + l).ToUpperInvariant();
        }
    }
}

public class PromoteMemberRequest
{
    public string SchoolId { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public string? ReceiptData { get; set; }
    public string? ReceiptMimeType { get; set; }
    public string? PromotedBySchoolId { get; set; }
}
