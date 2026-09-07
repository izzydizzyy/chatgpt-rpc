# ChatGPT RPC

A tiny Windows tray app that shows ChatGPT and Codex activity as Discord Rich Presence.

No terminal window, no Node process, no Python script sitting open. Build it once and run the `.exe`.

## What it does

- detects ChatGPT Desktop or ChatGPT in a browser
- detects Codex windows
- shows Chat or Codex mode in Discord
- optionally shows the current window/session title
- keeps an elapsed session timer
- lives in the Windows system tray
- can start automatically with Windows
- stores settings locally in `%LOCALAPPDATA%\\ChatGPT-RPC`

## Setup

1. Create a Discord application in the Discord Developer Portal.
2. Copy its **Application ID**.
3. Run `ChatGPT-RPC.exe` and paste the ID into Settings.
4. Optional: add a Rich Presence image asset named `chatgpt` in your Discord application.
5. Keep Discord Desktop open.

After the first setup, just run the `.exe` (or let it start with Windows).

## Build

Requires the .NET 9 SDK on Windows:

```powershell
dotnet restore
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The executable will be under:

```text
bin/Release/net9.0-windows/win-x64/publish/ChatGPT-RPC.exe
```

GitHub Actions also builds the Windows x64 executable automatically on every push.

## Privacy

ChatGPT RPC only checks visible Windows process/window titles to determine whether ChatGPT or Codex is active. It does not read chat messages, browser history, cookies, tokens, or account data.

If you do not want the conversation/window title shown publicly on Discord, disable **Show current ChatGPT/Codex window title** in Settings.

## Notes

This project is unofficial and is not affiliated with OpenAI or Discord.

## License

MIT
