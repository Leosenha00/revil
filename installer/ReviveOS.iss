[Setup]
AppId=ReviveOSHybridExtreme
AppName=ReviveOS Hybrid Extreme
AppVersion=0.6.0
DefaultDirName={localappdata}\Programs\ReviveOS
PrivilegesRequired=lowest
OutputDir=..\artifacts
OutputBaseFilename=ReviveOS-Hybrid-Extreme-Setup
Compression=lzma2
SolidCompression=yes
UninstallDisplayIcon={app}\ReviveOS.exe
[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
[Icons]
Name: "{userprograms}\ReviveOS Hybrid Extreme"; Filename: "{app}\ReviveOS.exe"
[Run]
Filename: "{app}\ReviveOS.exe"; Description: "Abrir ReviveOS"; Flags: nowait postinstall skipifsilent
