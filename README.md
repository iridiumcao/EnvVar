# EnvVar

English | [简体中文](./README.zh-CN.md) | [繁體中文](./README.zh-TW.md)

A visual environment variable management tool for Windows, built with .NET 10 / WPF.

![](docs/images/axsELw87FS.png)

## Download

[![GitHub Release](https://img.shields.io/github/v/release/iridiumcao/EnvVar?logo=github)](https://github.com/iridiumcao/EnvVar/releases/latest)

You can download the latest installer from the [Releases](https://github.com/iridiumcao/EnvVar/releases/latest) page.

## Features

- Browse user-level and system-level environment variables
- Combined display or grouped by level, supports column sorting
- Create, edit, and delete environment variables
- Edit local metadata (Alias / Description), with built-in presets for common variables
- Real-time search and filtering (by name, alias, value), search box with magnifying glass indicator
- Structured editing for multi-value variables (like PATH): item-by-item editing, adding, deleting, moving, and sorting
- Import / Export as JSON files
- Automatically record single variable history versions (configurable limit, default 5), support independent viewing and restoration by variable
- Multi-language support: English / Simplified Chinese / Traditional Chinese (selection persistence)
- Prompt to restart as administrator when permissions are insufficient

## Local Data

To avoid polluting real environment variables, `Alias` and `Description` are saved separately in local JSON files.

| Data | Path |
|------|------|
| Metadata | `%LocalAppData%\EnvVar\metadata.json` |
| Variable History | `%LocalAppData%\EnvVar\history.json` |
| Language Preference | `%LocalAppData%\EnvVar\language.txt` |

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
8. Export / Import variables through the "File" menu.
9. Switch interface language through the "Language" menu, the selection will be remembered automatically.

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
| `Models/` | Data Models |
| `Services/` | Business Services (Env var R/W, Metadata, Import/Export, History, Localization) |
| `Infrastructure/` | Infrastructure (ObservableObject) |
| `Utilities/` | Utilities (Multi-value parsing) |
| `Views/` | Sub-windows (About) |
| `Resources/Languages/` | Multi-language resource files |
| `docs/` | Design documents |
| `installer/` | Installer scripts (Inno Setup) |

## Documentation

- [Functional Design Document](docs/design.md)
- [UI Design Document](docs/ui-design.md)
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
