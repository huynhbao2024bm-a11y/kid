# KID

KID is a local MCP server for Windows desktop accessibility. An MCP client starts it when needed, and an AI agent can use its tools to inspect and interact with the foreground application. You do not need to open a separate KID window.

KID uses Windows UI Automation. It can only work with controls that an application exposes through that accessibility interface.

## Requirements

- Windows 10 or later
- .NET 10 SDK to build or install from source
- An MCP client that supports local stdio servers

## Install

In PowerShell, from the project folder:

```powershell
powershell -ExecutionPolicy Bypass -File .\install.ps1
```

The installer publishes a self-contained Windows executable to `%LOCALAPPDATA%\Programs\KID` and registers it with detected Codex and Claude Code installations. Restart the client after installation.

To select a client explicitly:

```powershell
powershell -ExecutionPolicy Bypass -File .\install.ps1 -Clients Codex
powershell -ExecutionPolicy Bypass -File .\install.ps1 -Clients Claude
powershell -ExecutionPolicy Bypass -File .\install.ps1 -Clients Both
```

`Both` requires both client command-line tools to be installed. Use `-SkipClientRegistration` to publish KID without registering a client.

The optional client skill file is not included in this repository. The MCP server still provides usage instructions to clients that support them, and the installer can register the server without the skill.

## Configure other MCP clients

KID uses the standard MCP stdio transport. For example, add the following to a VS Code `.vscode\mcp.json` file after installing KID:

```json
{
  "servers": {
    "kid": {
      "type": "stdio",
      "command": "${env:LOCALAPPDATA}\\Programs\\KID\\Kid.Mcp.exe",
      "args": []
    }
  }
}
```

Other clients may use a different configuration format. Use the installed executable as the command and configure the server to use stdio.

## Tools

- `list_open_windows` lists visible application windows.
- `focus_window` brings a listed window to the foreground.
- `read_active_window` reads the foreground window's accessible controls and exposed text.
- `focus_accessible_element` focuses a control by exact name or automation ID.
- `activate_accessible_element` invokes, selects, toggles, expands, or collapses a control.
- `set_accessible_text` and `set_focused_text` set text in editable controls.
- `press_keys` sends supported keys or key combinations to the foreground window.

Element searches require an exact match. If the accessibility tree is incomplete or more than one control matches, KID does not choose a control for you.

## Build and test

```powershell
dotnet build Kid.slnx
dotnet test Kid.slnx
```

To run the server from source for development:

```powershell
dotnet run --project src\Kid.Mcp\Kid.Mcp.csproj
```

The server communicates over standard input and output. Normally, let the MCP client start it.

## Limitations

- KID works only on Windows and only with applications that expose useful controls through Windows UI Automation.
- It reads and operates on the foreground window; it does not use screenshots or coordinate-based clicking.
- Keyboard input is sent through Windows globally. A focus change can occur between the foreground check and the input being sent.
- MCP clients decide when to call KID tools. Registering the server does not guarantee that an agent will choose to use it.

## License

KID is licensed under the MIT License. See [LICENSE](LICENSE).
