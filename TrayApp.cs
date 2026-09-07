using System.Diagnostics;

namespace ChatGPTRpc;

internal sealed class TrayApp : ApplicationContext
{
    private readonly NotifyIcon _tray;
    private readonly System.Windows.Forms.Timer _timer;
    private readonly DiscordPresence _presence = new();
    private AppConfig _config;
    private bool _wasActive;

    public TrayApp()
    {
        _config = ConfigStore.Load();

        var menu = new ContextMenuStrip();
        menu.Items.Add("ChatGPT RPC", null, (_, _) => OpenSettings()).Enabled = false;
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Settings", null, (_, _) => OpenSettings());
        menu.Items.Add("Open config folder", null, (_, _) => OpenConfigFolder());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Quit", null, (_, _) => ExitThread());

        _tray = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "ChatGPT RPC",
            Visible = true,
            ContextMenuStrip = menu
        };
        _tray.DoubleClick += (_, _) => OpenSettings();

        _presence.Configure(_config.ClientId);
        StartupManager.SetEnabled(_config.StartWithWindows);

        _timer = new System.Windows.Forms.Timer
        {
            Interval = Math.Clamp(_config.PollIntervalSeconds, 1, 30) * 1000
        };
        _timer.Tick += (_, _) => RefreshPresence();
        _timer.Start();

        if (string.IsNullOrWhiteSpace(_config.ClientId))
        {
            _tray.ShowBalloonTip(5000, "ChatGPT RPC", "Paste your Discord Application Client ID once to finish setup.", ToolTipIcon.Info);
            OpenSettings();
        }

        RefreshPresence();
    }

    private void RefreshPresence()
    {
        var activity = ActivityDetector.Detect();

        if (activity is null)
        {
            if (_wasActive)
                _presence.Clear();

            _wasActive = false;
            _tray.Text = "ChatGPT RPC · waiting for ChatGPT";
            return;
        }

        _wasActive = true;
        _presence.Update(activity, _config);
        _tray.Text = $"ChatGPT RPC · {activity.Mode}";
    }

    private void OpenSettings()
    {
        using var settings = new SettingsForm(_config);
        if (settings.ShowDialog() != DialogResult.OK)
            return;

        _config = settings.Config;
        _presence.Configure(_config.ClientId);
        _timer.Interval = Math.Clamp(_config.PollIntervalSeconds, 1, 30) * 1000;
        RefreshPresence();
    }

    private static void OpenConfigFolder()
    {
        Directory.CreateDirectory(ConfigStore.ConfigFolder);
        Process.Start(new ProcessStartInfo("explorer.exe", ConfigStore.ConfigFolder) { UseShellExecute = true });
    }

    protected override void ExitThreadCore()
    {
        _timer.Stop();
        _presence.Dispose();
        _tray.Visible = false;
        _tray.Dispose();
        base.ExitThreadCore();
    }
}
