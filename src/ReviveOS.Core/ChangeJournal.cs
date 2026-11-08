using System.Text.Json;
namespace ReviveOS.Core;
public record SettingChange(string Id,string? Before,string After,string Status,string? Error=null);
public interface ISettingsBackend {
 Task<string?> Read(string id);
 Task Write(string id,string? value);
}
/// <summary>Persists before mutation, verifies both directions, preserves failures for retry.</summary>
public sealed class ChangeJournal(string path,ISettingsBackend backend) {
 readonly SemaphoreSlim gate=new(1,1);
 public IReadOnlyList<SettingChange> Entries()=>File.Exists(path)?JsonSerializer.Deserialize<List<SettingChange>>(File.ReadAllText(path))??throw new IOException("Diário inválido"):[];
 public async Task Apply(string id,string after) {
  await gate.WaitAsync();try {
   var entries=Entries().ToList();if(entries.Any(x=>x.Id==id&&x.Status!="restored"))throw new InvalidOperationException("Restaure a alteração existente antes de reaplicar: "+id);
   var before=await backend.Read(id);entries.Add(new(id,before,after,"pending"));State.Save(path,entries);
   try {await backend.Write(id,after);if(await backend.Read(id)!=after)throw new IOException("Validação falhou: "+id);entries[^1]=entries[^1] with{Status="applied"};State.Save(path,entries);}
   catch(Exception e){entries[^1]=entries[^1] with{Error=e.Message};State.Save(path,entries);throw;}
  }finally{gate.Release();}
 }
 public async Task Restore(string? id=null) {
  await gate.WaitAsync();try {
   var entries=Entries().ToList();var errors=new List<Exception>();
   for(int i=entries.Count-1;i>=0;i--) {
    var entry=entries[i];if(entry.Status=="restored"||(id!=null&&entry.Id!=id))continue;
    try {await backend.Write(entry.Id,entry.Before);if(await backend.Read(entry.Id)!=entry.Before)throw new IOException("Validação de restauração falhou");entries[i]=entry with{Status="restored",Error=null};}
    catch(Exception e){entries[i]=entry with{Status="restore-failed",Error=e.Message};errors.Add(new IOException(entry.Id+": "+e.Message,e));}
    State.Save(path,entries);
   }
   if(errors.Count>0)throw new AggregateException("Algumas alterações não foram restauradas; consulte o diário e tente novamente",errors);
  }finally{gate.Release();}
 }
}
