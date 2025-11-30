namespace MCK9595.APIMocker.Core.Webhooks;

/// <summary>
/// Webhook event types.
/// </summary>
public static class WebhookEvents
{
    public static string Created(string collection) => $"{collection}.created";
    public static string Updated(string collection) => $"{collection}.updated";
    public static string Deleted(string collection) => $"{collection}.deleted";
}

/// <summary>
/// Webhook configuration.
/// </summary>
public class WebhookConfig
{
    public string Event { get; set; } = "";
    public string Url { get; set; } = "";
    public Dictionary<string, string>? Headers { get; set; }
}

/// <summary>
/// Webhook configuration file format.
/// </summary>
public class WebhookConfigFile
{
    public List<WebhookConfig> Webhooks { get; set; } = new();
}

/// <summary>
/// Webhook payload sent to registered URLs.
/// </summary>
public class WebhookPayload
{
    public string Event { get; set; } = "";
    public string Timestamp { get; set; } = "";
    public object? Data { get; set; }
}

/// <summary>
/// Interface for webhook providers.
/// </summary>
public interface IWebhookProvider
{
    /// <summary>
    /// Fires a webhook event asynchronously.
    /// </summary>
    Task FireAsync(string eventName, object? data);

    /// <summary>
    /// Gets the count of registered webhooks.
    /// </summary>
    int WebhookCount { get; }
}
