namespace USJR_COMES_WEBSITE.ViewModels;

public class PostCommentViewModel
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string SchoolId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string DateTime { get; set; } = string.Empty;
    public string? ProfileImageSrc { get; set; }
}