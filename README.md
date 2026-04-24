# EnvVar

English | [简体中文](./README.zh-CN.md) | [繁體中文](./README.zh-TW.md)

A visual environment variable management tool for Windows, built with .NET 10 / WPF.

![English](docs/images/8z9IhahUrl.png)

## Download

[![GitHub Release](https://img.shields.io/github/v/release/iridiumcao/EnvVar?logo=github)](https://github.com/iridiumcao/EnvVar/releases/latest)

You can download the latest installer from the [Releases](https://github.com/iridiumcao/EnvVar/releases/latest) page.

### Windows Security Notice

Because this project is a brand-new open source application and does not yet have a code signing certificate, Windows SmartScreen may show warnings such as:

- `Windows protected your PC`
- `Microsoft Defender SmartScreen prevented an unrecognized app from starting`

If that happens, click **Run anyway** to continue installation.

This software is fully open source and malware-free, and you can inspect the source code yourself.

VirusTotal result: **1/70**. Only one machine-learning engine reported a false positive; the other 69 security vendors reported no issue.

This is a common false positive for new Inno Setup installers, not a real problem. Full report:
https://www.virustotal.com/gui/file/0e1f6913a12bfc16547a948d793d55e54318d18cd1f2bad00f05538a1ae1a67f/detection

## Features

- Browse user-level and system-level environment variables
- Combined display or grouped by level, supports column sorting
- Create, edit, and delete environment variables
- Edit local metadata (Alias / Description), with built-in presets for common variables
- Real-time search and filtering (by name, alias, value), search box with magnifying glass indicator
- Structured editing for multi-value variables (like PATH): item-by-item editing, adding, deleting, moving, and sorting
- Import / Export as JSON files
- Automatically record single variable history versions (configurable limit, default 5), support independent viewing and restoration by variable
- Built-in rolling log system for lifecycle events, key operations, and unhandled exceptions
- Multi-language support: English / Simplified Chinese / Traditional Chinese (selection persistence)
- Theme support: Light / Dark / Follow System (theme persistence)
- Prompt to restart as administrator when permissions are insufficient

## Local Data

To avoid polluting real environment variables, `Alias` and `Description` are saved separately in local JSON files.

| Data | Path |
|------|------|
| Metadata | `%LocalAppData%\EnvVar\metadata.json` |
| Variable History | `%LocalAppData%\EnvVar\history.json` |
| Settings | `%LocalAppData%\EnvVar\settings.json` |
| Logs | `%LocalAppData%\EnvVar\Logs\YYYY-MM-DD.log` |

Metadata key format: `Name@Level`

```json
{
  "JAVA_HOME@User": {
    "alias": "Java Home",
    "description": "JDK installation path"
  }
}
```

## Instructions

1. After starting the application, the left side displays the environment variable list, and the right side displays the editing panel.
2. Click any variable to view and edit its content.
3. Click "New" to enter creation mode.
4. Click "Save" to write to the registry and local metadata.
5. Clicking "Delete" will require confirmation.
6. If the variable value contains `;`, the right side will display a structured editing area where items can be edited, added, deleted, moved, and sorted individually.
7. When editing an existing variable, click the "History" button to view and restore historical versions of that variable.
8. Export / Import through the "File" menu.
9. Use the "Preferences" menu to switch language, theme, alias column visibility, max history, and logging. Choices are remembered automatically.

## Permission Notes

- User-level variables can usually be modified directly.
- System-level variables require administrator permissions; a prompt will appear to restart as administrator if permissions are insufficient.

## Development

Project based on .NET 10 / WPF:

```bash
dotnet build
```

### Project Structure

| Directory / File | Description |
|-------------|------|
| `MainWindow.xaml(.cs)` | Main Window |
| `ViewModels/` | ViewModel Layer |
| `Models/` | Data Models (Entries, Settings, History, Logging) |
| `Services/` | Business Services (Env var R/W, Metadata, Import/Export, History, Logging, Localization, Settings, Themes) |
| `Infrastructure/` | Infrastructure (ObservableObject) |
| `Utilities/` | Utilities (Multi-value parsing) |
| `Views/` | Sub-windows (About, Settings, Themed MessageBox) |
| `Resources/Languages/` | Multi-language resource files |
| `docs/` | Documents |
| `installer/` | Installer scripts (Inno Setup) |

## Testing

The project includes a comprehensive suite of unit tests using **xUnit** and **Moq**.

To run the tests:

```bash
dotnet test
```

For more details, see the [Testing Documentation](docs/testing.md).

## Documentation

- [Functional Design Document](docs/design.md)
- [UI Design Document](docs/ui-design.md)
- [Testing Documentation](docs/testing.md)
- [Suggestions and Improvements](docs/suggestions.md)
- [Installer Build Guide](installer/BUILD.md)

## Building the Installer

The project uses [Inno Setup 6](https://jrsoftware.org/isdl.php) to create the Windows installer.

### Local Build
1. Ensure .NET 10 SDK and Inno Setup 6 are installed.
2. Run the build script:
   ```powershell
   ./installer/build-installer.ps1 -version 1.0.0
   ```
3. The generated installer will be located in the `release/` directory.

### Automated Build (GitHub Actions)
This project includes a GitHub Actions workflow for automated building and publishing:
- **On Push/PR to main**: Builds the installer and uploads it as a workflow artifact.
- **On Tag (`v*`)**: Builds the installer with the tag version and creates a GitHub Release.

For more details, see the [Installer Build Guide](installer/BUILD.md).
