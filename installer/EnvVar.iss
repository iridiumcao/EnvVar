[Setup]
AppName=EnvVar
AppVersion={#AppVersion}
AppPublisher=iridiumcao
DefaultDirName={autopf}\EnvVar
ArchitecturesInstallIn64BitMode=x64os
DefaultGroupName=EnvVar
OutputDir=..\release
OutputBaseFilename=EnvVar-Setup-{#AppVersionName}
Compression=lzma
SolidCompression=yes
WizardStyle=modern

; Requires administrator privileges (as it may modify system variables)
PrivilegesRequired=admin

[Files]
Source: "..\release\publish\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\EnvVar"; Filename: "{app}\EnvVar.exe"
Name: "{commondesktop}\EnvVar"; Filename: "{app}\EnvVar.exe"

[Run]
Filename: "{app}\EnvVar.exe"; Description: "Launch EnvVar"; Flags: nowait postinstall skipifsilent