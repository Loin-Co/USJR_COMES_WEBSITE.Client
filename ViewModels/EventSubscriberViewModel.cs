namespace USJR_COMES_WEBSITE.ViewModels;

public class EventSubscriberViewModel
{
    public int Id { get; set; }
    public int UpcomingEventId { get; set; }
    public string SchoolId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string SchoolEmail { get; set; } = string.Empty;
}