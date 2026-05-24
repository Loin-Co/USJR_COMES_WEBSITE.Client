using Microsoft.AspNetCore.Components;
using USJR_COMES_WEBSITE.Services;

namespace USJR_COMES_WEBSITE.ViewModels;

public class UserViewModel
{
    private readonly UserStateService _userStateService;
    private readonly NavigationManager _navigationManager;
    private readonly IUserServices _userServices;

    public bool IsLoggedIn => _userStateService.CurrentUser is not null;
    public string FullName => $"{_userStateService.CurrentUser?.FirstName} {_userStateService.CurrentUser?.LastName}".Trim();
    public string UserSchoolId => _userStateService.CurrentUser?.SchoolId ?? string.Empty;
    public string UserEmail => _userStateService.CurrentUser?.SchoolEmail ?? string.Empty;
    public string AccountRole => _userStateService.CurrentUser?.AccountRole ?? string.Empty;

    public string? ProfileImageSrc
    {
        get
        {
            var u = _userStateService.CurrentUser;
            if (u == null || string.IsNullOrEmpty(u.ProfileImageData) || string.IsNullOrEmpty(u.ProfileImageMimeType))
                return null;
            return $"data:{u.ProfileImageMimeType};base64,{u.ProfileImageData}";
        }
    }

    public UserViewModel(UserStateService userStateService, NavigationManager navigationManager, IUserServices userServices)
    {
        _userStateService = userStateService;
        _navigationManager = navigationManager;
        _userServices = userServices;
    }

    public async Task UploadAvatarAsync(string imageData, string mimeType)
    {
        var schoolId = _userStateService.CurrentUser?.SchoolId;
        if (string.IsNullOrEmpty(schoolId)) return;
        var ok = await _userServices.UpdateAvatarAsync(schoolId, imageData, mimeType);
        if (ok) _userStateService.UpdateProfileImage(imageData, mimeType);
    }

    public void Logout()
    {
        _userStateService.Logout();
        _navigationManager.NavigateTo("/");
    }

    public void SubscribeToChanges(Action handler) => _userStateService.OnChange += handler;
    public void UnsubscribeFromChanges(Action handler) => _userStateService.OnChange -= handler;
}
