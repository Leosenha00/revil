using ReviveOS.Core;
namespace ReviveOS.WiFiManager;
public static class Wifi {
 public static Task<string> Inspect()=>Commands.CheckedPowerShell("netsh wlan show interfaces; netsh wlan show drivers; Get-NetAdapter -Physical | Select Name,Status,InterfaceDescription | Format-Table | Out-String");
 public const string Status="Diagnóstico implementado. Auto Start não habilitado: exige identificar rádio, suporte OEM e testes de reinicialização, suspensão e partida a frio. Não simula Fn+F9.";
}
