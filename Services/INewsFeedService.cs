namespace USJR_COMES_WEBSITE.Services;
using USJR_COMES_WEBSITE.ViewModels;

public interface INewsFeedService
{
    Task<List<NewsFeedPostViewModel>?> GetNewsFeedPostsAsync(string? schoolId = null);
    Task<(bool IsLiked, int Likes)?> ToggleLikeAsync(int postId, string schoolId);
    Task<bool> CreateNewsFeedPostAsync(NewsFeedPostViewModel post);
    Task<bool> CreateMultiPostAsync(MultiPostRequestViewModel request);
    Task<bool> UpdateNewsFeedPostAsync(NewsFeedPostViewModel post);
    Task<bool> DeleteNewsFeedPostAsync(int id);
    Task<bool> ApprovePostAsync(int id, string adviserId);
    Task<bool> RejectPostAsync(int id, string adviserId, string reason);

    Task<List<UpcomingEventViewModel>?> GetUpcomingEventsAsync();
    Task<bool> CreateUpcomingEventAsync(UpcomingEventViewModel ev);
    Task<bool> UpdateUpcomingEventAsync(UpcomingEventViewModel ev);
    Task<bool> DeleteUpcomingEventAsync(int id);
    Task<bool> ApproveEventAsync(int id, string adviserId);
    Task<bool> RejectEventAsync(int id, string adviserId, string reason);

    Task<List<PostCommentViewModel>?> GetCommentsForPostAsync(int postId);
    Task<PostCommentViewModel?> CreateCommentAsync(PostCommentViewModel comment);
    Task<bool> SubscribeToEventAsync(EventSubscriberViewModel subscriber);
    Task<bool> CheckUserSubscriptionAsync(int eventId, string schoolId);
    Task<bool> UnsubscribeFromEventAsync(int eventId, string schoolId);
    Task<List<EventSubscriberViewModel>?> GetEventSubscribersAsync(int eventId);
}
