namespace ChatGPTRpc;

internal sealed class SettingsForm : Form
{
    private readonly AppConfig _config;
    private readonly Func<bool> _isDiscordConnected;
    private readonly Action<AppConfig> _onApplied;

    private readonly TextBox _clientId;
    private readonly TextBox _assetKey;
    private readonly ComboBox _mode;
    private readonly CheckBox _dnd;
    private readonly CheckBox _idle;
    private readonly CheckBox _showTitle;
    private readonly CheckBox _elapsed;
    private readonly CheckBox _startup;
    private readonly Label _discordStatus;
    private readonly Label _detectStatus;
    private readonly Label _previewMode;
    private readonly Label _previewDetails;
    private readonly Label _previewState;
    private readonly Label _applied;
    private readonly System.Windows.Forms.Timer _previewTimer;

    public SettingsForm(AppConfig config, Func<bool> isDiscordConnected, Action<AppConfig> onApplied)
    {
        _config = config;
        _isDiscordConnected = isDiscordConnected;
        _onApplied = onApplied;

        Text = "ChatGPT RPC";
        Width = 760;
        Height = 780;
        MinimumSize = new Size(650, 620);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.Text;
        Font = new Font("Segoe UI", 9.5f);

        var header = new Panel { Dock = DockStyle.Top, Height = 82, BackColor = UiTheme.Background };
        header.Controls.Add(new Label
        {
            Text = "ChatGPT RPC",
            Font = new Font("Segoe UI Semibold", 18f),
            ForeColor = UiTheme.Text,
            AutoSize = true,
            Location = new Point(24, 18)
        });
        header.Controls.Add(new Label
        {
            Text = "Discord presence for ChatGPT and Codex · v0.2.0",
            ForeColor = UiTheme.Muted,
            AutoSize = true,
            Location = new Point(27, 50)
        });

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 70, BackColor = UiTheme.Background };
        var apply = Button("Apply", 94);
        apply.Anchor = AnchorStyles.Right | AnchorStyles.Top;
        apply.Location = new Point(Width - 230, 18);
        apply.Click += (_, _) => Apply();

        var close = Button("Close", 94);
        close.Anchor = AnchorStyles.Right | AnchorStyles.Top;
        close.Location = new Point(Width - 126, 18);
        close.Click += (_, _) => Close();

        _applied = new Label
        {
            Text = "",
            ForeColor = UiTheme.Accent,
            AutoSize = true,
            Location = new Point(24, 27)
        };

        footer.Controls.Add(_applied);
        footer.Controls.Add(apply);
        footer.Controls.Add(close);

        var main = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(22, 4, 22, 20),
            BackColor = UiTheme.Background
        };

        _discordStatus = StatusLabel("○ Discord waiting", false);
        _detectStatus = StatusLabel("○ ChatGPT / Codex not detected", false);
        var statusCard = Card("Status", 116);
        _discordStatus.Location = new Point(20, 47);
        _detectStatus.Location = new Point(20, 75);
        statusCard.Controls.Add(_discordStatus);
        statusCard.Controls.Add(_detectStatus);
        main.Controls.Add(statusCard);

        _mode = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat,
            BackColor = UiTheme.SurfaceHover,
            ForeColor = UiTheme.Text,
            Width = 210,
            Location = new Point(150, 48)
        };
        _mode.Items.AddRange(["Playing", "Watching", "Listening", "Competing"]);
        _mode.SelectedItem = new[] { "Playing", "Watching", "Listening", "Competing" }
            .Contains(config.ActivityType) ? config.ActivityType : "Playing";
        _mode.SelectedIndexChanged += (_, _) => RefreshPreview();

        _dnd = Toggle("DND", config.DoNotDisturb);
        _idle = Toggle("Idle presence", config.IdlePresence);
        _dnd.Location = new Point(150, 90);
        _idle.Location = new Point(290, 90);

        var presenceCard = Card("Presence", 144);
        presenceCard.Controls.Add(Label("RPC mode", 20, 51));
        presenceCard.Controls.Add(_mode);
        presenceCard.Controls.Add(_dnd);
        presenceCard.Controls.Add(_idle);
        main.Controls.Add(presenceCard);

        _showTitle = Toggle("Session title", config.ShowSessionTitle);
        _elapsed = Toggle("Elapsed time", config.ShowElapsedTime);
        _showTitle.Location = new Point(20, 52);
        _elapsed.Location = new Point(160, 52);

        var infoCard = Card("RPC info", 108);
        infoCard.Controls.Add(_showTitle);
        infoCard.Controls.Add(_elapsed);
        main.Controls.Add(infoCard);

        _clientId = TextBox(config.ClientId, "Discord Application ID");
        _clientId.Location = new Point(20, 76);
        _clientId.Width = 460;

        _assetKey = TextBox(config.LargeImageKey, "Large image asset key");
        _assetKey.Location = new Point(20, 142);
        _assetKey.Width = 300;

        _startup = Toggle("Start on Windows", config.StartWithWindows);
        _startup.Location = new Point(20, 188);

        var setupCard = Card("Setup", 242);
        setupCard.Controls.Add(Label("Discord Application ID", 20, 50, UiTheme.Muted));
        setupCard.Controls.Add(_clientId);
        setupCard.Controls.Add(Label("Image asset key (optional)", 20, 116, UiTheme.Muted));
        setupCard.Controls.Add(_assetKey);
        setupCard.Controls.Add(_startup);
        main.Controls.Add(setupCard);

        _previewMode = new Label
        {
            Text = "PLAYING",
            ForeColor = UiTheme.Muted,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 8.5f),
            Location = new Point(20, 48)
        };
        _previewDetails = new Label
        {
            Text = "ChatGPT · Chat",
            ForeColor = UiTheme.Text,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 11f),
            Location = new Point(20, 78)
        };
        _previewState = new Label
        {
            Text = "Using ChatGPT",
            ForeColor = UiTheme.Muted,
            AutoEllipsis = true,
            Width = 580,
            Height = 24,
            Location = new Point(20, 105)
        };

        var previewCard = Card("Preview", 150);
        previewCard.Controls.Add(_previewMode);
        previewCard.Controls.Add(_previewDetails);
        previewCard.Controls.Add(_previewState);
        main.Controls.Add(previewCard);

        main.SizeChanged += (_, _) =>
        {
            var width = Math.Max(560, main.ClientSize.Width - 52);
            foreach (Control control in main.Controls)
                control.Width = width;
        };

        Controls.Add(main);
        Controls.Add(footer);
        Controls.Add(header);

        _previewTimer = new System.Windows.Forms.Timer { Interval = 1200 };
        _previewTimer.Tick += (_, _) => RefreshStatus();
        _previewTimer.Start();
        FormClosed += (_, _) => _previewTimer.Stop();

        RefreshStatus();
        RefreshPreview();
    }

    private Panel Card(string title, int height)
    {
        var card = new Panel
        {
            Width = 680,
            Height = height,
            BackColor = UiTheme.Surface,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0, 0, 0, 12)
        };
        card.Controls.Add(new Label
        {
            Text = title,
            ForeColor = UiTheme.Text,
            Font = new Font("Segoe UI Semibold", 10.5f),
            AutoSize = true,
            Location = new Point(18, 16)
        });
        return card;
    }

    private CheckBox Toggle(string name, bool value)
    {
        var toggle = new CheckBox
        {
            Tag = name,
            Checked = value,
            Appearance = Appearance.Button,
            AutoSize = false,
            Width = 126,
            Height = 32,
            FlatStyle = FlatStyle.Flat,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = UiTheme.Text,
            Cursor = Cursors.Hand
        };
        toggle.FlatAppearance.BorderColor = UiTheme.Border;
        toggle.CheckedChanged += (_, _) =>
        {
            PaintToggle(toggle);
            RefreshPreview();
        };
        PaintToggle(toggle);
        return toggle;
    }

    private static void PaintToggle(CheckBox toggle)
    {
        var name = toggle.Tag?.ToString() ?? "Option";
        toggle.Text = $"{name}: {(toggle.Checked ? "on" : "off")}";
        toggle.BackColor = toggle.Checked ? UiTheme.Accent : UiTheme.SurfaceHover;
        toggle.ForeColor = toggle.Checked ? Color.Black : UiTheme.Text;
    }

    private static TextBox TextBox(string value, string placeholder)
    {
        return new TextBox
        {
            Text = value,
            PlaceholderText = placeholder,
            BackColor = UiTheme.SurfaceHover,
            ForeColor = UiTheme.Text,
            BorderStyle = BorderStyle.FixedSingle,
            Height = 30
        };
    }

    private static Button Button(string text, int width)
    {
        var button = new Button
        {
            Text = text,
            Width = width,
            Height = 36,
            BackColor = UiTheme.SurfaceHover,
            ForeColor = UiTheme.Text,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderColor = UiTheme.Border;
        return button;
    }

    private static Label Label(string text, int x, int y, Color? color = null)
    {
        return new Label
        {
            Text = text,
            ForeColor = color ?? UiTheme.Text,
            AutoSize = true,
            Location = new Point(x, y)
        };
    }

    private static Label StatusLabel(string text, bool active)
    {
        return new Label
        {
            Text = text,
            ForeColor = active ? UiTheme.Accent : UiTheme.Muted,
            AutoSize = true
        };
    }

    private void RefreshStatus()
    {
        var connected = _isDiscordConnected();
        _discordStatus.Text = connected ? "● Discord connected" : "○ Discord waiting";
        _discordStatus.ForeColor = connected ? UiTheme.Accent : UiTheme.Muted;

        var activity = ActivityDetector.Detect();
        var detected = activity is not null;
        _detectStatus.Text = detected
            ? $"● {activity!.Mode} detected"
            : "○ ChatGPT / Codex not detected";
        _detectStatus.ForeColor = detected ? UiTheme.Accent : UiTheme.Muted;

        RefreshPreview(activity);
    }

    private void RefreshPreview(DetectedActivity? activity = null)
    {
        _previewMode.Text = (_mode.SelectedItem?.ToString() ?? "Playing").ToUpperInvariant();

        if (_dnd.Checked)
        {
            _previewDetails.Text = "Presence hidden";
            _previewState.Text = "Do Not Disturb is on";
            return;
        }

        activity ??= ActivityDetector.Detect();
        if (activity is null)
        {
            _previewDetails.Text = _idle.Checked ? "ChatGPT RPC" : "No active presence";
            _previewState.Text = _idle.Checked
                ? "Waiting for ChatGPT / Codex"
                : "Presence will appear when ChatGPT or Codex is detected";
            return;
        }

        _previewDetails.Text = activity.Mode == "Codex" ? "Codex · Coding" : "ChatGPT · Chat";
        _previewState.Text = _showTitle.Checked && !string.IsNullOrWhiteSpace(activity.Title)
            ? activity.Title
            : activity.Mode == "Codex" ? "Working with Codex" : "Using ChatGPT";
    }

    private void Apply()
    {
        _config.ClientId = _clientId.Text.Trim();
        _config.LargeImageKey = _assetKey.Text.Trim();
        _config.ActivityType = _mode.SelectedItem?.ToString() ?? "Playing";
        _config.DoNotDisturb = _dnd.Checked;
        _config.IdlePresence = _idle.Checked;
        _config.ShowSessionTitle = _showTitle.Checked;
        _config.ShowElapsedTime = _elapsed.Checked;
        _config.StartWithWindows = _startup.Checked;

        ConfigStore.Save(_config);
        StartupManager.SetEnabled(_config.StartWithWindows);
        _onApplied(_config);

        _applied.Text = "applied";
        var clear = new System.Windows.Forms.Timer { Interval = 1800 };
        clear.Tick += (_, _) =>
        {
            _applied.Text = "";
            clear.Stop();
            clear.Dispose();
        };
        clear.Start();
    }
}
