using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using USJR_COMES_WEBSITE.APIs;
using USJR_COMES_WEBSITE.ViewModels;

namespace USJR_COMES_WEBSITE.Services;

/// <summary>
/// Manages an offline-first write queue backed by localStorage.
/// When the user submits a form while offline the operation is enqueued here.
/// On reconnect ProcessQueueAsync replays all pending operations against the API.
/// </summary>
public class OfflineSyncService : IOfflineSyncService, IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly IHttpClientFactory _httpClientFactory;
    private DotNetObjectReference<OfflineSyncService>? _selfRef;

    private List<PendingOperation> _queue = new();

    public bool IsOnline { get; private set; } = true;
    public int PendingCount => _queue.Count;
    public bool IsSyncing { get; private set; }
    public DateTime? LastSyncedAt { get; private set; }

    public event Action? OnChange;

    public OfflineSyncService(IJSRuntime js, IHttpClientFactory httpClientFactory)
    {
        _js = js;
        _httpClientFactory = httpClientFactory;
    }

    public async Task InitializeAsync()
    {
        // Read existing queue from localStorage
        var raw = await SafeInvokeAsync<JsonElement?>("offlineSync.getPendingOps");
        if (raw.HasValue && raw.Value.ValueKind == JsonValueKind.Array)
        {
            _queue = JsonSerializer.Deserialize<List<PendingOperation>>(raw.Value.GetRawText()) ?? new();
        }

        // Read last sync time
        var lastSync = await SafeInvokeAsync<string>("offlineSync.loadLastSync");
        if (!string.IsNullOrEmpty(lastSync) && DateTime.TryParse(lastSync, out var dt))
            LastSyncedAt = dt;

        // Get current online state
        IsOnline = await SafeInvokeAsync<bool>("offlineSync.isOnline");

        // Register online/offline event callbacks
        _selfRef = DotNetObjectReference.Create(this);
        await SafeInvokeVoidAsync("offlineSync.registerOnlineHandler", _selfRef);

        NotifyChange();
    }

    public async Task EnqueueAsync(PendingOperation op)
    {
        _queue.Add(op);
        await PersistQueueAsync();
        NotifyChange();
    }

    public async Task ProcessQueueAsync()
    {
        if (_queue.Count == 0 || IsSyncing) return;

        IsSyncing = true;
        NotifyChange();

        var http = _httpClientFactory.CreateClient("api");
        var processed = new List<string>();

        foreach (var op in _queue.ToList())
        {
            try
            {
                bool success = op.Type switch
                {
                    "CreatePost"    => await ReplayCreatePostAsync(http, op),
                    "EditPost"      => await ReplayEditPostAsync(http, op),
                    "DeletePost"    => await ReplayDeletePostAsync(http, op),
                    "CreateEvent"   => await ReplayCreateEventAsync(http, op),
                    "EditEvent"     => await ReplayEditEventAsync(http, op),
                    "DeleteEvent"   => await ReplayDeleteEventAsync(http, op),
                    "CreateService" => await ReplayCreateServiceAsync(http, op),
                    "EditService"   => await ReplayEditServiceAsync(http, op),
                    _ => true // unknown types — remove silently
                };

                if (success)
                {
                    processed.Add(op.Id);
                    await SafeInvokeVoidAsync("offlineSync.dequeue", op.Id);
                }
                else
                {
                    op.Retries++;
                    if (op.Retries >= 5)
                        processed.Add(op.Id); // give up after 5 retries
                }
            }
            catch
            {
                op.Retries++;
                if (op.Retries >= 5)
                    processed.Add(op.Id);
            }
        }

        _queue.RemoveAll(o => processed.Contains(o.Id));
        await PersistQueueAsync();

        IsSyncing = false;
        LastSyncedAt = DateTime.UtcNow;
        await SafeInvokeVoidAsync("offlineSync.saveLastSync", LastSyncedAt.Value.ToString("O"));
        NotifyChange();
    }

    // ── Replay helpers ────────────────────────────────────────────────────────

    private async Task<bool> ReplayCreatePostAsync(HttpClient http, PendingOperation op)
    {
        var req = JsonSerializer.Deserialize<MultiPostRequestViewModel>(op.Payload);
        if (req == null) return false;
        var resp = await http.PostAsJsonAsync(ApiEndpoints.NewsFeed.MultiPost, req);
        return resp.IsSuccessStatusCode;
    }

    private async Task<bool> ReplayEditPostAsync(HttpClient http, PendingOperation op)
    {
        var req = JsonSerializer.Deserialize<NewsFeedPostViewModel>(op.Payload);
        if (req == null) return false;
        var resp = await http.PutAsJsonAsync(ApiEndpoints.NewsFeed.WithId(req.Id), req);
        return resp.IsSuccessStatusCode;
    }

    private async Task<bool> ReplayDeletePostAsync(HttpClient http, PendingOperation op)
    {
        if (!int.TryParse(op.Payload, out var id)) return false;
        var resp = await http.DeleteAsync(ApiEndpoints.NewsFeed.WithId(id));
        return resp.IsSuccessStatusCode;
    }

    private async Task<bool> ReplayCreateEventAsync(HttpClient http, PendingOperation op)
    {
        var req = JsonSerializer.Deserialize<UpcomingEventViewModel>(op.Payload);
        if (req == null) return false;
        var resp = await http.PostAsJsonAsync(ApiEndpoints.UpcomingEvents.Base, req);
        return resp.IsSuccessStatusCode;
    }

    private async Task<bool> ReplayEditEventAsync(HttpClient http, PendingOperation op)
    {
        var req = JsonSerializer.Deserialize<UpcomingEventViewModel>(op.Payload);
        if (req == null) return false;
        var resp = await http.PutAsJsonAsync(ApiEndpoints.UpcomingEvents.WithId(req.Id), req);
        return resp.IsSuccessStatusCode;
    }

    private async Task<bool> ReplayDeleteEventAsync(HttpClient http, PendingOperation op)
    {
        if (!int.TryParse(op.Payload, out var id)) return false;
        var resp = await http.DeleteAsync(ApiEndpoints.UpcomingEvents.WithId(id));
        return resp.IsSuccessStatusCode;
    }

    private async Task<bool> ReplayCreateServiceAsync(HttpClient http, PendingOperation op)
    {
        var req = JsonSerializer.Deserialize<ServiceOfferedViewModel>(op.Payload);
        if (req == null) return false;
        var resp = await http.PostAsJsonAsync(ApiEndpoints.ServicesOffered.Base, req);
        return resp.IsSuccessStatusCode;
    }

    private async Task<bool> ReplayEditServiceAsync(HttpClient http, PendingOperation op)
    {
        var req = JsonSerializer.Deserialize<ServiceOfferedViewModel>(op.Payload);
        if (req == null) return false;
        var resp = await http.PutAsJsonAsync(ApiEndpoints.ServicesOffered.WithId(req.Id), req);
        return resp.IsSuccessStatusCode;
    }

    // ── JS interop callbacks (called by JS when online/offline events fire) ───

    [JSInvokable]
    public async Task OnBrowserOnline()
    {
        IsOnline = true;
        NotifyChange();
        // Auto-process queue when connection is restored
        await ProcessQueueAsync();
    }

    [JSInvokable]
    public void OnBrowserOffline()
    {
        IsOnline = false;
        NotifyChange();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task PersistQueueAsync()
    {
        var json = JsonSerializer.Serialize(_queue);
        await SafeInvokeVoidAsync("offlineSync.setPendingOps", _queue);
    }

    private void NotifyChange() => OnChange?.Invoke();

    private async Task<T> SafeInvokeAsync<T>(string identifier, params object?[]? args)
    {
        try { return await _js.InvokeAsync<T>(identifier, args); }
        catch { return default!; }
    }

    private async Task SafeInvokeVoidAsync(string identifier, params object?[]? args)
    {
        try { await _js.InvokeVoidAsync(identifier, args); }
        catch { }
    }

    public async ValueTask DisposeAsync()
    {
        try { await SafeInvokeVoidAsync("offlineSync.dispose"); }
        catch { }
        _selfRef?.Dispose();
    }
}
