using System.Text.Json.Serialization;

namespace USJR_COMES_WEBSITE.Services;

/// <summary>
/// A write operation that was queued while the user was offline.
/// Serialised to/from localStorage via the offlineSync JS module.
/// </summary>
public class PendingOperation
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Identifies the operation type so the sync processor knows which API to call.
    /// Values: "CreatePost", "EditPost", "DeletePost",
    ///         "CreateEvent", "EditEvent", "DeleteEvent",
    ///         "CreateService", "EditService"
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>JSON-serialised request payload.</summary>
    [JsonPropertyName("payload")]
    public string Payload { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("retries")]
    public int Retries { get; set; }
}
