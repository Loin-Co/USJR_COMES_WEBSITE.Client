namespace USJR_COMES_WEBSITE.Services;
using System.Net.Http.Json;
using System.Collections.Concurrent;
using USJR_COMES_WEBSITE.Models;

public class HomeService : IHomeService
{
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
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

    public async Task<List<Headline>?> GetHeadlinesAsync()
    {
        return await GetFromCacheOrFetchAsync(
            "headlines",
            async () =>
            {
                try
                {
                    return await _httpClient.GetFromJsonAsync<List<Headline>>("api/Headlines");
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
        return await GetFromCacheOrFetchAsync(
            "slide_items",
            async () =>
            {
                try
                {
                    return await _httpClient.GetFromJsonAsync<List<SlideItem>>("api/SlideItems");
                }
                catch
                {
                    return null;
                }
            }
        );
    }
}
