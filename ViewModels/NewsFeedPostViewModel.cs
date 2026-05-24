using System.ComponentModel.DataAnnotations;

namespace USJR_COMES_WEBSITE.ViewModels;

public class NewsFeedPostViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DateTime { get; set; } = string.Empty;

    [Required(ErrorMessage = "Title is required")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;
    // Semicolon-separated mime types matching the ImageUrl entries
    public string PostMimeTypes { get; set; } = string.Empty;
    public string SchoolId { get; set; } = string.Empty;
    public int Likes { get; set; }
    public int Comments { get; set; }
    public int Shares { get; set; }

    private string _status = "Unpublished";
    [Required(ErrorMessage = "Status is required")]
    public string Status
    {
        get => _status;
        set
        {
            _status = value;
            if (_status == "Unpublished")
            {
                PostToNewsFeed = false;
                PostToHeadline = false;
                PostToSlideItem = false;
                PostToEvents = false;
            }
        }
    }

    // Approval workflow
    public string ApprovalStatus { get; set; } = "None";
    public string? ApprovedBy { get; set; }
    public string? RejectionReason { get; set; }
    public string CreatedByRole { get; set; } = "Officer";

    public bool IsLiked { get; set; }
    public string? ProfileImageSrc { get; set; }

    public bool PostToNewsFeed { get; set; }
    public bool PostToHeadline { get; set; }
    public bool PostToSlideItem { get; set; }
    public bool PostToEvents { get; set; }
    public string EventDate { get; set; } = string.Empty;
    public string EventTime { get; set; } = string.Empty;
    public string EventVenue { get; set; } = string.Empty;
}
