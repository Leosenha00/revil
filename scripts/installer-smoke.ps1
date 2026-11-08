$ErrorActionPreference='Stop'
$target=Join-Path $env:RUNNER_TEMP 'ReviveOS-Installer-Test'
$setup=Resolve-Path 'artifacts/ReviveOS-Hybrid-Extreme-Setup.exe'
$installed=Join-Path $target 'ReviveOS.exe'
$uninstaller=Join-Path $target 'unins000.exe'
try {
  $p=Start-Process $setup -ArgumentList @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART',"/DIR=`"$target`"") -Wait -PassThru
  if($p.ExitCode -ne 0 -or !(Test-Path $installed)){throw 'Silent install failed'}
  'Installer installed executable.' | Add-Content artifacts/windows-smoke.txt
  $app=Start-Process $installed -PassThru
  try {
    $ready=$false
    for($i=0;$i -lt 20;$i++){
      Start-Sleep -Seconds 1
      $app.Refresh()
      if($app.HasExited){throw 'Installed application exited early'}
      if($app.MainWindowHandle -ne 0){$ready=$true;break}
    }
    if(!$ready){throw 'Installed application did not open a window'}
    if(!$app.CloseMainWindow() -or !$app.WaitForExit(10000)){throw 'Installed app did not close cooperatively'}
    if($app.ExitCode -ne 0){throw 'Installed app exit code nonzero'}
    'Installed application opened and closed.' | Add-Content artifacts/windows-smoke.txt
  } finally {
    $app.Refresh()
    if(!$app.HasExited){$app.Kill()}
  }
} finally {
  if(Test-Path $uninstaller){
    $p=Start-Process $uninstaller -ArgumentList @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART') -Wait -PassThru
    if($p.ExitCode -ne 0 -or (Test-Path $installed)){throw 'Uninstall failed'}
    'Installer uninstall succeeded.' | Add-Content artifacts/windows-smoke.txt
  }
}
