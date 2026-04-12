# Copilot Instructions for EnvVar

## Build and test commands

This repository is a Windows-only .NET 10 WPF app with a solution file at `EnvVar.slnx`.

```powershell
dotnet build EnvVar.slnx
dotnet test EnvVar.slnx
dotnet test EnvVar.Tests\EnvVar.Tests.csproj --filter "FullyQualifiedName~EnvVar.Tests.Models.VariableEditorModelTests.DeduplicateValue_ShouldRemoveDuplicates"
```

To build the installer used by CI and releases:

```powershell
.\installer\build-installer.ps1 -version 1.0.0
```

## High-level architecture

The app is centered on a single main window and a single main view model:

- `MainWindow.xaml` and `MainWindow.xaml.cs` own UI-only behavior: menu actions, file dialogs, confirmation dialogs, column-sort click cycling, unsaved-change prompts, and the admin-restart prompt.
- `ViewModels/MainWindowViewModel.cs` owns application state and orchestration. It loads variables, tracks selection and search/filter state, applies grouped vs merged display mode, records history before destructive changes, and delegates persistence/import/export to services.
- `Models/VariableEditorModel.cs` is not just a DTO. It contains the editor's working state, original-value tracking, `HasChanges`, multi-value list editing, reordering/sorting, and value deduplication.

Persistence is split across Windows registry and local JSON files:

- `Services/EnvironmentVariableService.cs` reads user/system variables and writes them directly to the registry (`HKCU\Environment` and `HKLM\...\Environment`), then broadcasts `WM_SETTINGCHANGE`.
- `Services/MetadataStore.cs` stores alias/description separately from the registry in `%LocalAppData%\EnvVar\metadata.json`.
- `Services/VersionHistoryService.cs` stores per-variable history in `%LocalAppData%\EnvVar\history.json`.
- `Services/SettingsService.cs` stores theme/language/history/logging settings in `%LocalAppData%\EnvVar\settings.json`.
- `Services/LoggingService.cs` stores rolling logs in `%LocalAppData%\EnvVar\Logs`.
- `Services/ExportImportService.cs` is the only import/export path; exported JSON includes both registry-backed values and local metadata.

Localization and theming are service-driven rather than view-model-driven:

- `Services/LocalizationService.cs` swaps `Resources/Languages/Strings.*.xaml` dictionaries and raises a `LanguageChanged` event.
- `Services/ThemeService.cs` swaps `Resources/Themes/Theme.*.xaml`, persists the selected theme, and updates WPF window title bars.
- `Services/WellKnownVariables.cs` provides localized built-in descriptions for common variables. `MainWindowViewModel` and `VariableEditorModel` treat these descriptions specially so they follow the current language until a user overrides them.

## Key conventions

- Alias and description are local-only metadata. Do not try to persist them into the Windows registry; they belong in `metadata.json`.
- Variable identity is consistently keyed as `Name@Level` (`User` or `System`). That key shape is used across metadata, history, reselection after save, and overwrite detection.
- Multi-value variables are semicolon-delimited and edited through the structured list in `VariableEditorModel`. Empty items are dropped, duplicates are removed case-insensitively, and original order is preserved for surviving entries.
- For save operations that rename a variable or move it between `User` and `System`, `EnvironmentVariableService.Save` writes the new entry first and then deletes the old registry entry/metadata key.
- Permission escalation is a UI concern. Service methods throw on access problems; `MainWindow.TryRun` catches security/access exceptions and offers restart-as-admin.
- Tests are concentrated on logic-heavy code (`Utilities`, `Models`, `MetadataStore`). Registry interaction and WPF UI behavior are only lightly covered, so changes in those areas usually require extra manual scrutiny.
- CI does not just build the app binary; `.github/workflows/build.yml` drives the installer build through `installer\build-installer.ps1` on `main`, PRs to `main`, and `v*` tags.
