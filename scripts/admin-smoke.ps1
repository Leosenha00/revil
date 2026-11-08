$ErrorActionPreference='Stop'
New-Item artifacts -ItemType Directory -Force | Out-Null
$identity=[Security.Principal.WindowsIdentity]::GetCurrent()
$principal=New-Object Security.Principal.WindowsPrincipal($identity)
if(!$principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){
 'NOT TESTED: runner token is not elevated; interactive UAC requires manual validation.' | Out-File artifacts/admin-smoke.txt
 exit 0
}
$exe=Resolve-Path 'publish/admin/ReviveOS.Admin.exe'
$request=[Guid]::NewGuid()
$p=Start-Process -FilePath $exe -ArgumentList @('--operation','check','--request',$request.ToString(),'--expected-sid',$identity.User.Value,'--ids','W10=') -Wait -PassThru
if($p.ExitCode -ne 0){throw "Admin helper check failed: $($p.ExitCode)"}
$file=Join-Path $env:LOCALAPPDATA ('ReviveOS\admin-results\'+$request.ToString('N')+'.json')
$result=Get-Content -LiteralPath $file -Raw | ConvertFrom-Json
if(!$result.Success){throw 'Admin helper returned failure'}
'PASS: helper ran with existing elevated token and returned a structured response. Interactive UAC consent is not tested here.' | Out-File artifacts/admin-smoke.txt
$other=[Guid]::NewGuid()
$p=Start-Process -FilePath $exe -ArgumentList @('--operation','check','--request',$other.ToString(),'--expected-sid','S-1-5-21-1-1-1-1000','--ids','W10=') -Wait -PassThru
if($p.ExitCode -ne 17){throw 'Helper did not reject the wrong account SID'}
'PASS: helper rejected a different user SID before any operation.' | Add-Content artifacts/admin-smoke.txt

$cpuRequest=[Guid]::NewGuid()
$p=Start-Process -FilePath $exe -ArgumentList @('--operation','cpu-inspect','--request',$cpuRequest.ToString(),'--expected-sid',$identity.User.Value,'--ids','W10=') -Wait -PassThru
if($p.ExitCode -ne 0){throw 'Read-only CPU/memory inspection failed'}
$file=Join-Path $env:LOCALAPPDATA ('ReviveOS\admin-results\'+$cpuRequest.ToString('N')+'.json')
$result=Get-Content -LiteralPath $file -Raw | ConvertFrom-Json
if(!$result.Success -or $result.Message -notmatch 'TotalPhysicalMemory' -or $result.Message -notmatch 'MemoryCaution'){throw 'CPU inspection did not return physical/virtual memory distinctions'}
'PASS: read-only CPU/RAM/pagefile/GPU inspection returned a report; optional query errors are reported separately.' | Add-Content artifacts/admin-smoke.txt
