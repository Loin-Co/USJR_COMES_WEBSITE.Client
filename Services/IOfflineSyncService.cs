namespace USJR_COMES_WEBSITE.Services;

/// <summary>
/// Manages a localStorage-backed queue of operations that couldn't be sent
/// to the server while offline. Processes the queue automatically when the
/// connection is restored.
/// </summary>
public interface IOfflineSyncService
{
    /// <summary>True if the browser reports an active network connection.</summary>
    bool IsOnline { get; }

    /// <summary>Number of operations waiting to be synced.</summary>
    int PendingCount { get; }

    /// <summary>True while the queue is being flushed to the server.</summary>
    bool IsSyncing { get; }

    /// <summary>UTC timestamp of the last successful full sync, or null if never synced.</summary>
    DateTime? LastSyncedAt { get; }

    /// <summary>Fired whenever IsOnline, PendingCount, IsSyncing, or LastSyncedAt changes.</summary>
    event Action OnChange;

    /// <summary>
    /// Queue an operation for deferred execution.
    /// Call this instead of the real API when IsOnline is false.
    /// </summary>
    Task EnqueueAsync(PendingOperation op);

    /// <summary>
    /// Attempt to flush all queued operations to the server.
    /// Called automatically when the browser comes back online.
    /// </summary>
    Task ProcessQueueAsync();

    /// <summary>
    /// Initialize the service: reads the persisted queue, registers JS event listeners,
    /// and restores the last-sync timestamp.
    /// </summary>
    Task InitializeAsync();
}
