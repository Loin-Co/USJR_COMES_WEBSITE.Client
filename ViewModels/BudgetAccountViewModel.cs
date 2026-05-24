namespace USJR_COMES_WEBSITE.ViewModels;

public class BudgetAccountViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AccountType { get; set; } = "Program"; // "Main" | "Program"
    public decimal TargetBudget { get; set; }
    public string? CreatedBy { get; set; }
    public int? EventId { get; set; }
    public int? AcademicYearId { get; set; }
    public string ApprovalStatus { get; set; } = "Approved";
    public string? ApprovedBySchoolId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public List<BudgetTransactionViewModel> Transactions { get; set; } = new();

    public decimal TotalIncome => Transactions.Where(t => t.TransactionType == "Income" && t.ApprovalStatus == "Approved").Sum(t => t.Amount);
    public decimal TotalExpense => Transactions.Where(t => t.TransactionType == "Expense" && t.ApprovalStatus == "Approved").Sum(t => t.Amount);
    public decimal Balance => TotalIncome - TotalExpense;

    public bool IsMain => AccountType == "Main";
}
