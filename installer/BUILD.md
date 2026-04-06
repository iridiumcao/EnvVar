# Build Installer Guide

This guide explains how to build the EnvVar installer from source.

---

## 🧰 Requirements

* Windows 10/11
* .NET SDK (>= 10.0)
* Inno Setup 6

Download Inno Setup: https://jrsoftware.org/isdl.php

---

## 🚀 Steps

### 1. Clone repository

```bash
git clone https://github.com/iridiumcao/EnvVar.git
cd EnvVar
```

---

### 2. Publish application

Use the `/p:Version` flag to set the application version.

```bash
dotnet publish EnvVar.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:Version=1.0.0 -o release/publish
```

---

### 3. Build installer

#### Option A: Using the build script (Recommended)

The script handles publishing and installer compilation in one step. It automatically detects `iscc.exe` in your PATH or the default installation directory.

```powershell
./installer/build-installer.ps1 -version 1.0.0
```

---

#### Option B: Using the command line (ISCC)

If you prefer to run the Inno Setup compiler manually, you must provide the version defines:

```bash
iscc installer/EnvVar.iss /dAppVersion=1.0.0 /dAppVersionName=1.0.0
```

---

### 4. Output

The installer will be generated at:

```plaintext
release/EnvVar-Setup-1.0.0.exe
```

---

## 🤖 GitHub Actions

This project includes a CI/CD workflow (`.github/workflows/build.yml`) that automates the build process:

- **Work-in-progress**: Any push to `main` or Pull Request will trigger a build to ensure the installer can be created successfully. The result is uploaded as a workflow artifact.
- **Releases**: When you push a tag starting with `v` (e.g., `git tag v1.2.3`), the action will:
  1. Extract the version number (`1.2.3`).
  2. Build the installer with that version.
  3. Create a GitHub Release and upload the versioned installer.

---

## ⚠️ Notes

* Administrator privileges are required to install.
* Ensure Inno Setup 6 is installed correctly and optionally added to your `%PATH%`.
* The `AppVersion` define is required by `EnvVar.iss` to set the internal metadata and the output filename.
