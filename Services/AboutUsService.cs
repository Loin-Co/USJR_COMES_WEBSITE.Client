using System.Net.Http.Json;
using System.Collections.Concurrent;
using USJR_COMES_WEBSITE.ViewModels;

namespace USJR_COMES_WEBSITE.Services;

public class AboutUsService : IAboutUsService
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

    public AboutUsService(HttpClient httpClient)
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
            _cache.TryRemove(key, out _);
        }
    }

    public async Task<AboutUsContentViewModel?> GetAboutUsContentAsync()
    {
        return await GetFromCacheOrFetchAsync(
            "about_us_content",
            async () =>
            {
                try
                {
                    var contents = await _httpClient.GetFromJsonAsync<List<AboutUsContentViewModel>>("api/AboutUsContents");
                    return contents?.FirstOrDefault();
                }
                catch (HttpRequestException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error fetching AboutUs content: {ex.Message}");
                    return null;
                }
                catch (TaskCanceledException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Timeout fetching AboutUs content: {ex.Message}");
                    return null;
                }
            }
        );
    }

    public async Task<bool> SaveAboutUsContentAsync(AboutUsContentViewModel content)
    {
        try
        {
            bool success;
            if (content.Id == 0)
            {
                var response = await _httpClient.PostAsJsonAsync("api/AboutUsContents", content);
                success = response.IsSuccessStatusCode;
            }
            else
            {
                var response = await _httpClient.PutAsJsonAsync($"api/AboutUsContents/{content.Id}", content);
                success = response.IsSuccessStatusCode;
            }

            if (success)
            {
                InvalidateCache("about_us_content");
            }

            return success;
        }
        catch
        {
            return false;
        }
    }
}
