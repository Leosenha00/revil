$ErrorActionPreference='Stop'
New-Item artifacts -ItemType Directory -Force | Out-Null
# Exercise real collector on the Windows runner, without publishing identifying inventory.
$diagnostic = & powershell.exe -NoProfile -NonInteractive -File scripts/diagnose.ps1 | Out-String
if ($LASTEXITCODE -ne 0) { throw 'Collector process failed' }
$report=$diagnostic | ConvertFrom-Json
if (!$report.CPU -or !$report.Computer) { throw 'CPU or computer inventory missing' }
"Collector succeeded. Optional section errors: $($report.Errors.Count)" | Out-File artifacts/windows-smoke.txt
# Verify a real WPF window exists and closes cooperatively.
$p=Start-Process -FilePath (Resolve-Path publish/ReviveOS.exe) -PassThru
try {
  $ready=$false
  for ($i=0;$i -lt 20;$i++) {
    Start-Sleep -Seconds 1
    $p.Refresh()
    if ($p.HasExited) { throw "App exited early: $($p.ExitCode)" }
    if ($p.MainWindowHandle -ne 0) {$ready=$true;break}
  }
  if (!$ready) { throw 'No WPF main window appeared within 20 seconds' }
  'WPF window opened.' | Add-Content artifacts/windows-smoke.txt
  if (!$p.CloseMainWindow()) { throw 'Window refused close request' }
  if (!$p.WaitForExit(10000)) { throw 'App did not close within 10 seconds' }
  if ($p.ExitCode -ne 0) { throw "Unexpected exit: $($p.ExitCode)" }
  'WPF window closed cleanly.' | Add-Content artifacts/windows-smoke.txt
} finally {
  $p.Refresh()
  if (!$p.HasExited) { $p.Kill() }
}
