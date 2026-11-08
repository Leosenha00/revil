param([ValidateSet('Read','Write','Inventory')]$Operation,[string]$Id,[string]$ValueBase64)
$ErrorActionPreference='Stop'
$optional=@('OneDrive','Spotify','Discord','Steam','EpicGamesLauncher')
if($Operation -eq 'Inventory') {
 $hdd=$null
 try {$hdd=@(Get-PhysicalDisk | Where-Object MediaType -eq 'HDD').Count -gt 0} catch {}
 $queues=@(1..3 | ForEach-Object {try {$q=Get-CimInstance Win32_PerfFormattedData_PerfDisk_PhysicalDisk -Filter "Name='_Total'"; [double]$q.CurrentDiskQueueLength} catch {}; Start-Sleep -Seconds 1})
 $startup=@();$p='HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
 if(Test-Path $p){$k=Get-Item $p;foreach($n in $optional){if($k.GetValueNames() -contains $n){$startup+=$n}}}
 [ordered]@{Hdd=$hdd;QueueSamples=$queues;Startup=$startup;AutomaticPagefile=(Get-CimInstance Win32_ComputerSystem).AutomaticManagedPagefile;Services=@(Get-Service SysMain,WSearch -ErrorAction SilentlyContinue | Select-Object Name,Status)} | ConvertTo-Json -Depth 5 -Compress
 return
}
if($Id -like 'startup:*') {
 $name=$Id.Substring(8);if($optional -notcontains $name){throw 'Aplicativo fora da lista opcional'}
 $p='HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
 if($Operation -eq 'Read') {
  if((Test-Path $p) -and ((Get-Item $p).GetValueNames() -contains $name)) {
   $k=Get-Item $p;$kind=[string]$k.GetValueKind($name);if($kind -notin @('String','ExpandString')){throw 'Tipo de inicialização incompatível'}
   [ordered]@{Exists=$true;Kind=$kind;Value=$k.GetValue($name,$null,[Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames)} | ConvertTo-Json -Compress
  } else {'{"Exists":false,"Kind":null,"Value":null}'}
  return
 }
 $v=[Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($ValueBase64)) | ConvertFrom-Json
 if($v.Exists){if($v.Kind -notin @('String','ExpandString')){throw 'Tipo de backup inválido'};if(!(Test-Path $p)){New-Item $p -Force | Out-Null};New-ItemProperty -Path $p -Name $name -Value ([string]$v.Value) -PropertyType $v.Kind -Force | Out-Null}
 elseif(Test-Path $p){Remove-ItemProperty -Path $p -Name $name -ErrorAction SilentlyContinue}
 return
}
if($Id -like 'service:*') {
 $name=$Id.Substring(8);if($name -notin @('SysMain','WSearch')){throw 'Serviço fora da lista opcional'}
 $p='HKLM:\SYSTEM\CurrentControlSet\Services\'+$name
 if($Operation -eq 'Read') {
  $s=Get-Service -Name $name;if($s.Status -notin @('Running','Stopped')){throw 'Serviço em transição ou pausado; tente depois'}
  $v=Get-ItemProperty $p;if($v.Start -notin @(2,3,4)){throw 'Tipo de serviço não suportado'}
  [ordered]@{Start=[int]$v.Start;Delayed=$v.DelayedAutoStart;Running=($s.Status -eq 'Running')} | ConvertTo-Json -Compress
  return
 }
 $v=[Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($ValueBase64)) | ConvertFrom-Json
 if($v.Start -notin @(2,3,4) -or ($null -ne $v.Delayed -and $v.Delayed -notin @(0,1))){throw 'Backup de serviço inválido'}
 if($v.Running){Set-Service $name -StartupType Manual;Start-Service $name}
 else {Stop-Service $name -ErrorAction Stop}
 $s=Get-Service $name;$s.WaitForStatus($(if($v.Running){'Running'}else{'Stopped'}),[TimeSpan]::FromSeconds(20))
 $type=switch($v.Start){2{'Automatic'}3{'Manual'}4{'Disabled'}}
 Set-Service $name -StartupType $type
 if($null -eq $v.Delayed){Remove-ItemProperty $p -Name DelayedAutoStart -ErrorAction SilentlyContinue}else{New-ItemProperty $p -Name DelayedAutoStart -PropertyType DWord -Value ([int]$v.Delayed) -Force | Out-Null}
 return
}
if($Id -eq 'pagefile') {
 if($Operation -eq 'Read') {
  $auto=[bool](Get-CimInstance Win32_ComputerSystem).AutomaticManagedPagefile
  $settings=@();if(!$auto){$settings=@(Get-CimInstance Win32_PageFileSetting | Sort-Object Name | ForEach-Object {if($_.Name -notmatch '^[A-Za-z]:\\pagefile\.sys$'){throw 'Arquivo de paginação personalizado não suportado; ajuste recusado'};[ordered]@{Name=$_.Name;InitialSize=[uint32]$_.InitialSize;MaximumSize=[uint32]$_.MaximumSize}})}
  [ordered]@{Automatic=$auto;Settings=$settings} | ConvertTo-Json -Depth 5 -Compress
  return
 }
 $v=[Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($ValueBase64)) | ConvertFrom-Json
 foreach($s in $v.Settings){if($s.Name -notmatch '^[A-Za-z]:\\pagefile\.sys$' -or $s.InitialSize -lt 0 -or $s.MaximumSize -lt 0){throw 'Backup de paginação inválido'}}
 Get-CimInstance Win32_ComputerSystem | Set-CimInstance -Property @{AutomaticManagedPagefile=[bool]$v.Automatic}
 if(!$v.Automatic){Get-CimInstance Win32_PageFileSetting | Remove-CimInstance;foreach($s in $v.Settings){New-CimInstance -ClassName Win32_PageFileSetting -Property @{Name=[string]$s.Name;InitialSize=[uint32]$s.InitialSize;MaximumSize=[uint32]$s.MaximumSize} | Out-Null}}
 return
}
throw 'Ajuste desconhecido'
