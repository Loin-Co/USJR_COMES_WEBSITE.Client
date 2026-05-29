using System.Text.Json;
using USJR_COMES_WEBSITE.Services;

namespace USJR_COMES_WEBSITE.ViewModels;

public class ManagementPostsViewModel
{
    private readonly INewsFeedService _newsFeedService;
    private readonly UserStateService _userStateService;
    private readonly IOfflineSyncService _offlineSync;

    public List<NewsFeedPostViewModel>? Posts { get; private set; }
    public bool IsLoading { get; private set; } = true;

    public bool IsFormModalOpen { get; private set; }
    public bool IsEditing { get; private set; }
    public NewsFeedPostViewModel CurrentPost { get; set; } = new();
    public MultiPostRequestViewModel MultiPostRequest { get; set; } = new();

    public bool IsDetailModalOpen { get; private set; }
    public NewsFeedPostViewModel? SelectedPost { get; private set; }

    // Rejection modal
    public bool IsRejectModalOpen { get; private set; }
    public NewsFeedPostViewModel? RejectTarget { get; set; }
    public string RejectReason { get; set; } = string.Empty;

    // Success popup
    public bool ShowSuccess { get; private set; }
    public string SuccessMessage { get; private set; } = "Operation successful.";

    public void HideSuccess() { ShowSuccess = false; NotifyStateChanged(); }

    public event Action? OnChange;

    public ManagementPostsViewModel(INewsFeedService newsFeedService, UserStateService userStateService, IOfflineSyncService offlineSync)
    {
        _newsFeedService = newsFeedService;
        _userStateService = userStateService;
        _offlineSync = offlineSync;
    }

    public void Subscribe(Action handler) => OnChange += handler;
    public void Unsubscribe(Action handler) => OnChange -= handler;
    private void NotifyStateChanged() => OnChange?.Invoke();

    public string CurrentRole => _userStateService.CurrentUser?.AccountRole.ToString() ?? "NonMember";
    public string CurrentSchoolId => _userStateService.CurrentUser?.SchoolId ?? string.Empty;

    public async Task LoadPostsAsync()
    {
        IsLoading = true;
        NotifyStateChanged();

        var allPosts = await _newsFeedService.GetNewsFeedPostsAsync();
        var schoolId = _userStateService.CurrentUser?.SchoolId;
        var role = CurrentRole;

        if (role is "Adviser" or "DepartmentChairman" or "Admin")
        {
            // Advisers and dept chairs see everything
            Posts = allPosts ?? new();
        }
        else if (!string.IsNullOrEmpty(schoolId))
        {
            // Officers see only their own posts
            Posts = allPosts?.Where(p => string.Equals(p.SchoolId, schoolId, StringComparison.OrdinalIgnoreCase)).ToList() ?? new();
        }
        else
        {
            Posts = allPosts ?? new();
        }

        IsLoading = false;
        NotifyStateChanged();
    }

    public List<NewsFeedPostViewModel> PendingApprovalPosts =>
        Posts?.Where(p => p.ApprovalStatus == "PendingApproval").ToList() ?? new();

    public void OpenAddModal()
    {
        IsEditing = false;
        var userName = _userStateService.CurrentUser != null
            ? $"{_userStateService.CurrentUser.FirstName} {_userStateService.CurrentUser.LastName}".Trim()
            : "Unknown";
        var schoolId = _userStateService.CurrentUser?.SchoolId ?? string.Empty;
        var role = CurrentRole;

        MultiPostRequest = new MultiPostRequestViewModel
        {
            Name = userName,
            SchoolId = schoolId,
            CreatedByRole = role,
            Status = role == "DepartmentChairman" ? "Published" : "Unpublished",
            PostToNewsFeed = true
        };

        IsFormModalOpen = true;
        NotifyStateChanged();
    }

    public void OpenEditModal(NewsFeedPostViewModel post)
    {
        IsEditing = true;
        CurrentPost = new NewsFeedPostViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Description = post.Description,
            ImageUrl = post.ImageUrl,
            Status = post.Status,
            SchoolId = post.SchoolId,
            Name = post.Name,
            DateTime = post.DateTime,
            Likes = post.Likes,
            Comments = post.Comments,
            Shares = post.Shares,
            ApprovalStatus = post.ApprovalStatus,
            CreatedByRole = post.CreatedByRole,
            PostToNewsFeed = post.PostToNewsFeed,
            PostToHeadline = post.PostToHeadline,
            PostToSlideItem = post.PostToSlideItem
        };
        IsFormModalOpen = true;
        NotifyStateChanged();
    }

    public void CloseFormModal()
    {
        IsFormModalOpen = false;
        NotifyStateChanged();
    }

    public bool ShouldReloadPage { get; private set; }
    public void ClearReloadFlag() { ShouldReloadPage = false; }

    public async Task SavePostAsync()
    {
        if (_offlineSync.IsOnline)
        {
            if (IsEditing)
            {
                var ok = await _newsFeedService.UpdateNewsFeedPostAsync(CurrentPost);
                if (ok)
                {
                    // Directly patch the post in the list — no cache issues
                    var target = Posts?.FirstOrDefault(p => p.Id == CurrentPost.Id);
                    if (target != null)
                    {
                        target.Title = CurrentPost.Title;
                        target.Description = CurrentPost.Description;
                        target.Status = CurrentPost.Status;
                        target.PostToNewsFeed = CurrentPost.PostToNewsFeed;
                        target.PostToHeadline = CurrentPost.PostToHeadline;
                        target.PostToSlideItem = CurrentPost.PostToSlideItem;
                    }
                    SuccessMessage = "Post saved successfully.";
                }
                else
                {
                    SuccessMessage = "Failed to save post. Please try again.";
                }
            }
            else
            {
                if (CurrentRole != "DepartmentChairman")
                    MultiPostRequest.PostToNewsFeed = true;
                await _newsFeedService.CreateMultiPostAsync(MultiPostRequest);
                await LoadPostsAsync();
                SuccessMessage = "Post saved successfully.";
            }
            IsFormModalOpen = false;
            ShouldReloadPage = true;
        }
        else
        {
            // Offline — queue the operation for later sync
            if (IsEditing)
            {
                await _offlineSync.EnqueueAsync(new PendingOperation
                {
                    Type = "EditPost",
                    Payload = JsonSerializer.Serialize(CurrentPost),
                    UserId = CurrentSchoolId
                });
            }
            else
            {
                if (CurrentRole != "DepartmentChairman")
                    MultiPostRequest.PostToNewsFeed = true;
                await _offlineSync.EnqueueAsync(new PendingOperation
                {
                    Type = "CreatePost",
                    Payload = JsonSerializer.Serialize(MultiPostRequest),
                    UserId = CurrentSchoolId
                });
            }
            IsFormModalOpen = false;
            SuccessMessage = "Post saved locally — will sync when you're back online.";
        }

        ShowSuccess = true;
        NotifyStateChanged();
    }

    public async Task DeletePostAsync(NewsFeedPostViewModel post)
    {
        var idx = Posts?.IndexOf(post) ?? -1;
        Posts?.Remove(post);
        NotifyStateChanged();

        if (!_offlineSync.IsOnline)
        {
            await _offlineSync.EnqueueAsync(new PendingOperation
            {
                Type = "DeletePost",
                Payload = post.Id.ToString(),
                UserId = CurrentSchoolId
            });
            SuccessMessage = "Delete queued — will sync when back online.";
            ShowSuccess = true;
            NotifyStateChanged();
            return;
        }

        var ok = await _newsFeedService.DeleteNewsFeedPostAsync(post.Id);
        if (!ok)
        {
            if (idx >= 0) Posts?.Insert(idx, post);
            NotifyStateChanged();
            return;
        }
        SuccessMessage = "Post deleted successfully.";
        ShowSuccess = true;
        NotifyStateChanged();
    }

    public async Task ApprovePostAsync(NewsFeedPostViewModel post)
    {
        var oldApproval = post.ApprovalStatus;
        var oldStatus   = post.Status;
        post.ApprovalStatus = "Approved";
        post.Status = "Published";
        NotifyStateChanged();

        var ok = await _newsFeedService.ApprovePostAsync(post.Id, CurrentSchoolId);
        if (!ok)
        {
            post.ApprovalStatus = oldApproval;
            post.Status = oldStatus;
            NotifyStateChanged();
            return;
        }
        SuccessMessage = "Post approved successfully.";
        ShowSuccess = true;
        NotifyStateChanged();
    }

    public void OpenRejectModal(NewsFeedPostViewModel post)
    {
        RejectTarget = post;
        RejectReason = string.Empty;
        IsRejectModalOpen = true;
        NotifyStateChanged();
    }

    public void CloseRejectModal()
    {
        IsRejectModalOpen = false;
        RejectTarget = null;
        NotifyStateChanged();
    }

    public async Task ConfirmRejectAsync()
    {
        if (RejectTarget == null) return;
        var target = RejectTarget;
        var oldApproval = target.ApprovalStatus;
        target.ApprovalStatus = "Rejected";
        CloseRejectModal();
        NotifyStateChanged();

        var ok = await _newsFeedService.RejectPostAsync(target.Id, CurrentSchoolId, RejectReason);
        if (!ok)
        {
            target.ApprovalStatus = oldApproval;
            NotifyStateChanged();
            return;
        }
        SuccessMessage = "Post rejected.";
        ShowSuccess = true;
        NotifyStateChanged();
    }

    public void OpenDetailModal(NewsFeedPostViewModel post)
    {
        SelectedPost = post;
        IsDetailModalOpen = true;
        NotifyStateChanged();
    }

    public void CloseDetailModal()
    {
        IsDetailModalOpen = false;
        SelectedPost = null;
        NotifyStateChanged();
    }

    public string GetApprovalBadgeClass(string status) => status switch
    {
        "PendingApproval" => "bg-warning text-dark",
        "Approved" => "bg-success",
        "Rejected" => "bg-danger",
        _ => "bg-secondary"
    };

    public string GetApprovalLabel(string status) => status switch
    {
        "PendingApproval" => "Pending Approval",
        "Approved" => "Approved",
        "Rejected" => "Rejected",
        _ => "Draft"
    };
}
