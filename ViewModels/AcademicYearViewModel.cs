namespace USJR_COMES_WEBSITE.ViewModels;

public class AcademicYearViewModel
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public string? CurrentSemester { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TransferBudgetViewModel
{
    public int FromYearId { get; set; }
    public int ToYearId { get; set; }       // 0 = create new
    public string? NewYearLabel { get; set; }
    public string? CreatedBy { get; set; }
    public string? RequestedByName { get; set; }
    public decimal Amount { get; set; }
}
