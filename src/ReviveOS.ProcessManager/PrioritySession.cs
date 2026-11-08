using System.Text.Json;
using ReviveOS.Core;
namespace ReviveOS.ProcessManager;
public record PriorityChange(ProcessIdentity Identity,uint Before,uint After,string State,DateTime UpdatedUtc,string? Error=null);
public sealed class PrioritySession(string file,IPriorityBackend backend) {
 List<PriorityChange> Read()=>File.Exists(file)?JsonSerializer.Deserialize<List<PriorityChange>>(File.ReadAllText(file))??throw new IOException("Diário de prioridades inválido"):[];
 public IReadOnlyList<PriorityChange> Entries()=>Read();
 void Save(List<PriorityChange> entries){var closed=entries.Where(x=>x.State is not ("pending" or "applied" or "restore-failed")).OrderByDescending(x=>x.UpdatedUtc).Take(32);State.Save(file,entries.Where(x=>x.State is "pending" or "applied" or "restore-failed").Concat(closed).ToArray());}
 public string Apply(ProcessDescriptor process,uint target){var entries=Read();if(entries.Any(x=>x.Identity.Key==process.Identity.Key&&x.State is "pending" or "applied" or "restore-failed"))return "já registrado";if(process.Priority!=NativePriorityBackend.Normal)return "prioridade personalizada preservada";var change=new PriorityChange(process.Identity,process.Priority,target,"pending",DateTime.UtcNow);entries.Add(change);Save(entries);
  try{backend.Change(process.Identity,process.Priority,target);entries[^1]=change with{State="applied"};Save(entries);return "aplicado";}catch(Exception e){entries[^1]=change with{Error=e.Message};Save(entries);throw;}
 }
 public string Restore(string? keepKey=null,ISet<string>? keepKeys=null){var entries=Read();var messages=new List<string>();bool changed=false;for(int i=entries.Count-1;i>=0;i--){var entry=entries[i];if(entry.State is not ("pending" or "applied" or "restore-failed")||entry.Identity.Key==keepKey||keepKeys?.Contains(entry.Identity.Key)==true)continue;changed=true;
   try{var current=backend.Describe(entry.Identity.Pid);if(current==null||!NativePriorityBackend.Same(current.Identity,entry.Identity))entries[i]=entry with{State="exited-or-reused",UpdatedUtc=DateTime.UtcNow};else if(current.Priority!=entry.After)entries[i]=entry with{State="external-change",UpdatedUtc=DateTime.UtcNow,Error="Prioridade externa preservada"};else{backend.Change(entry.Identity,entry.After,entry.Before);entries[i]=entry with{State="restored",UpdatedUtc=DateTime.UtcNow,Error=null};}messages.Add(entry.Identity.Pid+": "+entries[i].State);}
   catch(Exception e){entries[i]=entry with{State="restore-failed",UpdatedUtc=DateTime.UtcNow,Error=e.Message};messages.Add(entry.Identity.Pid+": falha de restauração: "+e.Message);}
  }if(changed)Save(entries);return string.Join("; ",messages);
 }
 public void ReconcileExternalChanges(){var entries=Read();bool changed=false;for(int i=0;i<entries.Count;i++){var entry=entries[i];if(entry.State!="applied")continue;try{var p=backend.Describe(entry.Identity.Pid);if(p==null||!NativePriorityBackend.Same(p.Identity,entry.Identity)){entries[i]=entry with{State="exited-or-reused",UpdatedUtc=DateTime.UtcNow};changed=true;}else if(p.Priority!=entry.After){entries[i]=entry with{State="external-change",UpdatedUtc=DateTime.UtcNow,Error="Alteração externa respeitada"};changed=true;}}catch(System.ComponentModel.Win32Exception){}}
  if(changed)Save(entries);
 }
}
