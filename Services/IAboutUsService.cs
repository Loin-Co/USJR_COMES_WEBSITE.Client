using USJR_COMES_WEBSITE.ViewModels;

namespace USJR_COMES_WEBSITE.Services;

public interface IAboutUsService
{
    Task<AboutUsContentViewModel?> GetAboutUsContentAsync();
    Task<bool> SaveAboutUsContentAsync(AboutUsContentViewModel content);
}
