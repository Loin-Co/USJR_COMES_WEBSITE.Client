namespace USJR_COMES_WEBSITE.ViewModels;

public class OfficerViewModel
{
    public int Id { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Quote { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? SchoolId { get; set; }
}
