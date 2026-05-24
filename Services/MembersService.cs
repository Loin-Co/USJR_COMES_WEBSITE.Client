using System.Net.Http.Json;
using USJR_COMES_WEBSITE.ViewModels;

namespace USJR_COMES_WEBSITE.Services;

public class MembersService : IMembersService
{
    private readonly HttpClient _http;
    private readonly Dictionary<string, CacheEntry> _cache = new();
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    private class CacheEntry
    {
        public object Data { get; set; }
        public DateTime ExpirationTime { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpirationTime;
    }

    public MembersService(HttpClient http)
    {
        _http = http;
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
            if (key.EndsWith("*"))
            {
                var prefix = key[..^1];
                var toRemove = _cache.Keys.Where(k => k.StartsWith(prefix)).ToList();
                foreach (var k in toRemove) _cache.Remove(k);
            }
            else
            {
                _cache.Remove(key);
            }
        }
    }

    public async Task<List<AcademicYearViewModel>> GetYearsAsync()
    {
        return await GetFromCacheOrFetchAsync(
            "members_years",
            async () =>
            {
                try { return await _http.GetFromJsonAsync<List<AcademicYearViewModel>>("api/Membership/years") ?? new(); }
                catch { return new(); }
            }
        );
    }

    public async Task<AcademicYearViewModel?> CreateYearAsync(string label, bool isCurrent)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/Membership/years", new { label, isCurrent });
            if (res.IsSuccessStatusCode)
            {
                InvalidateCache("members_years");
                return await res.Content.ReadFromJsonAsync<AcademicYearViewModel>();
            }
            return null;
        }
        catch { return null; }
    }

    public async Task<bool> SetCurrentYearAsync(int yearId)
    {
        try
        {
            var res = await _http.PutAsJsonAsync($"api/Membership/years/{yearId}/setcurrent", new { });
            if (res.IsSuccessStatusCode)
            {
                InvalidateCache("members_years");
                return true;
            }
            return false;
        }
        catch { return false; }
    }

    public async Task<bool> SetCurrentYearWithSemesterAsync(int yearId, string semester)
    {
        try
        {
            var res = await _http.PutAsJsonAsync($"api/Membership/years/{yearId}/setcurrent", new { semester });
            if (res.IsSuccessStatusCode)
            {
                InvalidateCache("members_years");
                return true;
            }
            return false;
        }
        catch { return false; }
    }

    public async Task<List<MemberViewModel>> GetUsersForYearAsync(string yearLabel)
    {
        var cacheKey = $"members_users_{Uri.EscapeDataString(yearLabel)}";
        return await GetFromCacheOrFetchAsync(
            cacheKey,
            async () =>
            {
                try
                {
                    return await _http.GetFromJsonAsync<List<MemberViewModel>>(
                        $"api/Membership/users/{Uri.EscapeDataString(yearLabel)}") ?? new();
                }
                catch { return new(); }
            }
        );
    }

    public async Task<bool> PromoteAsync(PromoteMemberRequest request)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/Membership/promote", request);
            if (res.IsSuccessStatusCode)
            {
                InvalidateCache("members_years", "members_users_*");
            }
            return res.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> RevokeAsync(string yearLabel, string schoolId)
    {
        try
        {
            var res = await _http.DeleteAsync(
                $"api/Membership/revoke/{Uri.EscapeDataString(yearLabel)}/{Uri.EscapeDataString(schoolId)}");
            if (res.IsSuccessStatusCode)
            {
                InvalidateCache("members_years", "members_users_*");
            }
            return res.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<(string? year, bool isMember, bool isOfficerOfYear)> GetCurrentStatusAsync(string schoolId)
    {
        var cacheKey = $"member_status_{Uri.EscapeDataString(schoolId)}";
        var res = await GetFromCacheOrFetchAsync(
            cacheKey,
            async () =>
            {
                try
                {
                    return await _http.GetFromJsonAsync<CurrentStatusDto>(
                        $"api/Membership/current-status/{Uri.EscapeDataString(schoolId)}");
                }
                catch { return null; }
            }
        );
        return (res?.CurrentYear, res?.IsMember ?? false, res?.IsOfficerOfYear ?? false);
    }

    private class CurrentStatusDto
    {
        public string? CurrentYear { get; set; }
        public bool IsMember { get; set; }
        public bool IsOfficerOfYear { get; set; }
    }

    public async Task<bool> CheckMembershipForYearAsync(string yearLabel, string schoolId)
    {
        try
        {
            var res = await _http.GetFromJsonAsync<MembershipCheckDto>(
                $"api/Membership/check/{Uri.EscapeDataString(yearLabel)}/{Uri.EscapeDataString(schoolId)}");
            return res?.IsMember ?? false;
        }
        catch { return false; }
    }

    private class MembershipCheckDto { public bool IsMember { get; set; } }
}
