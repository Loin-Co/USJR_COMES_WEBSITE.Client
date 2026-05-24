namespace USJR_COMES_WEBSITE.Services;
using USJR_COMES_WEBSITE.ViewModels;

public interface IServicesOfferedService
{
    Task<List<ServiceOfferedViewModel>?> GetServicesOfferedAsync();
    Task<ServicesHeaderViewModel?> GetServicesHeaderAsync();
    Task<bool> CreateServiceOfferedAsync(ServiceOfferedViewModel service);
    Task<bool> UpdateServiceOfferedAsync(int id, ServiceOfferedViewModel service);
    Task<bool> DeleteServiceOfferedAsync(int id);
    Task<bool> ApproveServiceAsync(int id, string adviserId);
    Task<bool> RejectServiceAsync(int id, string adviserId, string reason);

    Task<List<ServiceRequest>?> GetServiceRequestsAsync();
    Task<bool> CreateServiceRequestAsync(ServiceRequest request);
    Task<bool> UpdateServiceRequestStatusAsync(int id, string status);
    Task<bool> UpdateServiceRequestPaymentDetailsAsync(int id, int? budgetAccountId, bool isForMembership, string? academicYear, string? semester);
}
