$report=[ordered]@{CollectedUtc=[DateTime]::UtcNow.ToString('o');Errors=@()}
function Collect($name,[scriptblock]$body) { try {$report[$name]=@(& $body)} catch {$report.Errors+=@{Section=$name;Error=$_.Exception.Message}} }
Collect Computer {Get-CimInstance Win32_ComputerSystem | Select Manufacturer,Model,TotalPhysicalMemory,NumberOfLogicalProcessors}
Collect Board {Get-CimInstance Win32_BaseBoard | Select Manufacturer,Product,Version}
Collect BIOS {Get-CimInstance Win32_BIOS | Select Manufacturer,SMBIOSBIOSVersion,ReleaseDate}
Collect OS {Get-CimInstance Win32_OperatingSystem | Select Caption,Version,BuildNumber,LastBootUpTime,FreePhysicalMemory,TotalVisibleMemorySize}
Collect CPU {Get-CimInstance Win32_Processor | Select Name,NumberOfCores,NumberOfLogicalProcessors,MaxClockSpeed,LoadPercentage}
Collect GPU {Get-CimInstance Win32_VideoController | Select Name,DriverVersion,AdapterRAM,VideoModeDescription}
Collect Disk {Get-PhysicalDisk | Select FriendlyName,MediaType,HealthStatus,OperationalStatus,Size}
Collect Reliability {Get-PhysicalDisk | Get-StorageReliabilityCounter | Select DeviceId,Temperature,Wear,ReadErrorsTotal,WriteErrorsTotal}
Collect Volumes {Get-CimInstance Win32_LogicalDisk -Filter 'DriveType=3' | Select DeviceID,Size,FreeSpace}
Collect PageFile {Get-CimInstance Win32_PageFileUsage | Select Name,AllocatedBaseSize,CurrentUsage,PeakUsage}
Collect Performance {Get-CimInstance Win32_PerfFormattedData_PerfOS_Memory | Select AvailableMBytes,PagesPersec}
Collect DiskPerformance {Get-CimInstance Win32_PerfFormattedData_PerfDisk_PhysicalDisk | Select Name,PercentDiskTime,AvgDisksecPerTransfer,DiskBytesPersec,CurrentDiskQueueLength}
Collect Processes {Get-Process | Sort WorkingSet64 -Descending | Select -First 80 Id,ProcessName,CPU,WorkingSet64}
Collect Services {Get-CimInstance Win32_Service | Select Name,State,StartMode}
Collect Tasks {Get-ScheduledTask | Select TaskName,TaskPath,State}
Collect Startup {Get-CimInstance Win32_StartupCommand | Select Name,Command,Location}
Collect Applications {Get-ItemProperty 'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*','HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*' | Where DisplayName | Select DisplayName,DisplayVersion,Publisher}
Collect Drivers {Get-CimInstance Win32_PnPSignedDriver | Select DeviceName,DriverVersion,Manufacturer,IsSigned}
Collect Network {Get-NetAdapter | Select Name,InterfaceDescription,Status,MacAddress,LinkSpeed}
Collect WiFi {netsh wlan show interfaces; netsh wlan show drivers; netsh wlan show profiles}
Collect Power {powercfg /getactivescheme}
$report.Limitations=@('Temperaturas e rádio Wi-Fi dependem do fabricante; campos ausentes não significam estado saudável.','CPU de processo é tempo acumulado, não porcentagem instantânea.','Velocidade de internet não medida; nenhum upload realizado.','Dados identificáveis: revisar relatório antes de compartilhar.')
$report | ConvertTo-Json -Depth 7
