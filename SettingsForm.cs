namespace ChatGPTRpc;

internal sealed class SettingsForm : Form
{
    private readonly TextBox _clientId = new() { Width = 340 };
    private readonly TextBox _assetKey = new() { Width = 340 };
    private readonly CheckBox _showTitle = new() { Text = "Show current ChatGPT/Codex window title", AutoSize = true };
    private readonly CheckBox _startup = new() { Text = "Start with Windows", AutoSize = true };

    public AppConfig Config { get; }

    public SettingsForm(AppConfig config)
    {
        Config = config;

        Text = "ChatGPT RPC Settings";
        Width = 430;
        Height = 300;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        _clientId.Text = config.ClientId;
        _assetKey.Text = config.LargeImageKey;
        _showTitle.Checked = config.ShowSessionTitle;
        _startup.Checked = config.StartWithWindows;

        var save = new Button { Text = "Save", AutoSize = true };
        save.Click += (_, _) => SaveAndClose();

        var cancel = new Button { Text = "Cancel", AutoSize = true };
        cancel.Click += (_, _) => Close();

        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(18),
            AutoScroll = true
        };

        layout.Controls.Add(new Label { Text = "Discord Application Client ID", AutoSize = true });
        layout.Controls.Add(_clientId);
        layout.Controls.Add(new Label { Text = "Large image asset key (optional)", AutoSize = true, Margin = new Padding(3, 12, 3, 3) });
        layout.Controls.Add(_assetKey);
        layout.Controls.Add(_showTitle);
        layout.Controls.Add(_startup);

        var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, Margin = new Padding(0, 14, 0, 0) };
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        layout.Controls.Add(buttons);

        Controls.Add(layout);
        AcceptButton = save;
        CancelButton = cancel;
    }

    private void SaveAndClose()
    {
        Config.ClientId = _clientId.Text.Trim();
        Config.LargeImageKey = _assetKey.Text.Trim();
        Config.ShowSessionTitle = _showTitle.Checked;
        Config.StartWithWindows = _startup.Checked;

        ConfigStore.Save(Config);
        StartupManager.SetEnabled(Config.StartWithWindows);
        DialogResult = DialogResult.OK;
        Close();
    }
}
