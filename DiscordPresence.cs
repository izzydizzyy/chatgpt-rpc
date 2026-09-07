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

        _client = new DiscordRpcClient(clientId.Trim());
        _client.Initialize();
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

        var state = config.ShowSessionTitle && !string.IsNullOrWhiteSpace(activity.Title)
            ? activity.Title
            : activity.Mode == "Codex" ? "Working with Codex" : "Using ChatGPT";

        _client.SetPresence(new RichPresence
        {
            Details = activity.Mode == "Codex" ? "Codex · Coding" : "ChatGPT · Chat",
            State = state,
            Timestamps = new Timestamps(_sessionStart ?? DateTime.UtcNow),
            Assets = new Assets
            {
                LargeImageKey = config.LargeImageKey,
                LargeImageText = config.LargeImageText
            }
        });
    }

    public void Clear()
    {
        _sessionStart = null;
        _lastMode = null;
        _client?.ClearPresence();
    }

    public void Dispose()
    {
        DisposeClient();
        GC.SuppressFinalize(this);
    }

    private void DisposeClient()
    {
        if (_client is null)
            return;

        try { _client.ClearPresence(); } catch { }
        try { _client.Dispose(); } catch { }
        _client = null;
    }
}
