namespace USJR_COMES_WEBSITE.ViewModels;

public class AboutUsContentViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "ABOUT US";
    public string AboutUsText { get; set; } = string.Empty;
    public string VisionText { get; set; } = string.Empty;
    public string MissionText { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public int? PresidentId { get; set; }
    public OfficerViewModel? President { get; set; }
    public string PresidentPersonalMessage { get; set; } = string.Empty;
    public string BannerImageUrl { get; set; } = string.Empty;
    // Used when uploading a new banner image (base64 data URL)
    public string? BannerImageBase64 { get; set; }
}
