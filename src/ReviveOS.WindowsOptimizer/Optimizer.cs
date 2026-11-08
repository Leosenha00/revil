using System.Text.Json;
using ReviveOS.Core;
namespace ReviveOS.WindowsOptimizer;
public record Tweak(string Id,string Description,string Benefit,string Risk,string After);
public sealed class Optimizer {
 readonly ChangeJournal journal=new(Path.Combine(State.Root,"changes-v2.json"),new WindowsSettings());
 public static readonly Tweak[] Catalog=[
  new("visual","Reduzir animações da janela","Menor trabalho de composição","Altera aparência; novo login pode ser necessário","0"),
  new("power","Plano Alto Desempenho","Pode reduzir latência de economia de energia","Mais calor e consumo; plano pode estar indisponível","8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c"),
  new("transparency","Desativar transparência","Pode reduzir trabalho da GPU","Altera aparência","0"),
  new("gamecapture","Desativar gravação de jogos em segundo plano para este usuário","Evita captura quando habilitada","Gravação por Game DVR deixa de estar habilitada","0")];
 public bool HasPending=>journal.Entries().Any(x=>x.Status!="restored")||LegacyPending();
 static bool LegacyPending(){var p=Path.Combine(State.Root,"changes.json");return File.Exists(p)&&JsonDocument.Parse(File.ReadAllText(p)).RootElement.EnumerateArray().Any(x=>x.GetProperty("Status").GetString()!="restored");}
 public string History()=>JsonSerializer.Serialize(journal.Entries(),State.Json);
 public async Task<string> Apply(string id,bool dryRun=true){var t=Catalog.Single(x=>x.Id==id);if(dryRun)return JsonSerializer.Serialize(t,State.Json);if(LegacyPending())throw new InvalidOperationException("Restaure primeiro os ajustes da versão 0.1");await journal.Apply(id,t.After);return "Aplicado e verificado: "+id;}
 public async Task Restore(string? id=null) {var errors=new List<Exception>();try{await journal.Restore(id);}catch(Exception e){errors.Add(e);}if(id==null)try{await RestoreLegacy();}catch(Exception e){errors.Add(e);}if(errors.Count>0)throw new AggregateException(errors);}
 static async Task RestoreLegacy(){var path=Path.Combine(State.Root,"changes.json");if(!File.Exists(path))return;var entries=JsonSerializer.Deserialize<List<LegacyChange>>(File.ReadAllText(path))!;var backend=new WindowsSettings();var errors=new List<Exception>();for(int i=entries.Count-1;i>=0;i--){var e=entries[i];if(e.Status=="restored")continue;try{var before=e.Before==""?null:e.Before;await backend.Write(e.Id,before);if(await backend.Read(e.Id)!=before)throw new IOException("Verificação falhou");entries[i]=e with{Status="restored"};State.Save(path,entries);}catch(Exception ex){errors.Add(ex);}}if(errors.Count>0)throw new AggregateException(errors);}
 record LegacyChange(string Id,string? Before,string After,string Status);
}
public sealed class WindowsSettings:ISettingsBackend {
 static (string Path,string Name,bool Dword) Registry(string id)=>id switch {
  "visual"=>(@"HKCU:\Control Panel\Desktop\WindowMetrics","MinAnimate",false),
  "transparency"=>(@"HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize","EnableTransparency",true),
  "gamecapture"=>(@"HKCU:\System\GameConfigStore","GameDVR_Enabled",true),
  _=>throw new ArgumentException("Ajuste desconhecido")};
 public async Task<string?> Read(string id){
  if(id=="power"){var s=await Commands.CheckedPowerShell("$s=powercfg /getactivescheme; if($LASTEXITCODE -ne 0){throw $s}; [regex]::Match($s,'[a-fA-F0-9]{8}(-[a-fA-F0-9]{4}){3}-[a-fA-F0-9]{12}').Value");if(!Guid.TryParse(s.Trim(),out var g))throw new IOException("Plano não identificado");return g.ToString();}
  var r=Registry(id);var value=await Commands.CheckedPowerShell($"$p='{r.Path}'; if(Test-Path $p){{$v=Get-ItemProperty -Path $p; if($null -ne $v.{r.Name}){{$kind=(Get-Item -Path $p).GetValueKind('{r.Name}'); if($kind -ne '{(r.Dword?"DWord":"String")}'){{throw 'Tipo de registro incompatível; ajuste recusado'}}; $v.{r.Name}}}}}");return value.Trim() is ""?null:value.Trim();
 }
 public async Task Write(string id,string? value){
  if(id=="power"){if(!Guid.TryParse(value,out var guid))throw new IOException("GUID inválido");await Commands.CheckedPowerShell($"powercfg /setactive {guid}; if($LASTEXITCODE -ne 0){{throw 'Plano indisponível'}}");return;}
  var r=Registry(id);if(value!=null&&!uint.TryParse(value,out _))throw new IOException("Backup de configuração inválido");
  var command=value==null?$"if(Test-Path $p){{Remove-ItemProperty -Path $p -Name '{r.Name}' -ErrorAction SilentlyContinue}}":$"if(!(Test-Path $p)){{New-Item -Path $p -Force | Out-Null}}; New-ItemProperty -Path $p -Name '{r.Name}' -PropertyType {(r.Dword?"DWord":"String")} -Value '{value}' -Force | Out-Null";
  await Commands.CheckedPowerShell($"$p='{r.Path}'; "+command);
 }
}
