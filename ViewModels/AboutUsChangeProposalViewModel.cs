namespace USJR_COMES_WEBSITE.ViewModels;

public class AboutUsChangeProposalViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "ABOUT US";
    public string AboutUsText { get; set; } = string.Empty;
    public string VisionText { get; set; } = string.Empty;
    public string MissionText { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public int? PresidentId { get; set; }
    public string PresidentPersonalMessage { get; set; } = string.Empty;
    public bool HasBannerImage { get; set; }
    public string? BannerImageMimeType { get; set; }
    public string ProposedBySchoolId { get; set; } = string.Empty;
    public string? ProposerName { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    public string? PresidentName { get; set; }
    public string? PresidentImage { get; set; }
}

public class AboutUsChangeProposalRequest
{
    public string Title { get; set; } = "ABOUT US";
    public string AboutUsText { get; set; } = string.Empty;
    public string VisionText { get; set; } = string.Empty;
    public string MissionText { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public int? PresidentId { get; set; }
    public string PresidentPersonalMessage { get; set; } = string.Empty;
    public string ProposedBySchoolId { get; set; } = string.Empty;
    public string? BannerImageBase64 { get; set; }
    public string? BannerImageMimeType { get; set; }
}

public class AboutUsChangeReviewRequest
{
    public string? ReviewedBySchoolId { get; set; }
    public string? RejectionReason { get; set; }
}
