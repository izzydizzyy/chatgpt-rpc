namespace ChatGPTRpc;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        using var mutex = new Mutex(true, "ChatGPT-RPC-SingleInstance", out var firstInstance);
        if (!firstInstance)
        {
            MessageBox.Show("ChatGPT RPC is already running in your system tray.", "ChatGPT RPC");
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new TrayApp());
    }
}
