using System.Net.Http.Json;
using System.Collections.Concurrent;
using USJR_COMES_WEBSITE.ViewModels;

namespace USJR_COMES_WEBSITE.Services;

public class OfficersService : IOfficersService
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

    public OfficersService(HttpClient httpClient)
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

    public async Task<List<OfficerViewModel>> GetAllOfficersAsync()
    {
        return await GetFromCacheOrFetchAsync(
            "all_officers",
            async () => await _httpClient.GetFromJsonAsync<List<OfficerViewModel>>("api/Officers") ?? new()
        );
    }

    public async Task<List<string>> GetAcademicYearsAsync()
    {
        return await GetFromCacheOrFetchAsync(
            "academic_years",
            async () => await _httpClient.GetFromJsonAsync<List<string>>("api/Officers/years") ?? new()
        );
    }

    public async Task<List<OfficerViewModel>> GetOfficersByYearAsync(string academicYear)
    {
        var cacheKey = $"officers_year_{Uri.EscapeDataString(academicYear)}";
        return await GetFromCacheOrFetchAsync(
            cacheKey,
            async () => await _httpClient.GetFromJsonAsync<List<OfficerViewModel>>($"api/Officers/year/{Uri.EscapeDataString(academicYear)}") ?? new()
        );
    }

    public async Task<OfficerViewModel?> CreateOfficerAsync(OfficerViewModel officer)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Officers", officer);
            if (response.IsSuccessStatusCode)
            {
                InvalidateCache("all_officers", "academic_years");
                return await response.Content.ReadFromJsonAsync<OfficerViewModel>();
            }
            return null;
        }
        catch { return null; }
    }

    public async Task<bool> UpdateOfficerAsync(OfficerViewModel officer)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Officers/{officer.Id}", officer);
            if (response.IsSuccessStatusCode)
            {
                InvalidateCache("all_officers", "academic_years");
            }
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> DeleteOfficerAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/Officers/{id}");
            if (response.IsSuccessStatusCode)
            {
                InvalidateCache("all_officers", "academic_years");
            }
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<UserLookupViewModel?> LookupUserBySchoolIdAsync(string schoolId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/Officers/lookup-user/{Uri.EscapeDataString(schoolId)}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<UserLookupViewModel>();
        }
        catch { return null; }
    }
}
