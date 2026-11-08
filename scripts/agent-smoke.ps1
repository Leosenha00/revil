$ErrorActionPreference='Stop'
$p=Start-Process './publish/agent/ReviveOS.Agent.exe' -ArgumentList '--sample-once' -PassThru -Wait
if($p.ExitCode -ne 0){throw 'Native agent sample failed'}
$sample=Get-Content "$env:LOCALAPPDATA/ReviveOS/continuous/native-sample.json" -Raw | ConvertFrom-Json
if($sample.TotalMemoryMB -lt 1 -or $null -eq $sample.CpuPercent -or $sample.CpuPercent -lt 0 -or $sample.CpuPercent -gt 100){throw 'Invalid native CPU/RAM sample'}
New-Item artifacts -ItemType Directory -Force | Out-Null
$sample | ConvertTo-Json | Set-Content artifacts/agent-native-sample.json
# Full unattended operation is intentionally denied with an elevated token.
$p=Start-Process './publish/agent/ReviveOS.Agent.exe' -PassThru -Wait
if($p.ExitCode -ne 1){throw 'Elevated continuous agent was not refused'}
$state=Get-Content "$env:LOCALAPPDATA/ReviveOS/continuous/status.json" -Raw | ConvertFrom-Json
if($state.State -ne 'faulted' -or $state.Error -notmatch 'sem eleva'){throw 'Wrong refusal status'}
'Native CPU/RAM sampling and elevated-token refusal passed.' | Set-Content artifacts/agent-smoke.txt
