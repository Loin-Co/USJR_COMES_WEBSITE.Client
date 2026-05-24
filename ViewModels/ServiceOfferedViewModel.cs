namespace USJR_COMES_WEBSITE.ViewModels;

public class ServiceOfferedViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int ActionType { get; set; }
    public string? FormDefinitionJson { get; set; }
    public bool IsPublished { get; set; } = true;

    // Payment / Membership fields (only relevant when ActionType == Payment)
    public int? BudgetAccountId { get; set; }
    public bool IsForMembership { get; set; }
    public int? AcademicYearId { get; set; }
    public string? BudgetAccountTitle { get; set; }

    // Approval workflow
    public string ApprovalStatus { get; set; } = "None";
    public string? ApprovedBy { get; set; }
    public string? RejectionReason { get; set; }
    public string? CreatedBy { get; set; }
    public string CreatedByRole { get; set; } = "Officer";
    public string? CreatedByName { get; set; }
}
