using System.Text.Json;
using Microsoft.JSInterop;
using USJR_COMES_WEBSITE.Models;

namespace USJR_COMES_WEBSITE.Services;

/// <summary>
/// Holds the currently logged-in user and persists the session to localStorage
/// so it survives page refreshes in WASM mode.
/// </summary>
public class UserStateService
{
    private readonly IJSRuntime _js;
    private bool _sessionLoaded;

    public User? CurrentUser { get; private set; }
    public event Action? OnChange;

    public UserStateService(IJSRuntime js)
    {
        _js = js;
    }

    /// <summary>
    /// Restore session from localStorage on first app load.
    /// Safe to call multiple times — only runs once.
    /// </summary>
    public async Task TryRestoreSessionAsync()
    {
        if (_sessionLoaded) return;
        _sessionLoaded = true;
        try
        {
            var json = await _js.InvokeAsync<string>("offlineSync.loadUserSession");
            if (!string.IsNullOrEmpty(json))
            {
                var user = JsonSerializer.Deserialize<User>(json);
                if (user != null)
                {
                    CurrentUser = user;
                    OnChange?.Invoke();
                }
            }
        }
        catch { /* JS not ready yet or no saved session */ }
    }

    public void SetUser(User? user)
    {
        CurrentUser = user;
        _ = PersistSessionAsync(user);
        OnChange?.Invoke();
    }

    public void Logout()
    {
        CurrentUser = null;
        _ = ClearSessionAsync();
        OnChange?.Invoke();
    }

    public void UpdateProfileImage(string? imageData, string? mimeType)
    {
        if (CurrentUser == null) return;
        CurrentUser.ProfileImageData = imageData;
        CurrentUser.ProfileImageMimeType = mimeType;
        _ = PersistSessionAsync(CurrentUser);
        OnChange?.Invoke();
    }

    private async Task PersistSessionAsync(User? user)
    {
        try
        {
            var json = user != null ? JsonSerializer.Serialize(user) : "";
            await _js.InvokeVoidAsync("offlineSync.saveUserSession", json);
        }
        catch { }
    }

    private async Task ClearSessionAsync()
    {
        try { await _js.InvokeVoidAsync("offlineSync.clearUserSession"); }
        catch { }
    }
}
