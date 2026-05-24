using USJR_COMES_WEBSITE.ViewModels;

namespace USJR_COMES_WEBSITE.Services;

public interface IMembersService
{
    Task<List<AcademicYearViewModel>> GetYearsAsync();
    Task<AcademicYearViewModel?> CreateYearAsync(string label, bool isCurrent);
    Task<bool> SetCurrentYearAsync(int yearId);
    Task<bool> SetCurrentYearWithSemesterAsync(int yearId, string semester);
    Task<List<MemberViewModel>> GetUsersForYearAsync(string yearLabel);
    Task<bool> PromoteAsync(PromoteMemberRequest request);
    Task<bool> RevokeAsync(string yearLabel, string schoolId);
    Task<(string? year, bool isMember, bool isOfficerOfYear)> GetCurrentStatusAsync(string schoolId);
    Task<bool> CheckMembershipForYearAsync(string yearLabel, string schoolId);
}
