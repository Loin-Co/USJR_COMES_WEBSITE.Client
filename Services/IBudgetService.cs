using USJR_COMES_WEBSITE.ViewModels;

namespace USJR_COMES_WEBSITE.Services;

public interface IBudgetService
{
    Task<List<AcademicYearViewModel>> GetYearsAsync();
    Task<AcademicYearViewModel?> CreateYearAsync(AcademicYearViewModel year);
    Task<(int toYearId, decimal transferredAmount)?> TransferBudgetAsync(TransferBudgetViewModel request);

    Task<List<BudgetAccountViewModel>> GetAccountsAsync(int? yearId = null);
    Task<List<BudgetAccountViewModel>> GetPendingAccountsAsync();
    Task<BudgetAccountViewModel?> CreateAccountAsync(BudgetAccountViewModel account);
    Task<bool> UpdateAccountAsync(BudgetAccountViewModel account);
    Task<bool> DeleteAccountAsync(int id);
    Task<bool> ApproveAccountAsync(int id, string adviserId);
    Task<bool> RejectAccountAsync(int id, string adviserId, string reason);

    Task<BudgetTransactionViewModel?> CreateTransactionAsync(BudgetTransactionViewModel transaction);
    Task<bool> UpdateTransactionAsync(BudgetTransactionViewModel transaction);
    Task<bool> DeleteTransactionAsync(int id);
}
