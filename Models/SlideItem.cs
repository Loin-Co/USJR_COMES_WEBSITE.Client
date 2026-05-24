namespace USJR_COMES_WEBSITE.Models;

public class SlideItem
{
    public int SlideItemId { get; set; }
    public string SlideItemName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? ImageMimeType { get; set; }
}
