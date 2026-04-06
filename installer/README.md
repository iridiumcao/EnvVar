# Build Installer Guide

This guide explains how to build the EnvVar installer from source.

---

## 🧰 Requirements

* Windows 10/11
* .NET SDK (>= 8.0)
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

```bash
dotnet publish EnvVar.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o release/publish
```

---

### 3. Build installer

#### Option A: Using GUI

* Open `installer/EnvVar.iss`
* Click **Compile**

---

#### Option B: Using command line

`iscc` can be found in the Inno Setup home directory. If the directory has not been added to the `%PATH%` environment variable, please add it manually.

```bash
iscc installer/EnvVar.iss
```

---

### 4. Output

The installer will be generated at:

```plaintext
release/EnvVar-Setup.exe
```

---

## ⚠️ Notes

* Administrator privileges are required to install
* Make sure Inno Setup is installed correctly

---

## 💡 Tip

You can also run:

```powershell
installer/build-installer.ps1
```

to build everything in one step.
