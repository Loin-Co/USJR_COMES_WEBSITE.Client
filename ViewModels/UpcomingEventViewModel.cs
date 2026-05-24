namespace USJR_COMES_WEBSITE.ViewModels;

public class UpcomingEventViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public string Organizer { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GoogleFormLink { get; set; } = string.Empty;
    public int AttendeesCount { get; set; }
    public int Likes { get; set; }
    public int Comments { get; set; }
    public bool IsLiked { get; set; }

    public string Status { get; set; } = "Unpublished";
    public string ImageUrl { get; set; } = string.Empty;

    // Approval workflow
    public string ApprovalStatus { get; set; } = "None";
    public string? ApprovedBy { get; set; }
    public string? RejectionReason { get; set; }
    public string CreatedByRole { get; set; } = "Officer";
    public string? CreatedBy { get; set; }

    public bool PostToNewsFeed { get; set; }
    public bool PostToHeadline { get; set; }
    public bool PostToSlideItem { get; set; }
    public List<string> FileUrls { get; set; } = new();
}
