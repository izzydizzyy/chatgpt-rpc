# ChatGPT RPC

A small Windows tray app that shows ChatGPT and Codex activity as Discord Rich Presence.

No terminal window, no Node process, no Python script sitting open. Run the `.exe` and leave it in the tray.

## v0.2.0

- dark settings window with a live Discord-style preview
- tray controls for Settings, DND, startup, activity mode, updates, and quit
- Playing / Watching / Listening / Competing presence modes
- optional idle presence
- optional session title and elapsed time
- improved ChatGPT / Codex window detection
- automatic startup with Windows
- self-contained Windows x64 build

## Setup

1. Create a Discord application in the Discord Developer Portal.
2. Copy its **Application ID**.
3. Run `ChatGPT-RPC.exe` and open **Settings**.
4. Paste the Application ID and click **Apply**.
5. Optional: upload a Rich Presence image asset named `chatgpt` to the Discord application.
6. Keep Discord Desktop open.

After setup, ChatGPT RPC can start with Windows and stay in the system tray.

## Presence controls

Right-click the tray icon to quickly change:

- Do Not Disturb
- Start on Windows
- Playing / Watching / Listening / Competing
- Settings
- Check for updates
- Quit

Settings also lets you control idle presence, session titles, elapsed time, and the image asset key.

## Build

Requires the .NET 9 SDK on Windows:

```powershell
dotnet restore
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The executable is created at:

```text
bin/Release/net9.0-windows/win-x64/publish/ChatGPT-RPC.exe
```

GitHub Actions also builds a Windows x64 artifact automatically on pushes to `main`.

## Privacy

ChatGPT RPC only checks visible Windows process and window titles to determine whether ChatGPT or Codex is active. It does not read chat messages, browser history, cookies, login tokens, or account data.

Session-title sharing can be disabled in Settings if you do not want the active window title displayed on Discord.

## Notes

ChatGPT RPC is unofficial and is not affiliated with OpenAI or Discord.

## License

MIT
