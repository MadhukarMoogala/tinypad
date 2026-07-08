# tinypad

A lightweight, dark-themed Windows text editor built with .NET 10 and WinForms. Designed as a bloat-free replacement for Windows 11 Notepad.

## Features

- **Minimal & Fast**: Responsive text editor with minimal dependencies
- **Dark Theme**: VS Code "One Dark Pro" color scheme throughout
- **Syntax Highlighting**: C# syntax highlighting with ScintillaNET
- **Find & Replace**: Full find/replace dialog with keyboard shortcuts
- **Command-line Support**: Open files directly: `tiny.exe myfile.txt`
- **Fullscreen Mode**: F11 to toggle fullscreen
- **Word Wrap**: View menu toggle
- **Borderless Window**: Clean, modern interface

## Installation

### From Release

Download `tiny.exe` from [Releases](https://github.com/MadhukarMoogala/tinypad/releases)

### From Source

```bash
git clone https://github.com/MadhukarMoogala/tinypad.git
cd tinypad/tinypad-cs
dotnet build -c Release
```

Output binary: `bin/Release/net10.0-windows/tiny.exe`

## Usage

Open a file:

```bash
tiny.exe path/to/file.txt
```

Or open the editor without a file:

```bash
tiny.exe
```

### Keyboard Shortcuts

| Shortcut     | Action         |
| ------------ | -------------- |
| Ctrl+N       | New            |
| Ctrl+O       | Open           |
| Ctrl+S       | Save           |
| Ctrl+Shift+S | Save As        |
| Ctrl+Z       | Undo           |
| Ctrl+Y       | Redo           |
| Ctrl+X       | Cut            |
| Ctrl+C       | Copy           |
| Ctrl+V       | Paste          |
| Ctrl+A       | Select All     |
| Ctrl+F       | Find           |
| Ctrl+H       | Find & Replace |
| F11          | Fullscreen     |

## Requirements

- .NET 10.0 runtime (Windows)
- Windows 7 or later

## Technology Stack

- **Framework**: .NET 10.0-windows (.NET Framework for Windows)
- **UI**: Windows Forms (WinForms)
- **Editor Component**: ScintillaNET.Core 3.6.51
- **Language**: C#

## License

MIT License - see [LICENSE](LICENSE) file for details

## Contributing

Contributions welcome! Please submit issues or pull requests on GitHub.
