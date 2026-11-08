using System.Diagnostics;
using System.Text.Json;
using Microsoft.Win32;
using ReviveOS.Core;
using ReviveOS.ProcessManager;
namespace ReviveOS.TurboEngine;
public record ContinuousStatus(int Pid,DateTime Utc,string State,ResourceSample? Sample,ContinuousDecision? Decision,string[] Actions,string? Error);
public static class ContinuousClient {
 public static string Root=>Path.Combine(State.Root,"continuous");
 public static string ConfigFile=>Path.Combine(Root,"config.json");
 public static string JournalFile=>Path.Combine(Root,"priorities.json");
 static T? Read<T>(string file){using var stream=new FileStream(file,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete);return JsonSerializer.Deserialize<T>(stream);}
 public static ContinuousConfig Config()=>File.Exists(ConfigFile)?Read<ContinuousConfig>(ConfigFile)??throw new IOException("Configuração inválida"):new();
 public static ContinuousStatus? Status()=>File.Exists(Path.Combine(Root,"status.json"))?Read<ContinuousStatus>(Path.Combine(Root,"status.json")):null;
 public static FileStream Lease(){Directory.CreateDirectory(Root);return new FileStream(Path.Combine(Root,"agent.lock"),FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);}
 static FileStream Control(){Directory.CreateDirectory(Root);return new FileStream(Path.Combine(Root,"control.lock"),FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);}
 const string RunKey=@"Software\Microsoft\Windows\CurrentVersion\Run",Name="ReviveOSContinuous";
 static void Startup(bool enable,string executable){if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException();using var key=Registry.CurrentUser.CreateSubKey(RunKey);var path=Path.Combine(Root,"startup.json");var command="\""+executable+"\"";var expected=File.Exists(path)?JsonSerializer.Deserialize<string>(File.ReadAllText(path)):null;var raw=key.GetValue(Name);if(raw!=null&&raw is not string)throw new IOException("Entrada de inicialização de tipo externo preservada");var current=raw as string;
  if(enable){if(current!=null&&current!=expected)throw new IOException("Entrada de inicialização externa preservada");State.Save(path,command);key.SetValue(Name,command,RegistryValueKind.String);if((string?)key.GetValue(Name)!=command)throw new IOException("Inicialização não confirmada");}
  else if(expected!=null){if(current==expected)key.DeleteValue(Name,false);else if(current!=null)throw new IOException("Entrada de inicialização alterada externamente; preservada");File.Delete(path);}
 }
 public static async Task<string> Start(ContinuousConfig config,string executable){if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException();using var control=Control();config.Validate();if(!File.Exists(executable))throw new FileNotFoundException("Agente não instalado",executable);using(var lease=Lease()){var recovery=new PrioritySession(JournalFile,new NativePriorityBackend());recovery.Restore();if(recovery.Entries().Any(x=>x.State=="restore-failed"))throw new IOException("Há prioridades pendentes; restaure antes de iniciar");Startup(config.StartAtLogin,executable);State.Save(ConfigFile,config with{Enabled=true});File.Delete(Path.Combine(Root,"stop"));}
  using var process=Process.Start(new ProcessStartInfo(executable){UseShellExecute=false,CreateNoWindow=true})??throw new IOException("Agente não iniciou");for(var i=0;i<60;i++){await Task.Delay(250);var status=Status();if(status?.Pid==process.Id&&status.Utc>DateTime.UtcNow.AddSeconds(-10)){if(status.State=="running")return "Otimização contínua ativa. Continua ao fechar esta janela.\n"+JsonSerializer.Serialize(status,State.Json);if(status.State=="faulted")throw new IOException(status.Error);}if(process.HasExited)throw new IOException("Agente encerrou: consulte continuous/status.json");}throw new IOException("Início não confirmado; use Desativar e restaurar");
 }
 public static async Task<string> Stop(){using var control=Control();Directory.CreateDirectory(Root);File.WriteAllText(Path.Combine(Root,"stop"),"stop");ContinuousConfig config;try{config=Config();}catch(System.Text.Json.JsonException){File.Copy(ConfigFile,ConfigFile+".invalid",true);config=new();}State.Save(ConfigFile,config with{Enabled=false});Exception? startupError=null;try{Startup(false,"");}catch(Exception ex){startupError=ex;}for(int i=0;i<80;i++){FileStream lease;try{lease=Lease();}catch(IOException){await Task.Delay(250);continue;}using(lease){var journal=new PrioritySession(JournalFile,new NativePriorityBackend());var result=journal.Restore();if(journal.Entries().Any(x=>x.State=="restore-failed"))throw new IOException("Restauração incompleta: "+result);if(startupError!=null)throw new IOException("Prioridades restauradas, mas inicialização não removida: "+startupError.Message,startupError);return "Agente desativado, inicialização removida e prioridades restauradas. "+result;}}throw new IOException("Agente ainda ocupado; solicitação de parada registrada. Tente restaurar novamente.");}
}
