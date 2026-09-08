using DiscordRPC;

namespace ChatGPTRpc;

internal sealed class DiscordPresence : IDisposable
{
    private DiscordRpcClient? _client;
    private DateTime? _sessionStart;
    private string? _lastMode;

    public bool Connected => _client?.IsInitialized == true;

    public void Configure(string clientId)
    {
        DisposeClient();

        if (string.IsNullOrWhiteSpace(clientId))
            return;

        try
        {
            _client = new DiscordRpcClient(clientId.Trim());
            _client.Initialize();
        }
        catch
        {
            DisposeClient();
        }
    }

    public void Update(DetectedActivity activity, AppConfig config)
    {
        if (_client is null || !_client.IsInitialized)
            return;

        if (!string.Equals(_lastMode, activity.Mode, StringComparison.Ordinal))
        {
            _sessionStart = DateTime.UtcNow;
            _lastMode = activity.Mode;
        }

        var fallback = activity.Mode == "Codex" ? "Working with Codex" : "Using ChatGPT";
        var state = config.ShowSessionTitle && !string.IsNullOrWhiteSpace(activity.Title)
            ? activity.Title
            : fallback;

        _client.SetPresence(new RichPresence
        {
            Type = ParseActivityType(config.ActivityType),
            Details = activity.Mode == "Codex" ? "Codex · Coding" : "ChatGPT · Chat",
            State = state,
            Timestamps = config.ShowElapsedTime
                ? new Timestamps(_sessionStart ?? DateTime.UtcNow)
                : null,
            Assets = BuildAssets(config)
        });
    }

    public void UpdateIdle(AppConfig config)
    {
        if (_client is null || !_client.IsInitialized)
            return;

        _sessionStart = null;
        _lastMode = "Idle";

        _client.SetPresence(new RichPresence
        {
            Type = ParseActivityType(config.ActivityType),
            Details = "ChatGPT RPC",
            State = "Waiting for ChatGPT / Codex",
            Assets = BuildAssets(config)
        });
    }

    public void Clear()
    {
        _sessionStart = null;
        _lastMode = null;
        try { _client?.ClearPresence(); } catch { }
    }

    public void Dispose()
    {
        DisposeClient();
        GC.SuppressFinalize(this);
    }

    private static Assets? BuildAssets(AppConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.LargeImageKey))
            return null;

        return new Assets
        {
            LargeImageKey = config.LargeImageKey.Trim(),
            LargeImageText = string.IsNullOrWhiteSpace(config.LargeImageText)
                ? "ChatGPT RPC"
                : config.LargeImageText.Trim()
        };
    }

    private static ActivityType ParseActivityType(string value) => value switch
    {
        "Watching" => ActivityType.Watching,
        "Listening" => ActivityType.Listening,
        "Competing" => ActivityType.Competing,
        _ => ActivityType.Playing
    };

    private void DisposeClient()
    {
        if (_client is null)
            return;

        try { _client.ClearPresence(); } catch { }
        try { _client.Dispose(); } catch { }
        _client = null;
    }
}
