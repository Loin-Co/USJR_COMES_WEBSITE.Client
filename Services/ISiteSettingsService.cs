using USJR_COMES_WEBSITE.ViewModels;

namespace USJR_COMES_WEBSITE.Services;

public interface ISiteSettingsService
{
    SiteSettingsViewModel Current { get; }
    bool IsLoaded { get; }
    event Action? OnChange;
    Task LoadAsync();
    Task ForceReloadAsync();
    Task<bool> SaveAsync(SiteSettingsRequest req);
    void NotifyChanged(SiteSettingsViewModel updated);
}
