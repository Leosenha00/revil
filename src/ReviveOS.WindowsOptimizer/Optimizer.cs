using System.Text.Json;
using ReviveOS.Core;
namespace ReviveOS.WindowsOptimizer;
public record Tweak(string Id,string Description,string Benefit,string Risk);
public record Change(string Id,string? Before,string After,string Status);
public sealed class Optimizer {
 readonly string journal=Path.Combine(State.Root,"changes.json");
 readonly SemaphoreSlim gate=new(1,1);
 public static readonly Tweak[] Catalog=[new("visual","Reduzir animações da janela","Menor trabalho de composição; ganho depende do hardware","Altera aparência do usuário atual"),new("power","Plano Alto Desempenho","Pode reduzir atraso por economia de energia","Maior consumo, calor e ruído; pode não existir neste equipamento")];
 List<Change> Read()=>File.Exists(journal)?JsonSerializer.Deserialize<List<Change>>(File.ReadAllText(journal))!:[];
 public async Task<string> Apply(string id,bool dryRun=true) {
  if(!Catalog.Any(t=>t.Id==id))throw new ArgumentException("Ajuste não suportado");
  if(dryRun)return JsonSerializer.Serialize(Catalog.Single(t=>t.Id==id),State.Json);
  await gate.WaitAsync();try {
   var items=Read(); if(items.Any(x=>x.Id==id && x.Status!="restored"))throw new InvalidOperationException("Alteração já registrada; restaure antes de reaplicar");
   string? before;
   if(id=="visual") before=(await Commands.CheckedPowerShell("$p='HKCU:\\Control Panel\\Desktop\\WindowMetrics'; $v=Get-ItemProperty -Path $p; if($null -ne $v.MinAnimate){$v.MinAnimate}" )).Trim();
   else {var s=await Commands.CheckedPowerShell("$s=powercfg /getactivescheme; if($LASTEXITCODE -ne 0){throw $s}; [regex]::Match($s,'[a-fA-F0-9]{8}(-[a-fA-F0-9]{4}){3}-[a-fA-F0-9]{12}').Value");before=s.Trim();if(!Guid.TryParse(before,out _))throw new IOException("Plano atual não identificado");}
   var change=new Change(id,before,id=="visual"?"0":"8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c","pending");items.Add(change);State.Save(journal,items);
   await Set(change,false); items[^1]=change with{Status="applied"};State.Save(journal,items);return "Aplicado e verificado: "+id;
  }finally{gate.Release();}
 }
 static async Task Set(Change c,bool undo) {
  var v=undo?c.Before:c.After;
  if(c.Id=="visual") {
   if(v is not ("" or "0" or "1"))throw new IOException("Valor de backup inválido");
   var body=string.IsNullOrEmpty(v)?"Remove-ItemProperty -Path $p -Name MinAnimate -ErrorAction SilentlyContinue":"Set-ItemProperty -Path $p -Name MinAnimate -Value '"+v+"'";
   await Commands.CheckedPowerShell("$p='HKCU:\\Control Panel\\Desktop\\WindowMetrics'; "+body+"; $actual=(Get-ItemProperty -Path $p).MinAnimate; if([string]$actual -ne '"+v+"'){throw 'Validação falhou'}");
  }else if(c.Id=="power") {
   if(!Guid.TryParse(v,out var guid))throw new IOException("GUID inválido");
   await Commands.CheckedPowerShell($"powercfg /setactive {guid}; if($LASTEXITCODE -ne 0){{throw 'Plano indisponível'}}; $s=powercfg /getactivescheme; if($s -notmatch '{guid}'){{throw 'Validação falhou'}}");
  } else throw new IOException("Registro desconhecido");
 }
 public async Task Restore() {await gate.WaitAsync();try{var items=Read();for(int i=items.Count-1;i>=0;i--){if(items[i].Status=="restored")continue;await Set(items[i],true);items[i]=items[i] with{Status="restored"};State.Save(journal,items);}}finally{gate.Release();}}
}
