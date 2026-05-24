namespace USJR_COMES_WEBSITE.APIs;

public static class ApiEndpoints
{
    public static string ServerBaseUrl { get; set; } = "https://localhost:7265";
    public static string ClientBaseUrl { get; set; } = "https://localhost:7118";

    public static class NewsFeed
    {
        public const string Base = "api/NewsFeedPosts";
        public const string MultiPost = "api/NewsFeedPosts/multipost";
        public static string WithId(int id) => $"api/NewsFeedPosts/{id}";
        public static string Approve(int id) => $"api/NewsFeedPosts/{id}/approve";
        public static string Reject(int id) => $"api/NewsFeedPosts/{id}/reject";
    }

    public static class UpcomingEvents
    {
        public const string Base = "api/UpcomingEvents";
        public static string WithId(int id) => $"api/UpcomingEvents/{id}";
        public static string Approve(int id) => $"api/UpcomingEvents/{id}/approve";
        public static string Reject(int id) => $"api/UpcomingEvents/{id}/reject";
    }

    public static class PostComments
    {
        public const string Base = "api/PostComments";
        public static string ForPost(int postId) => $"api/PostComments/{postId}";
    }

    public static class ServicesOffered
    {
        public const string Base = "api/ServicesOffered";
        public static string WithId(int id) => $"api/ServicesOffered/{id}";
        public static string Approve(int id) => $"api/ServicesOffered/{id}/approve";
        public static string Reject(int id) => $"api/ServicesOffered/{id}/reject";
    }

    public static class Budget
    {
        public const string Accounts     = "api/Budget/accounts";
        public const string Transactions = "api/Budget/transactions";
        public const string Pending      = "api/Budget/transactions/pending";
        public static string AccountWithId(int id) => $"api/Budget/accounts/{id}";
        public static string TransactionWithId(int id) => $"api/Budget/transactions/{id}";
        public static string AccountTransactions(int accountId) => $"api/Budget/accounts/{accountId}/transactions";
        public static string TransactionReceipt(int id) => $"{ServerBaseUrl}/api/Budget/transactions/{id}/receipt";
        public static string ApproveTransaction(int id) => $"api/Budget/transactions/{id}/approve";
        public static string RejectTransaction(int id)  => $"api/Budget/transactions/{id}/reject";
        public const string PendingAccounts = "api/Budget/accounts/pending";
        public static string ApproveAccount(int id) => $"api/Budget/accounts/{id}/approve";
        public static string RejectAccount(int id)  => $"api/Budget/accounts/{id}/reject";
    }

    public static class SiteSettings
    {
        public const string Base = "api/SiteSettings";
        public static string BackgroundImage => $"{ServerBaseUrl}/api/SiteSettings/background-image";
        public static string HeaderLogo => $"{ServerBaseUrl}/api/SiteSettings/header-logo";
    }

    public static class Users
    {
        public const string Base                  = "api/Users";
        public const string Login                 = "api/Users/login";
        public const string RequestPasswordSetup  = "api/Users/request-password-setup";
        public const string RequestPasswordReset  = "api/Users/request-password-reset";
        public const string CompletePasswordSetup = "api/Users/complete-password-setup";
        public const string ImportCsv             = "api/Users/import-csv";
        public static string WithId(string schoolId) => $"api/Users/{Uri.EscapeDataString(schoolId)}";
    }

    public static class Advisers
    {
        public const string Base   = "api/Advisers";
        public const string Appoint = "api/Advisers/appoint";
        public static string Remove(string schoolId) => $"api/Advisers/{Uri.EscapeDataString(schoolId)}";
    }

    public static class SiteChanges
    {
        public const string Propose  = "api/SiteChanges/propose";
        public const string Pending  = "api/SiteChanges/pending";
        public const string All      = "api/SiteChanges/all";
        public static string Approve(int id) => $"api/SiteChanges/{id}/approve";
        public static string Reject(int id)  => $"api/SiteChanges/{id}/reject";
    }

    public static class AboutUsChanges
    {
        public const string Propose  = "api/AboutUsChanges/propose";
        public const string Pending  = "api/AboutUsChanges/pending";
        public static string Approve(int id) => $"api/AboutUsChanges/{id}/approve";
        public static string Reject(int id)  => $"api/AboutUsChanges/{id}/reject";
        public static string Banner(int id)  => $"{ServerBaseUrl}/api/AboutUsChanges/{id}/banner";
    }

    public static class ServiceRequests
    {
        public const string Base       = "api/ServiceRequests";
        public const string Membership = "api/ServiceRequests/membership";
        public static string UpdateStatus(int id) => $"api/ServiceRequests/{id}/status";
    }

    public static class Membership
    {
        public const string Years         = "api/Membership/years";
        public const string Promote       = "api/Membership/promote";
        public static string SetCurrent(int id) => $"api/Membership/years/{id}/setcurrent";
        public static string UsersForYear(string year) => $"api/Membership/users/{Uri.EscapeDataString(year)}";
        public static string Revoke(string year, string schoolId) => $"api/Membership/revoke/{Uri.EscapeDataString(year)}/{Uri.EscapeDataString(schoolId)}";
        public static string Receipt(int id) => $"{ServerBaseUrl}/api/Membership/receipt/{id}";
    }

    public static class Files
    {
        public static string PostFile(int fileId) => $"{ServerBaseUrl}/api/files/postfile/{fileId}";
        public static string EventImage(int eventId) => $"{ServerBaseUrl}/api/files/event/{eventId}";
        public static string ServiceImage(int serviceId) => $"{ServerBaseUrl}/api/files/service/{serviceId}";
        public static string OfficerImage(int officerId) => $"{ServerBaseUrl}/api/files/officer/{officerId}";
        public static string SlideImage(int slideId) => $"{ServerBaseUrl}/api/files/slide/{slideId}";
        public static string ServiceRequestFile(int requestId) => $"{ServerBaseUrl}/api/files/servicerequest/{requestId}";
        public static string AboutUsBanner(int contentId) => $"{ServerBaseUrl}/api/files/aboutus/{contentId}";
        public static string UserAvatar(string schoolId) => $"{ServerBaseUrl}/api/files/user/{Uri.EscapeDataString(schoolId)}";

        // Converts a relative API path like "/api/files/event/5" to a full URL
        public static string ToFullUrl(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return "";
            if (relativePath.StartsWith("http")) return relativePath;
            if (relativePath.StartsWith("data:")) return relativePath;
            return ServerBaseUrl + relativePath;
        }
    }
}
