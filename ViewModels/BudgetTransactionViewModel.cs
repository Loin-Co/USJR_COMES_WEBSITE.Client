namespace USJR_COMES_WEBSITE.ViewModels;

public class BudgetTransactionViewModel
{
    public int Id { get; set; }
    public int BudgetAccountId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = "Expense"; // "Income" | "Expense"
    public string TransactionDate { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");
    public string? CreatedBy { get; set; }
    public string? RequestedByName { get; set; }
    public string? ReceiptBase64 { get; set; }
    public string? ReceiptMimeType { get; set; }
    public bool HasReceipt { get; set; }
    public string ApprovalStatus { get; set; } = "Pending";
    public string? ApprovedBySchoolId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
}
