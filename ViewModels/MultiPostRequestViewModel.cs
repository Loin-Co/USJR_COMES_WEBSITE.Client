using System.ComponentModel.DataAnnotations;

namespace USJR_COMES_WEBSITE.ViewModels;

public class MultiPostRequestViewModel
{
    [Required(ErrorMessage = "Title is required")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;
    public List<string> FileUrls { get; set; } = new();
    public string Name { get; set; } = string.Empty;
    public string SchoolId { get; set; } = string.Empty;
    public string CreatedByRole { get; set; } = "Officer";

    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } = "Unpublished";

    public string EventDate { get; set; } = string.Empty;
    public string EventTime { get; set; } = string.Empty;
    public string EventVenue { get; set; } = string.Empty;

    public bool PostToNewsFeed { get; set; } = true;
    public bool PostToHeadline { get; set; } = false;
    public bool PostToSlideItem { get; set; } = false;
    public bool PostToEvents { get; set; } = false;
}