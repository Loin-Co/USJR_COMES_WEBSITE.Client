namespace USJR_COMES_WEBSITE.Services;
using System.Net.Http.Json;
using System.Collections.Concurrent;
using USJR_COMES_WEBSITE.Models;

public class HomeService : IHomeService
{
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly HashSet<string> _bustOnNextFetch = new();
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    private class CacheEntry
    {
        public object Data { get; set; }
        public DateTime ExpirationTime { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpirationTime;
    }

    public HomeService(HttpClient httpClient)
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

    public void InvalidateHomeCache()
    {
        _cache.TryRemove("headlines", out _);
        _cache.TryRemove("slide_items", out _);
        _bustOnNextFetch.Add("headlines");
        _bustOnNextFetch.Add("slide_items");
    }

    public async Task<List<Headline>?> GetHeadlinesAsync()
    {
        var bust = _bustOnNextFetch.Remove("headlines");
        return await GetFromCacheOrFetchAsync(
            "headlines",
            async () =>
            {
                try
                {
                    var url = "api/Headlines";
                    if (bust) url += $"?_cb={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                    return await _httpClient.GetFromJsonAsync<List<Headline>>(url);
                }
                catch
                {
                    return null;
                }
            }
        );
    }

    public async Task<List<SlideItem>?> GetSlideItemsAsync()
    {
        var bust = _bustOnNextFetch.Remove("slide_items");
        return await GetFromCacheOrFetchAsync(
            "slide_items",
            async () =>
            {
                try
                {
                    var url = "api/SlideItems";
                    if (bust) url += $"?_cb={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                    return await _httpClient.GetFromJsonAsync<List<SlideItem>>(url);
                }
                catch
                {
                    return null;
                }
            }
        );
    }
}
