using System.Diagnostics;

namespace ChatGPTRpc;

internal sealed class TrayApp : ApplicationContext
{
    private static readonly string[] Modes = ["Playing", "Watching", "Listening", "Competing"];

    private readonly NotifyIcon _tray;
    private readonly System.Windows.Forms.Timer _timer;
    private readonly DiscordPresence _presence = new();
    private readonly ToolStripMenuItem _dndItem;
    private readonly ToolStripMenuItem _startupItem;
    private readonly Dictionary<string, ToolStripMenuItem> _modeItems = new(StringComparer.OrdinalIgnoreCase);
    private AppConfig _config;

    public TrayApp()
    {
        _config = ConfigStore.Load();

        var menu = new ContextMenuStrip
        {
            BackColor = UiTheme.Surface,
            ForeColor = UiTheme.Text,
            Renderer = new ToolStripProfessionalRenderer()
        };

        var title = new ToolStripMenuItem("ChatGPT RPC") { Enabled = false };
        menu.Items.Add(title);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Settings", null, (_, _) => OpenSettings());

        _dndItem = new ToolStripMenuItem("Do Not Disturb");
        _dndItem.Click += (_, _) =>
        {
            _config.DoNotDisturb = !_config.DoNotDisturb;
            SaveConfig();
            RefreshPresence();
        };
        menu.Items.Add(_dndItem);

        _startupItem = new ToolStripMenuItem("Start on Windows");
        _startupItem.Click += (_, _) =>
        {
            _config.StartWithWindows = !_config.StartWithWindows;
            StartupManager.SetEnabled(_config.StartWithWindows);
            SaveConfig();
        };
        menu.Items.Add(_startupItem);
        menu.Items.Add(new ToolStripSeparator());

        var modeMenu = new ToolStripMenuItem("Mode");
        foreach (var mode in Modes)
        {
            var item = new ToolStripMenuItem(mode);
            item.Click += (_, _) => SetMode(mode);
            _modeItems[mode] = item;
            modeMenu.DropDownItems.Add(item);
        }
        menu.Items.Add(modeMenu);
        menu.Items.Add(new ToolStripSeparator());

        menu.Items.Add("Check for updates", null, (_, _) => CheckForUpdates());
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

        _timer = new System.Windows.Forms.Timer();
        _timer.Tick += (_, _) => RefreshPresence();

        ApplyConfig();
        _timer.Start();

        if (string.IsNullOrWhiteSpace(_config.ClientId))
        {
            _tray.ShowBalloonTip(
                5000,
                "ChatGPT RPC",
                "Open Settings and paste your Discord Application ID to finish setup.",
                ToolTipIcon.Info);
            OpenSettings();
        }

        RefreshPresence();
    }

    private void ApplyConfig()
    {
        _presence.Configure(_config.ClientId);
        StartupManager.SetEnabled(_config.StartWithWindows);
        _timer.Interval = Math.Clamp(_config.PollIntervalSeconds, 1, 30) * 1000;
        SyncMenu();
    }

    private void RefreshPresence()
    {
        if (_config.DoNotDisturb)
        {
            _presence.Clear();
            _tray.Text = "ChatGPT RPC · DND";
            return;
        }

        var activity = ActivityDetector.Detect();
        if (activity is null)
        {
            if (_config.IdlePresence)
            {
                _presence.UpdateIdle(_config);
                _tray.Text = "ChatGPT RPC · idle";
            }
            else
            {
                _presence.Clear();
                _tray.Text = "ChatGPT RPC · waiting for ChatGPT";
            }
            return;
        }

        _presence.Update(activity, _config);
        _tray.Text = $"ChatGPT RPC · {activity.Mode}";
    }

    private void OpenSettings()
    {
        using var settings = new SettingsForm(
            _config,
            () => _presence.Connected,
            applied =>
            {
                _config = applied;
                ApplyConfig();
                RefreshPresence();
            });

        settings.ShowDialog();
        SyncMenu();
    }

    private void SetMode(string mode)
    {
        _config.ActivityType = mode;
        SaveConfig();
        RefreshPresence();
    }

    private void SaveConfig()
    {
        ConfigStore.Save(_config);
        SyncMenu();
    }

    private void SyncMenu()
    {
        _dndItem.Checked = _config.DoNotDisturb;
        _startupItem.Checked = _config.StartWithWindows;

        foreach (var (mode, item) in _modeItems)
            item.Checked = mode.Equals(_config.ActivityType, StringComparison.OrdinalIgnoreCase);
    }

    private static void CheckForUpdates()
    {
        Process.Start(new ProcessStartInfo(
            "https://github.com/izzydizzyy/chatgpt-rpc/releases/latest")
        {
            UseShellExecute = true
        });
    }

    private static void OpenConfigFolder()
    {
        Directory.CreateDirectory(ConfigStore.ConfigFolder);
        Process.Start(new ProcessStartInfo("explorer.exe", ConfigStore.ConfigFolder)
        {
            UseShellExecute = true
        });
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
