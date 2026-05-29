using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using USJR_COMES_WEBSITE.Services;

namespace USJR_COMES_WEBSITE.ViewModels;

public enum ServiceType
{
    FileUpload,
    Payment,
    Inventory,
    Appointment,
    Clearance
}

public class ServiceRequest
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";

    // Store the dynamic answers as a JSON string
    public string? DynamicDataJson { get; set; }

    // Helper to read the JSON seamlessly in Razor components
    public Dictionary<string, string> GetDynamicData()
    {
        if (string.IsNullOrEmpty(DynamicDataJson)) return new Dictionary<string, string>();
        return JsonSerializer.Deserialize<Dictionary<string, string>>(DynamicDataJson) ?? new Dictionary<string, string>();
    }
}

public class ManagementServicesViewModel
{
    private readonly IServicesOfferedService _servicesService;
    private readonly IOfflineSyncService _offlineSync;
    public event Action? OnChange;

    private readonly UserStateService? _userState;

    public ManagementServicesViewModel(IServicesOfferedService servicesService, UserStateService userState, IOfflineSyncService offlineSync)
    {
        _servicesService = servicesService;
        _userState = userState;
        _offlineSync = offlineSync;
        NewService = new ServiceOfferedViewModel { ActionType = (int)ServiceType.FileUpload };
    }

    public string CurrentRole => _userState?.CurrentUser?.AccountRole.ToString() ?? "NonMember";
    public string CurrentSchoolId => _userState?.CurrentUser?.SchoolId ?? string.Empty;

    private void NotifyStateChanged() => OnChange?.Invoke();

    public bool ShowSuccess { get; private set; }
    public string SuccessMessage { get; private set; } = "Operation successful.";
    public void HideSuccess() { ShowSuccess = false; NotifyStateChanged(); }

    public bool ShouldReloadPage { get; private set; }
    public void ClearReloadFlag() { ShouldReloadPage = false; }

    private string _serviceSearchTerm = "";
    public string ServiceSearchTerm
    {
        get => _serviceSearchTerm;
        set { _serviceSearchTerm = value; NotifyStateChanged(); }
    }

    private string _requestSearchTerm = "";
    public string RequestSearchTerm
    {
        get => _requestSearchTerm;
        set { _requestSearchTerm = value; NotifyStateChanged(); }
    }

    private string _sortOrder = "None"; // None, A-Z, Z-A
    public string SortOrder
    {
        get => _sortOrder;
        set { _sortOrder = value; NotifyStateChanged(); }
    }

    private string _statusFilter = "All"; // All, Pending, Completed, etc.
    public string StatusFilter
    {
        get => _statusFilter;
        set { _statusFilter = value; NotifyStateChanged(); }
    }

    private string _fileTypeFilter = "All"; // All, PDF, DOCX, Image
    public string FileTypeFilter
    {
        get => _fileTypeFilter;
        set { _fileTypeFilter = value; NotifyStateChanged(); }
    }

    public ServiceRequest? ViewingRequest { get; private set; }
    public bool IsViewerOpen { get; private set; }

    public void ViewRequest(ServiceRequest request)
    {
        ViewingRequest = request;
        IsViewerOpen = true;
        NotifyStateChanged();
    }

    public void CloseViewer()
    {
        IsViewerOpen = false;
        ViewingRequest = null;
        NotifyStateChanged();
    }

    public List<ServiceOfferedViewModel> Services { get; private set; } = new();
    public List<ServiceRequest> Requests { get; private set; } = new();

    public ServiceOfferedViewModel? SelectedService { get; private set; }
    public bool IsAddModalOpen { get; private set; }
    public bool IsEditModalOpen { get; private set; }
    public ServiceOfferedViewModel NewService { get; set; }

    public IEnumerable<ServiceOfferedViewModel> FilteredServices =>
        string.IsNullOrWhiteSpace(ServiceSearchTerm)
            ? Services
            : Services.Where(s => s.Name.Contains(ServiceSearchTerm, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<ServiceRequest> FilteredRequests
    {
        get
        {
            if (SelectedService == null) return Array.Empty<ServiceRequest>();

            var query = Requests.Where(r => r.ServiceId == SelectedService.Id);

            if (!string.IsNullOrWhiteSpace(RequestSearchTerm))
            {
                query = query.Where(r => r.UserName.Contains(RequestSearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            if (StatusFilter != "All")
            {
                query = query.Where(r => r.Status.Equals(StatusFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (FileTypeFilter != "All" && SelectedService.ActionType == (int)ServiceType.FileUpload)
            {
                query = query.Where(r => 
                {
                    var data = r.GetDynamicData();
                    if (data.TryGetValue("Attachment", out var filename))
                    {
                        if (FileTypeFilter == "PDF") return filename.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
                        if (FileTypeFilter == "DOCX") return filename.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) || filename.EndsWith(".doc", StringComparison.OrdinalIgnoreCase);
                        if (FileTypeFilter == "Image") return filename.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || filename.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) || filename.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
                    }
                    return false;
                });
            }

            if (SortOrder == "A-Z")
            {
                query = query.OrderBy(r => r.UserName);
            }
            else if (SortOrder == "Z-A")
            {
                query = query.OrderByDescending(r => r.UserName);
            }

            return query;
        }
    }


    public async Task InitializeAsync()
    {
        await LoadServicesAsync();
        await LoadRequestsAsync();

        NotifyStateChanged();
    }

    public async Task LoadServicesAsync()
    {
        var result = await _servicesService.GetServicesOfferedAsync();
        Services = result ?? new List<ServiceOfferedViewModel>();
        NotifyStateChanged();
    }

    public async Task LoadRequestsAsync()
    {
        var result = await _servicesService.GetServiceRequestsAsync();
        Requests = result ?? new List<ServiceRequest>();
        NotifyStateChanged();
    }

    public async Task OpenAddServiceModalAsync()
    {
        NewService = new ServiceOfferedViewModel { ActionType = (int)ServiceType.FileUpload };
        IsAddModalOpen = true;
        NotifyStateChanged();
        await Task.CompletedTask;
    }

    public void OpenAddServiceModal()
    {
        NewService = new ServiceOfferedViewModel { ActionType = (int)ServiceType.FileUpload };
        IsAddModalOpen = true;
        NotifyStateChanged();
    }

    public void CloseAddServiceModal()
    {
        IsAddModalOpen = false;
        NotifyStateChanged();
    }

    public void OpenEditServiceModal()
    {
        if (SelectedService == null) return;
        NewService = new ServiceOfferedViewModel
        {
            Id = SelectedService.Id,
            Name = SelectedService.Name,
            Description = SelectedService.Description,
            ActionType = SelectedService.ActionType,
            ImageUrl = SelectedService.ImageUrl,
            FormDefinitionJson = SelectedService.FormDefinitionJson,
            IsPublished = SelectedService.IsPublished,
            BudgetAccountId = SelectedService.BudgetAccountId,
            IsForMembership = SelectedService.IsForMembership,
            AcademicYearId = SelectedService.AcademicYearId,
        };
        IsEditModalOpen = true;
        NotifyStateChanged();
    }

    public void CloseEditServiceModal()
    {
        IsEditModalOpen = false;
        NotifyStateChanged();
    }

    public async Task HandleFileSelectedAsync(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file != null && file.ContentType.StartsWith("image/"))
        {
            try
            {
                var processFile = await file.RequestImageFileAsync(file.ContentType, 1280, 720);
                using var stream = processFile.OpenReadStream(50 * 1024 * 1024);
                using var memoryStream = new System.IO.MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var base64 = Convert.ToBase64String(memoryStream.ToArray());
                NewService.ImageUrl = $"data:{file.ContentType};base64,{base64}";
                NotifyStateChanged();
            }
            catch { }
        }
    }

    public async Task AddServiceAsync()
    {
        if (string.IsNullOrWhiteSpace(NewService.Name)) return;

        NewService.CreatedBy = CurrentSchoolId;
        NewService.CreatedByRole = CurrentRole;

        if (_offlineSync.IsOnline)
        {
            await _servicesService.CreateServiceOfferedAsync(NewService);
            await LoadServicesAsync();
            SuccessMessage = "Service created successfully.";
        }
        else
        {
            await _offlineSync.EnqueueAsync(new PendingOperation
            {
                Type = "CreateService",
                Payload = JsonSerializer.Serialize(NewService),
                UserId = CurrentSchoolId
            });
            SuccessMessage = "Service saved locally — will sync when back online.";
        }
        CloseAddServiceModal();
        ShowSuccess = true;
        ShouldReloadPage = true;
        NotifyStateChanged();
    }

    public async Task UpdateServiceAsync()
    {
        if (string.IsNullOrWhiteSpace(NewService.Name)) return;

        if (_offlineSync.IsOnline)
        {
            await _servicesService.UpdateServiceOfferedAsync(NewService.Id, NewService);
            await LoadServicesAsync();
            SelectedService = Services.FirstOrDefault(s => s.Id == NewService.Id);
            SuccessMessage = "Service updated successfully.";
        }
        else
        {
            await _offlineSync.EnqueueAsync(new PendingOperation
            {
                Type = "EditService",
                Payload = JsonSerializer.Serialize(NewService),
                UserId = CurrentSchoolId
            });
            SuccessMessage = "Service update saved locally — will sync when back online.";
        }
        CloseEditServiceModal();
        ShowSuccess = true;
        ShouldReloadPage = true;
        NotifyStateChanged();
    }

    public async Task TogglePublishStatusAsync()
    {
        if (SelectedService == null) return;

        var target = SelectedService;
        target.IsPublished = !target.IsPublished;
        NotifyStateChanged();

        var ok = await _servicesService.UpdateServiceOfferedAsync(target.Id, target);
        if (!ok)
        {
            target.IsPublished = !target.IsPublished;
            NotifyStateChanged();
            return;
        }
        SuccessMessage = "Service status updated.";
        ShowSuccess = true;
        NotifyStateChanged();
    }

    public async Task DeleteSelectedServiceAsync()
    {
        if (SelectedService == null) return;

        var toDelete = SelectedService;
        var idx = Services.IndexOf(toDelete);
        Services.Remove(toDelete);
        SelectedService = null;
        NotifyStateChanged();

        var ok = await _servicesService.DeleteServiceOfferedAsync(toDelete.Id);
        if (!ok)
        {
            Services.Insert(Math.Max(0, idx), toDelete);
            NotifyStateChanged();
            return;
        }
        SuccessMessage = "Service deleted successfully.";
        ShowSuccess = true;
        NotifyStateChanged();
    }

    public void SelectService(ServiceOfferedViewModel svc)
    {
        SelectedService = svc;
        RequestSearchTerm = "";
        SortOrder = "None";
        StatusFilter = "All";
        FileTypeFilter = "All";
        NotifyStateChanged();
    }

    public string GetFriendlyTypeName(int typeInt)
    {
        var type = (ServiceType)typeInt;
        return type switch
        {
            ServiceType.FileUpload => "File Processing",
            ServiceType.Payment => "Transactions",
            ServiceType.Inventory => "Inventory Orders",
            ServiceType.Appointment => "Appointments",
            ServiceType.Clearance => "Inquiries / Clearances",
            _ => type.ToString()
        };
    }

    public string GetStatusClass(string status)
    {
        return status switch
        {
            "Pending" => "bg-warning text-dark",
            "Printed" or "Verified" or "Completed" or "Confirmed" or "Cleared" => "bg-success",
            _ => "bg-secondary"
        };
    }
    
    public async Task ChangeRequestStatusAsync(ServiceRequest request, string newStatus)
    {
        var oldStatus = request.Status;
        request.Status = newStatus;
        NotifyStateChanged();

        var ok = await _servicesService.UpdateServiceRequestStatusAsync(request.Id, newStatus);
        if (!ok)
        {
            request.Status = oldStatus;
            NotifyStateChanged();
            return;
        }
        SuccessMessage = "Request status updated.";
        ShowSuccess = true;
        NotifyStateChanged();
    }

    // ── CSV Export ─────────────────────────────────────────────────────────────

    public string CsvExportStatus { get; set; } = "All";

    public string GenerateCsv(string statusFilter)
    {
        if (SelectedService == null || Requests == null) return "";

        var rows = Requests.Where(r => r.ServiceId == SelectedService.Id);
        if (statusFilter != "All")
            rows = rows.Where(r => r.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase));

        var allData = rows.Select(r => r.GetDynamicData()).ToList();

        // Collect all unique field keys (excluding raw file data)
        var excludedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "FileData", "FileApiUrl" };
        var dynamicKeys = allData
            .SelectMany(d => d.Keys)
            .Where(k => !excludedKeys.Contains(k))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var sb = new System.Text.StringBuilder();

        // Header row
        var headers = new List<string> { "Name", "Status" };
        headers.AddRange(dynamicKeys);
        sb.AppendLine(string.Join(",", headers.Select(CsvEscape)));

        // Data rows
        var rowList = rows.ToList();
        for (int i = 0; i < rowList.Count; i++)
        {
            var req = rowList[i];
            var data = allData[i];
            var cols = new List<string> { req.UserName, req.Status };
            foreach (var key in dynamicKeys)
                cols.Add(data.TryGetValue(key, out var val) ? val : "");
            sb.AppendLine(string.Join(",", cols.Select(CsvEscape)));
        }

        return sb.ToString();
    }

    private static string CsvEscape(string? value)
    {
        if (value == null) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }
}