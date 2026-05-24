namespace USJR_COMES_WEBSITE.Services;
using System.Net.Http.Json;
using USJR_COMES_WEBSITE.ViewModels;
using USJR_COMES_WEBSITE.APIs;

public class NewsFeedService : INewsFeedService
{
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, CacheEntry> _cache = new();
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    private class CacheEntry
    {
        public object Data { get; set; }
        public DateTime ExpirationTime { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpirationTime;
    }

    public NewsFeedService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private async Task<T> GetFromCacheOrFetchAsync<T>(string cacheKey, Func<Task<T>> fetchFunc)
    {
        if (_cache.TryGetValue(cacheKey, out var entry) && !entry.IsExpired)
            return (T)entry.Data;

        try
        {
            var result = await fetchFunc();
            _cache[cacheKey] = new CacheEntry { Data = result!, ExpirationTime = DateTime.UtcNow.Add(_cacheExpiration) };
            return result;
        }
        catch
        {
            return entry != null ? (T)entry.Data : default!;
        }
    }

    private void InvalidateCache(params string[] keys)
    {
        foreach (var key in keys)
        {
            _cache.Remove(key);
        }
    }

    public async Task<List<NewsFeedPostViewModel>?> GetNewsFeedPostsAsync(string? schoolId = null)
    {
        var cacheKey = string.IsNullOrEmpty(schoolId) ? "newsfeed_all" : $"newsfeed_user_{Uri.EscapeDataString(schoolId)}";

        return await GetFromCacheOrFetchAsync(
            cacheKey,
            async () =>
            {
                try
                {
                    var url = string.IsNullOrEmpty(schoolId)
                        ? ApiEndpoints.NewsFeed.Base
                        : $"{ApiEndpoints.NewsFeed.Base}?schoolId={Uri.EscapeDataString(schoolId)}";
                    return await _httpClient.GetFromJsonAsync<List<NewsFeedPostViewModel>>(url);
                }
                catch { return null; }
            }
        );
    }

    public async Task<(bool IsLiked, int Likes)?> ToggleLikeAsync(int postId, string schoolId)
    {
        try
        {
            var res = await _httpClient.PutAsJsonAsync($"{ApiEndpoints.NewsFeed.Base}/{postId}/like", new { schoolId });
            if (!res.IsSuccessStatusCode) return null;
            var obj = await res.Content.ReadFromJsonAsync<LikeResult>();
            if (obj != null)
            {
                InvalidateCache("newsfeed_all");
            }
            return obj is null ? null : (obj.IsLiked, obj.Likes);
        }
        catch { return null; }
    }

    private class LikeResult
    {
        public bool IsLiked { get; set; }
        public int Likes { get; set; }
    }

    public async Task<bool> CreateNewsFeedPostAsync(NewsFeedPostViewModel post)
    {
        try 
        { 
            var result = (await _httpClient.PostAsJsonAsync(ApiEndpoints.NewsFeed.Base, post)).IsSuccessStatusCode;
            if (result) InvalidateCache("newsfeed_all", "upcoming_events");
            return result;
        }
        catch { return false; }
    }

    public async Task<bool> CreateMultiPostAsync(MultiPostRequestViewModel request)
    {
        try 
        { 
            var result = (await _httpClient.PostAsJsonAsync(ApiEndpoints.NewsFeed.MultiPost, request)).IsSuccessStatusCode;
            if (result) InvalidateCache("newsfeed_all", "upcoming_events");
            return result;
        }
        catch { return false; }
    }

    public async Task<bool> UpdateNewsFeedPostAsync(NewsFeedPostViewModel post)
    {
        try 
        { 
            var result = (await _httpClient.PutAsJsonAsync(ApiEndpoints.NewsFeed.WithId(post.Id), post)).IsSuccessStatusCode;
            if (result) InvalidateCache("newsfeed_all", "upcoming_events");
            return result;
        }
        catch { return false; }
    }

    public async Task<bool> DeleteNewsFeedPostAsync(int id)
    {
        try 
        { 
            var result = (await _httpClient.DeleteAsync(ApiEndpoints.NewsFeed.WithId(id))).IsSuccessStatusCode;
            if (result) InvalidateCache("newsfeed_all", "upcoming_events");
            return result;
        }
        catch { return false; }
    }

    public async Task<bool> ApprovePostAsync(int id, string adviserId)
    {
        try 
        { 
            var result = (await _httpClient.PutAsJsonAsync(ApiEndpoints.NewsFeed.Approve(id), new { ActorSchoolId = adviserId })).IsSuccessStatusCode;
            if (result) InvalidateCache("newsfeed_all", "upcoming_events");
            return result;
        }
        catch { return false; }
    }

    public async Task<bool> RejectPostAsync(int id, string adviserId, string reason)
    {
        try 
        { 
            var result = (await _httpClient.PutAsJsonAsync(ApiEndpoints.NewsFeed.Reject(id), new { ActorSchoolId = adviserId, Reason = reason })).IsSuccessStatusCode;
            if (result) InvalidateCache("newsfeed_all", "upcoming_events");
            return result;
        }
        catch { return false; }
    }

    public async Task<List<UpcomingEventViewModel>?> GetUpcomingEventsAsync()
    {
        return await GetFromCacheOrFetchAsync(
            "upcoming_events",
            async () =>
            {
                try { return await _httpClient.GetFromJsonAsync<List<UpcomingEventViewModel>>(ApiEndpoints.UpcomingEvents.Base); }
                catch { return null; }
            }
        );
    }

    public async Task<bool> CreateUpcomingEventAsync(UpcomingEventViewModel ev)
    {
        try 
        { 
            var result = (await _httpClient.PostAsJsonAsync(ApiEndpoints.UpcomingEvents.Base, ev)).IsSuccessStatusCode;
            if (result) InvalidateCache("upcoming_events");
            return result;
        }
        catch { return false; }
    }

    public async Task<bool> UpdateUpcomingEventAsync(UpcomingEventViewModel ev)
    {
        try 
        { 
            var result = (await _httpClient.PutAsJsonAsync(ApiEndpoints.UpcomingEvents.WithId(ev.Id), ev)).IsSuccessStatusCode;
            if (result) InvalidateCache("upcoming_events");
            return result;
        }
        catch { return false; }
    }

    public async Task<bool> DeleteUpcomingEventAsync(int id)
    {
        try
        {
            var result = (await _httpClient.DeleteAsync(ApiEndpoints.UpcomingEvents.WithId(id))).IsSuccessStatusCode;
            if (result) InvalidateCache("upcoming_events");
            return result;
        }
        catch { return false; }
    }

    public async Task<bool> ApproveEventAsync(int id, string adviserId)
    {
        try
        {
            var result = (await _httpClient.PutAsJsonAsync(ApiEndpoints.UpcomingEvents.Approve(id), new { ActorSchoolId = adviserId })).IsSuccessStatusCode;
            if (result) InvalidateCache("upcoming_events");
            return result;
        }
        catch { return false; }
    }

    public async Task<bool> RejectEventAsync(int id, string adviserId, string reason)
    {
        try
        {
            var result = (await _httpClient.PutAsJsonAsync(ApiEndpoints.UpcomingEvents.Reject(id), new { ActorSchoolId = adviserId, Reason = reason })).IsSuccessStatusCode;
            if (result) InvalidateCache("upcoming_events");
            return result;
        }
        catch { return false; }
    }

    public async Task<List<PostCommentViewModel>?> GetCommentsForPostAsync(int postId)
    {
        try { return await _httpClient.GetFromJsonAsync<List<PostCommentViewModel>>(ApiEndpoints.PostComments.ForPost(postId)); }
        catch { return null; }
    }

    public async Task<PostCommentViewModel?> CreateCommentAsync(PostCommentViewModel comment)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.PostComments.Base, comment);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<PostCommentViewModel>() : null;
        }
        catch { return null; }
    }

    public async Task<bool> SubscribeToEventAsync(EventSubscriberViewModel subscriber)
    {
        try { return (await _httpClient.PostAsJsonAsync($"{ApiEndpoints.UpcomingEvents.Base}/{subscriber.UpcomingEventId}/subscribe", subscriber)).IsSuccessStatusCode; }
        catch { return false; }
    }

    public async Task<bool> CheckUserSubscriptionAsync(int eventId, string schoolId)
    {
        try { return await _httpClient.GetFromJsonAsync<bool>($"{ApiEndpoints.UpcomingEvents.Base}/{eventId}/subscribe/{schoolId}"); }
        catch { return false; }
    }

    public async Task<bool> UnsubscribeFromEventAsync(int eventId, string schoolId)
    {
        try { return (await _httpClient.DeleteAsync($"{ApiEndpoints.UpcomingEvents.Base}/{eventId}/unsubscribe/{schoolId}")).IsSuccessStatusCode; }
        catch { return false; }
    }

    public async Task<List<EventSubscriberViewModel>?> GetEventSubscribersAsync(int eventId)
    {
        try { return await _httpClient.GetFromJsonAsync<List<EventSubscriberViewModel>>($"{ApiEndpoints.UpcomingEvents.Base}/{eventId}/subscribers"); }
        catch { return null; }
    }
}
