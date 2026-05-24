using USJR_COMES_WEBSITE.ViewModels;

namespace USJR_COMES_WEBSITE.Services;

public interface IOfficersService
{
    Task<List<OfficerViewModel>> GetAllOfficersAsync();
    Task<List<string>> GetAcademicYearsAsync();
    Task<List<OfficerViewModel>> GetOfficersByYearAsync(string academicYear);
    Task<OfficerViewModel?> CreateOfficerAsync(OfficerViewModel officer);
    Task<bool> UpdateOfficerAsync(OfficerViewModel officer);
    Task<bool> DeleteOfficerAsync(int id);
    Task<UserLookupViewModel?> LookupUserBySchoolIdAsync(string schoolId);
}
