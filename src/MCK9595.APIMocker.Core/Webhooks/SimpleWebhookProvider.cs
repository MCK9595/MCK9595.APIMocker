using System.Text;
using System.Text.Json;

namespace MCK9595.APIMocker.Core.Webhooks;

/// <summary>
/// Simple webhook provider that sends HTTP POST requests to registered URLs.
/// </summary>
public class SimpleWebhookProvider : IWebhookProvider
{
    private readonly List<WebhookConfig> _webhooks = new();
    private readonly HttpClient _httpClient;
    private readonly bool _verbose;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public SimpleWebhookProvider(bool verbose = false, int timeoutMs = 5000)
    {
        _verbose = verbose;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(timeoutMs)
        };
    }

    public int WebhookCount => _webhooks.Count;

    /// <summary>
    /// Registers a webhook configuration.
    /// </summary>
    public void Register(WebhookConfig config)
    {
        _webhooks.Add(config);
    }

    /// <summary>
    /// Loads webhook configurations from a JSON file.
    /// </summary>
    public void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Webhook config file not found: {filePath}");
        }

        var json = File.ReadAllText(filePath);
        var config = JsonSerializer.Deserialize<WebhookConfigFile>(json, _jsonOptions);

        if (config?.Webhooks != null)
        {
            foreach (var webhook in config.Webhooks)
            {
                Register(webhook);
            }
        }
    }

    public async Task FireAsync(string eventName, object? data)
    {
        var matchingWebhooks = _webhooks
            .Where(w => MatchesEvent(w.Event, eventName))
            .ToList();

        if (matchingWebhooks.Count == 0) return;

        var payload = new WebhookPayload
        {
            Event = eventName,
            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            Data = data
        };

        var jsonPayload = JsonSerializer.Serialize(payload, _jsonOptions);

        foreach (var webhook in matchingWebhooks)
        {
            _ = SendWebhookAsync(webhook, jsonPayload, eventName);
        }
    }

    private bool MatchesEvent(string pattern, string eventName)
    {
        // Support wildcards: "users.*" matches "users.created", "users.updated", etc.
        if (pattern.EndsWith(".*"))
        {
            var prefix = pattern[..^2];
            return eventName.StartsWith(prefix + ".", StringComparison.OrdinalIgnoreCase);
        }

        // Exact match
        return string.Equals(pattern, eventName, StringComparison.OrdinalIgnoreCase);
    }

    private async Task SendWebhookAsync(WebhookConfig webhook, string jsonPayload, string eventName)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, webhook.Url);
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Add custom headers
            if (webhook.Headers != null)
            {
                foreach (var (key, value) in webhook.Headers)
                {
                    request.Headers.TryAddWithoutValidation(key, value);
                }
            }

            var response = await _httpClient.SendAsync(request);

            if (_verbose)
            {
                var status = response.IsSuccessStatusCode ? "OK" : "FAILED";
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Webhook {status}: {eventName} -> {webhook.Url} ({(int)response.StatusCode})");
            }
        }
        catch (Exception ex)
        {
            if (_verbose)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Webhook ERROR: {eventName} -> {webhook.Url}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Creates a webhook provider from a config file path.
    /// </summary>
    public static SimpleWebhookProvider? FromFile(string? filePath, bool verbose = false)
    {
        if (string.IsNullOrEmpty(filePath)) return null;

        var provider = new SimpleWebhookProvider(verbose);
        provider.LoadFromFile(filePath);
        return provider;
    }
}
