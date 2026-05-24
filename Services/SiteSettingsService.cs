using System.Net.Http.Json;
using USJR_COMES_WEBSITE.ViewModels;

namespace USJR_COMES_WEBSITE.Services;

public class SiteSettingsService : ISiteSettingsService
{
    private readonly HttpClient _http;
    private SiteSettingsViewModel _current = new();
    private DateTime _cacheExpiration = DateTime.MinValue;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
    private Lazy<Task>? _loadingTask = null;

    public SiteSettingsViewModel Current => _current;
    public bool IsLoaded { get; private set; }
    public event Action? OnChange;

    public SiteSettingsService(HttpClient http)
    {
        _http = http;
        // Start background load immediately (fire and forget)
        _ = LoadAsync();
    }

    private bool IsCacheValid => DateTime.UtcNow < _cacheExpiration;

    public async Task LoadAsync()
    {
        // If already loaded and cache is valid, return immediately
        if (IsLoaded && IsCacheValid)
            return;

        // Use Lazy<Task> to avoid lock contention
        _loadingTask ??= new Lazy<Task>(LoadAsyncInternal);

        // If load is already in progress, await the existing task
        if (!_loadingTask.IsValueCreated || !_loadingTask.Value.IsCompleted)
        {
            await _loadingTask.Value;
        }
    }

    private async Task LoadAsyncInternal()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<SiteSettingsViewModel>("api/SiteSettings");
            if (result != null)
            {
                _current = result;
                IsLoaded = true;
                _cacheExpiration = DateTime.UtcNow.Add(_cacheDuration);
                OnChange?.Invoke();
            }
        }
        catch
        {
            // Silently fail - use defaults if load fails
            IsLoaded = false;
        }
    }

    public async Task ForceReloadAsync()
    {
        _cacheExpiration = DateTime.MinValue;
        _loadingTask = null;
        await LoadAsync();
    }

    public async Task<bool> SaveAsync(SiteSettingsRequest req)
    {
        try
        {
            var resp = await _http.PutAsJsonAsync("api/SiteSettings", req);
            if (resp.IsSuccessStatusCode)
            {
                _cacheExpiration = DateTime.MinValue;
                _loadingTask = null;  // Reset lazy task
                await LoadAsync();
                return true;
            }
        }
        catch { }
        return false;
    }

    public void NotifyChanged(SiteSettingsViewModel updated)
    {
        _current = updated;
        IsLoaded = true;
        _cacheExpiration = DateTime.UtcNow.Add(_cacheDuration);
        OnChange?.Invoke();
    }
}
