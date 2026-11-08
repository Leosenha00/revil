using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using ReviveOS.Core;
namespace ReviveOS.Security;
public record AdminResult(bool Success,string Message);
public static class AdminBroker {
 public static bool IsAdministrator(){if(!OperatingSystem.IsWindows())return false;using var identity=WindowsIdentity.GetCurrent();return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);}
 public static string UserSid(){if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException();using var identity=WindowsIdentity.GetCurrent();return identity.User?.Value??throw new IOException("SID não identificado");}
 public static void Validate(string operation,string[] ids){if(operation is not ("check" or "cpu-inspect" or "cpu-max" or "cpu-restore" or "deep-apply" or "deep-restore"))throw new ArgumentException("Operação administrativa não permitida");if(ids.Length>8)throw new ArgumentException("Limite de seleção excedido");foreach(var id in ids)if(id is not ("service:SysMain" or "service:WSearch" or "pagefile" or "startup:OneDrive" or "startup:Spotify" or "startup:Discord" or "startup:Steam" or "startup:EpicGamesLauncher"))throw new ArgumentException("ID não permitido");if(!operation.StartsWith("deep-")&&ids.Length!=0)throw new ArgumentException("Parâmetros inesperados");}
 public static string ResultPath(Guid request)=>Path.Combine(State.Root,"admin-results",request.ToString("N")+".json");
 public static async Task<string> Run(string operation,string[]? ids=null){ids??=[];Validate(operation,ids);if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException();var path=Path.Combine(AppContext.BaseDirectory,"admin","ReviveOS.Admin.exe");if(!File.Exists(path))throw new IOException("Componente administrativo não encontrado; reinstale a versão completa");var request=Guid.NewGuid();var si=new ProcessStartInfo(path){UseShellExecute=true,Verb="runas",WorkingDirectory=Path.GetDirectoryName(path)!};foreach(var arg in new[]{"--operation",operation,"--request",request.ToString(),"--expected-sid",UserSid(),"--ids",Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(ids)))})si.ArgumentList.Add(arg);
  Process process;try{process=Process.Start(si)??throw new IOException("Componente não iniciou");}catch(System.ComponentModel.Win32Exception e) when(e.NativeErrorCode==1223){throw new OperationCanceledException("Permissão cancelada no UAC; operação não iniciada");}
  using(process){await process.WaitForExitAsync();if(process.ExitCode==17)throw new IOException("A elevação usou outra conta Windows; operação recusada para preservar os backups do usuário. Use a mesma conta administradora.");var file=ResultPath(request);if(!File.Exists(file))throw new IOException("Componente terminou sem relatório; consulte os diários e não presuma sucesso");var result=JsonSerializer.Deserialize<AdminResult>(await File.ReadAllTextAsync(file))??throw new IOException("Resposta administrativa inválida");if(!result.Success||process.ExitCode!=0)throw new IOException(result.Message);return result.Message;}
 }
}
