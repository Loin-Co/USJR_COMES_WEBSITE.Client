namespace USJR_COMES_WEBSITE.Services;
using System.Net.Http.Json;
using System.Collections.Concurrent;
using USJR_COMES_WEBSITE.ViewModels;
using USJR_COMES_WEBSITE.APIs;

public class ServicesOfferedService : IServicesOfferedService
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

    public ServicesOfferedService(HttpClient httpClient)
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

    public async Task<List<ServiceOfferedViewModel>?> GetServicesOfferedAsync()
    {
        return await GetFromCacheOrFetchAsync(
            "services_offered",
            async () =>
            {
                try { return await _httpClient.GetFromJsonAsync<List<ServiceOfferedViewModel>>(ApiEndpoints.ServicesOffered.Base); }
                catch { return null; }
            }
        );
    }

    public async Task<ServicesHeaderViewModel?> GetServicesHeaderAsync()
    {
        return await GetFromCacheOrFetchAsync(
            "services_header",
            async () =>
            {
                try { return await _httpClient.GetFromJsonAsync<ServicesHeaderViewModel>("api/ServicesHeader"); }
                catch { return null; }
            }
        );
    }

    public async Task<bool> CreateServiceOfferedAsync(ServiceOfferedViewModel service)
    {
        try 
        { 
            var result = (await _httpClient.PostAsJsonAsync(ApiEndpoints.ServicesOffered.Base, service)).IsSuccessStatusCode;
            if (result) InvalidateCache("services_offered", "services_header");
            return result;
        }
        catch { return false; }
    }

    public async Task<bool> UpdateServiceOfferedAsync(int id, ServiceOfferedViewModel service)
    {
        try 
        { 
            var result = (await _httpClient.PutAsJsonAsync(ApiEndpoints.ServicesOffered.WithId(id), service)).IsSuccessStatusCode;
            if (result) InvalidateCache("services_offered", "services_header");
            return result;
        }
        catch { return false; }
    }

    public async Task<bool> DeleteServiceOfferedAsync(int id)
    {
        try 
        { 
            var result = (await _httpClient.DeleteAsync(ApiEndpoints.ServicesOffered.WithId(id))).IsSuccessStatusCode;
            if (result) InvalidateCache("services_offered", "services_header");
            return result;
        }
        catch { return false; }
    }

    public async Task<bool> ApproveServiceAsync(int id, string adviserId)
    {
        try 
        { 
            var result = (await _httpClient.PutAsJsonAsync(ApiEndpoints.ServicesOffered.Approve(id), new { ActorSchoolId = adviserId })).IsSuccessStatusCode;
            if (result) InvalidateCache("services_offered", "services_header");
            return result;
        }
        catch { return false; }
    }

    public async Task<bool> RejectServiceAsync(int id, string adviserId, string reason)
    {
        try { return (await _httpClient.PutAsJsonAsync(ApiEndpoints.ServicesOffered.Reject(id), new { ActorSchoolId = adviserId, Reason = reason })).IsSuccessStatusCode; }
        catch { return false; }
    }

    public async Task<List<ServiceRequest>?> GetServiceRequestsAsync()
    {
        try { return await _httpClient.GetFromJsonAsync<List<ServiceRequest>>("api/ServiceRequests"); }
        catch { return null; }
    }

    public async Task<bool> CreateServiceRequestAsync(ServiceRequest request)
    {
        try { return (await _httpClient.PostAsJsonAsync("api/ServiceRequests", request)).IsSuccessStatusCode; }
        catch { return false; }
    }

    public async Task<bool> UpdateServiceRequestStatusAsync(int id, string status)
    {
        try { return (await _httpClient.PutAsJsonAsync($"api/ServiceRequests/{id}/status", new { Status = status })).IsSuccessStatusCode; }
        catch { return false; }
    }

    public async Task<bool> UpdateServiceRequestPaymentDetailsAsync(int id, int? budgetAccountId, bool isForMembership, string? academicYear, string? semester)
    {
        try
        {
            var payload = new { budgetAccountId, isForMembership, academicYear, semester };
            return (await _httpClient.PatchAsJsonAsync($"api/ServiceRequests/{id}/payment-details", payload)).IsSuccessStatusCode;
        }
        catch { return false; }
    }
}
