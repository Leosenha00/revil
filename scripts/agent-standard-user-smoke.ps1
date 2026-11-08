$ErrorActionPreference='Stop'
# Isolated CI-only account; no real notebook accounts or credentials are touched.
$name='ReviveT'+[guid]::NewGuid().ToString('N').Substring(0,10)
$secret=[guid]::NewGuid().ToString('N')+'!aA7'
$credential=New-Object System.Management.Automation.PSCredential("$env:COMPUTERNAME\$name",(ConvertTo-SecureString $secret -AsPlainText -Force))
$root=Join-Path $env:PUBLIC ('ReviveAgent-'+[guid]::NewGuid().ToString('N'))
$probeBuild=Join-Path $env:RUNNER_TEMP 'Revive-Agent-Probe'
$secondary=Get-Service seclogon
$user=$null
try {
  dotnet publish tests/ReviveOS.AgentProbe -c Release -r win-x64 --self-contained true -o $probeBuild
  if($LASTEXITCODE -ne 0){throw 'Probe publish failed'}
  $user=New-LocalUser -Name $name -Password $credential.Password -AccountNeverExpires -PasswordNeverExpires
  Add-LocalGroupMember -SID 'S-1-5-32-545' -Member $name
  New-Item "$root/data" -ItemType Directory -Force | Out-Null
  Copy-Item publish/agent "$root/agent" -Recurse
  Copy-Item $probeBuild "$root/probe" -Recurse
  & icacls $root /grant "*$($user.SID.Value):(OI)(CI)RX" /Q | Out-Null
  if($LASTEXITCODE -ne 0){throw 'Probe read permissions failed'}
  & icacls "$root/data" /grant "*$($user.SID.Value):(OI)(CI)M" /Q | Out-Null
  if($LASTEXITCODE -ne 0){throw 'Probe output permissions failed'}
  Start-Service seclogon
  $agent="$root/agent/ReviveOS.Agent.exe"
  $probe="$root/probe/ReviveOS.AgentProbe.exe"
  $report="$root/data/report.json"
  foreach($mode in @('start','verify-stop')){
    $bootstrap="$root/$mode.ps1"
    # Paths come from a fixed local public test directory; no secrets in scripts/arguments.
    @"
`$ErrorActionPreference='Stop'
try { & '$probe' '$mode' '$agent' '$report'; if(`$LASTEXITCODE -ne 0){throw 'Probe exited nonzero'} }
catch { `$_ | Out-String | Set-Content '$root/data/error.txt'; exit 1 }
"@ | Set-Content $bootstrap
    $child=Start-Process "$env:WINDIR/System32/WindowsPowerShell/v1.0/powershell.exe" -ArgumentList "-NoProfile -NonInteractive -File `"$bootstrap`"" -Credential $credential -LoadUserProfile -PassThru -Wait -WorkingDirectory $root
    if($child.ExitCode -ne 0){if(Test-Path "$root/data/error.txt"){Get-Content "$root/data/error.txt"};throw "Standard-user probe failed: $mode"}
    if($mode -eq 'start'){Start-Sleep -Seconds 3}
  }
  Copy-Item $report artifacts/agent-standard-user.json
} finally {
  if($user){
    try {if(Test-Path "$root/agent/ReviveOS.Agent.exe"){Start-Process "$root/agent/ReviveOS.Agent.exe" -ArgumentList '--stop' -Credential $credential -LoadUserProfile -Wait -WorkingDirectory $root | Out-Null}}catch{Write-Warning 'CI agent stop failed during cleanup'}
    $sid=$user.SID.Value
    Remove-LocalUser -Name $name
    Get-CimInstance Win32_UserProfile | Where-Object SID -eq $sid | Remove-CimInstance -ErrorAction SilentlyContinue
  }
  if($secondary.Status -eq 'Stopped'){Stop-Service seclogon -ErrorAction SilentlyContinue}
  if(Test-Path $root){Remove-Item $root -Recurse -Force -ErrorAction SilentlyContinue}
}
