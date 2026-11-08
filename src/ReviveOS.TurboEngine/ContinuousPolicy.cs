using ReviveOS.Core;
namespace ReviveOS.TurboEngine;
public sealed record ContinuousConfig {
 public int Schema {get;init;}=1;public bool Enabled {get;init;}public bool StartAtLogin {get;init;}=true;public bool FavorForeground {get;init;}=true;public int PollSeconds {get;init;}=15;public string[] ApprovedBackgroundPaths {get;init;}=[];
 public void Validate(){if(Schema!=1||PollSeconds<10||PollSeconds>60||ApprovedBackgroundPaths.Length>8)throw new ArgumentException("Configuração contínua inválida");foreach(var path in ApprovedBackgroundPaths)if(!Path.IsPathFullyQualified(path)||!OptionalNames.Contains(Path.GetFileNameWithoutExtension(path),StringComparer.OrdinalIgnoreCase))throw new ArgumentException("Aplicativo de fundo não permitido: "+path);}
 public static readonly string[] OptionalNames=["OneDrive","Spotify","Discord","Steam","EpicGamesLauncher"];
 public static readonly string[] ForegroundNames=["acad","acadlt","chrome","msedge","firefox","Code","python","python3","blender","WINWORD","EXCEL","POWERPNT","Acrobat","AcroRd32"];
}
public record ContinuousDecision(bool CpuPressure,bool MemoryPressure,bool DiskPressure,bool LowerApprovedBackground,bool BoostForeground,bool DeferOwnBackup,bool ResourceBudgetLimited,string Reason);
public sealed class ContinuousPolicy {
 sealed class Latch {int high,low;public bool Active;public void Feed(bool? isHigh,bool isLow){if(isHigh==null){high=low=0;Active=false;return;}if(isHigh==true){high++;low=0;if(high>=3)Active=true;}else if(isLow){low++;high=0;if(low>=3)Active=false;}else{high=low=0;}}}
 readonly Latch cpu=new(),memory=new(),disk=new(),budget=new();
 public ContinuousDecision Evaluate(ResourceSample sample,ContinuousConfig config){config.Validate();cpu.Feed(sample.CpuPercent is double c?c>=85:null,sample.CpuPercent<65);memory.Feed(sample.AvailableMemoryMB<256,sample.AvailableMemoryMB>512);disk.Feed(sample.DiskQueue is double q?q>=2:null,sample.DiskQueue<0.5);
  budget.Feed(sample.AgentCpuPercent is double overhead?overhead>3||sample.AgentWorkingSetMB>96:null,sample.AgentCpuPercent<1&&sample.AgentWorkingSetMB<80);
  var critical=sample.AvailableMemoryMB<128;var boost=config.FavorForeground&&cpu.Active&&!disk.Active&&!critical&&sample.OnBattery==false&&!budget.Active;
  return new(cpu.Active,memory.Active,disk.Active,cpu.Active&&!budget.Active,boost,disk.Active||critical,budget.Active,budget.Active?"Orçamento do agente excedido: restauração e amostragem mais lenta":sample.CpuPercent==null?"CPU indisponível/aquecendo: sem intervenção de prioridade":cpu.Active||memory.Active||disk.Active?"Pressão sustentada; ações limitadas por contexto":"Observando; sem ajuste necessário");
 }
}
