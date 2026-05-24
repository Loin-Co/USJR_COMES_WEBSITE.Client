using System.Net.Http.Json;
using USJR_COMES_WEBSITE.APIs;
using USJR_COMES_WEBSITE.ViewModels;

namespace USJR_COMES_WEBSITE.Services;

public class BudgetService : IBudgetService
{
    private readonly HttpClient _http;
    private readonly Dictionary<string, CacheEntry> _cache = new();
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    private class CacheEntry
    {
        public object Data { get; set; }
        public DateTime ExpirationTime { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpirationTime;
    }

    public BudgetService(HttpClient http)
    {
        _http = http;
    }

    private async Task<T> GetFromCacheOrFetchAsync<T>(string cacheKey, Func<Task<T>> fetchFunc)
    {
        if (_cache.TryGetValue(cacheKey, out var entry) && !entry.IsExpired)
            return (T)entry.Data;

        try
        {
            var result = await fetchFunc();
            _cache[cacheKey] = new CacheEntry { Data = result!, ExpirationTime = DateTime.UtcNow.Add(_cacheExpiration) };
            return result;
        }
        catch
        {
            return entry != null ? (T)entry.Data : default!;
        }
    }

    private void InvalidateCache(params string[] keys)
    {
        foreach (var key in keys)
        {
            if (key.EndsWith("*"))
            {
                var prefix = key[..^1];
                var toRemove = _cache.Keys.Where(k => k.StartsWith(prefix)).ToList();
                foreach (var k in toRemove) _cache.Remove(k);
            }
            else
            {
                _cache.Remove(key);
            }
        }
    }

    // ── Academic Years ────────────────────────────────────────────────────────

    public async Task<List<AcademicYearViewModel>> GetYearsAsync()
    {
        return await GetFromCacheOrFetchAsync(
            "budget_years",
            async () =>
            {
                try { return await _http.GetFromJsonAsync<List<AcademicYearViewModel>>("api/Budget/years") ?? new(); }
                catch { return new(); }
            }
        );
    }

    public async Task<AcademicYearViewModel?> CreateYearAsync(AcademicYearViewModel year)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/Budget/years", year);
            if (res.IsSuccessStatusCode)
            {
                InvalidateCache("budget_years");
                return await res.Content.ReadFromJsonAsync<AcademicYearViewModel>();
            }
            return null;
        }
        catch { return null; }
    }

    public async Task<(int toYearId, decimal transferredAmount)?> TransferBudgetAsync(TransferBudgetViewModel request)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/Budget/years/transfer", request);
            if (!res.IsSuccessStatusCode) return null;
            InvalidateCache("budget_years");
            var result = await res.Content.ReadFromJsonAsync<TransferResult>();
            return result is null ? null : (result.ToYearId, result.TransferredAmount);
        }
        catch { return null; }
    }

    // ── Accounts ──────────────────────────────────────────────────────────────

    public async Task<List<BudgetAccountViewModel>> GetAccountsAsync(int? yearId = null)
    {
        var cacheKey = yearId.HasValue ? $"budget_accounts_year_{yearId}" : "budget_accounts_all";
        return await GetFromCacheOrFetchAsync(
            cacheKey,
            async () =>
            {
                try
                {
                    var url = yearId.HasValue
                        ? $"{ApiEndpoints.Budget.Accounts}?yearId={yearId}"
                        : ApiEndpoints.Budget.Accounts;
                    return await _http.GetFromJsonAsync<List<BudgetAccountViewModel>>(url) ?? new();
                }
                catch { return new(); }
            }
        );
    }

    public async Task<List<BudgetAccountViewModel>> GetPendingAccountsAsync()
    {
        try { return await _http.GetFromJsonAsync<List<BudgetAccountViewModel>>(ApiEndpoints.Budget.PendingAccounts) ?? new(); }
        catch { return new(); }
    }

    public async Task<bool> ApproveAccountAsync(int id, string adviserId)
    {
        try
        {
            var res = await _http.PutAsJsonAsync(ApiEndpoints.Budget.ApproveAccount(id), new { ReviewedBySchoolId = adviserId });
            if (res.IsSuccessStatusCode) InvalidateCache("budget_accounts_all", "budget_accounts_year_*");
            return res.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> RejectAccountAsync(int id, string adviserId, string reason)
    {
        try
        {
            var res = await _http.PutAsJsonAsync(ApiEndpoints.Budget.RejectAccount(id), new { ReviewedBySchoolId = adviserId, RejectionReason = reason });
            return res.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<BudgetAccountViewModel?> CreateAccountAsync(BudgetAccountViewModel account)
    {
        try
        {
            var res = await _http.PostAsJsonAsync(ApiEndpoints.Budget.Accounts, account);
            if (res.IsSuccessStatusCode)
            {
                InvalidateCache("budget_accounts_all", "budget_accounts_year_*");
                return await res.Content.ReadFromJsonAsync<BudgetAccountViewModel>();
            }
            return null;
        }
        catch { return null; }
    }

    public async Task<bool> UpdateAccountAsync(BudgetAccountViewModel account)
    {
        try 
        { 
            var result = (await _http.PutAsJsonAsync(ApiEndpoints.Budget.AccountWithId(account.Id), account)).IsSuccessStatusCode;
            if (result) InvalidateCache("budget_accounts_all", "budget_accounts_year_*");
            return result;
        }
        catch { return false; }
    }

    public async Task<bool> DeleteAccountAsync(int id)
    {
        try 
        { 
            var result = (await _http.DeleteAsync(ApiEndpoints.Budget.AccountWithId(id))).IsSuccessStatusCode;
            if (result) InvalidateCache("budget_accounts_all", "budget_accounts_year_*");
            return result;
        }
        catch { return false; }
    }

    // ── Transactions ──────────────────────────────────────────────────────────

    public async Task<BudgetTransactionViewModel?> CreateTransactionAsync(BudgetTransactionViewModel transaction)
    {
        try
        {
            var res = await _http.PostAsJsonAsync(ApiEndpoints.Budget.Transactions, transaction);
            if (res.IsSuccessStatusCode)
            {
                InvalidateCache("budget_accounts_all", "budget_accounts_year_*");
                return await res.Content.ReadFromJsonAsync<BudgetTransactionViewModel>();
            }
            return null;
        }
        catch { return null; }
    }

    public async Task<bool> UpdateTransactionAsync(BudgetTransactionViewModel transaction)
    {
        try 
        { 
            var result = (await _http.PutAsJsonAsync(ApiEndpoints.Budget.TransactionWithId(transaction.Id), transaction)).IsSuccessStatusCode;
            if (result) InvalidateCache("budget_accounts_all", "budget_accounts_year_*");
            return result;
        }
        catch { return false; }
    }

    public async Task<bool> DeleteTransactionAsync(int id)
    {
        try 
        { 
            var result = (await _http.DeleteAsync(ApiEndpoints.Budget.TransactionWithId(id))).IsSuccessStatusCode;
            if (result) InvalidateCache("budget_accounts_all", "budget_accounts_year_*");
            return result;
        }
        catch { return false; }
    }

    private class TransferResult
    {
        public int ToYearId { get; set; }
        public decimal TransferredAmount { get; set; }
    }
}
