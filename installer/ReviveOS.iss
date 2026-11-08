[Setup]
AppId=ReviveOSHybridExtreme
AppName=ReviveOS Hybrid Extreme
AppVersion=0.8.0
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

[Code]
function PrepareToInstall(var NeedsRestart: Boolean): String;
var ResultCode: Integer; Agent: String;
begin
  Agent := ExpandConstant('{app}\agent\ReviveOS.Agent.exe');
  if FileExists(Agent) then
    if not Exec(Agent, '--stop', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
      Result := 'Não foi possível parar o agente anterior. Desative e restaure antes de atualizar.'
    else if ResultCode <> 0 then
      Result := 'Restauração do agente incompleta. Abra ReviveOS e use Desativar e restaurar.';
end;

function InitializeUninstall(): Boolean;
var ResultCode: Integer; Agent: String;
begin
  Result := True;
  Agent := ExpandConstant('{app}\agent\ReviveOS.Agent.exe');
  if FileExists(Agent) then begin
    Result := Exec(Agent, '--stop', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    if Result then Result := ResultCode = 0;
    if not Result then MsgBox('Não foi possível restaurar o agente. Use Desativar e restaurar antes de desinstalar.', mbError, MB_OK);
  end;
end;
