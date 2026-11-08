using ReviveOS.Core;
namespace ReviveOS.WiFiManager;
public static class Wifi {
 public static Task<string> Inspect()=>Commands.CheckedPowerShell("netsh wlan show interfaces; netsh wlan show drivers; Get-NetAdapter -Physical | Select Name,Status,InterfaceDescription | Format-Table | Out-String");
 public const string Status="API WLAN e tarefa opcional no login implementadas. Hardware/OEM e modo avião podem impedir ativação. Requer validação real de reinício, suspensão e partida a frio; não simula Fn+F9.";
}
