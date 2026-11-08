using ReviveOS.Core;
using Xunit;
public class RecoveryTests {
 sealed class Backend:ISettingsBackend {
  public Dictionary<string,string?> Values=new(){{"a","original"},{"b",null}};public string? Fail;
  public Task<string?> Read(string id)=>Task.FromResult(Values[id]);
  public Task Write(string id,string? value){if(id==Fail)throw new IOException("Injected failure");Values[id]=value;return Task.CompletedTask;}
 }
 [Fact] public async Task InterruptedMutationCanRestoreAfterRestart(){var dir=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString());try{var backend=new Backend{Fail="a"};var journal=new ChangeJournal(Path.Combine(dir,"changes.json"),backend);await Assert.ThrowsAsync<IOException>(()=>journal.Apply("a","changed"));Assert.Equal("pending",Assert.Single(journal.Entries()).Status);backend.Fail=null;backend.Values["a"]="partially applied";await new ChangeJournal(Path.Combine(dir,"changes.json"),backend).Restore();Assert.Equal("original",backend.Values["a"]);Assert.Equal("restored",Assert.Single(journal.Entries()).Status);}finally{if(Directory.Exists(dir))Directory.Delete(dir,true);}}
 [Fact] public async Task RestoreContinuesAfterOneFailureAndSupportsRetry(){var dir=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString());try{var backend=new Backend();var journal=new ChangeJournal(Path.Combine(dir,"changes.json"),backend);await journal.Apply("a","1");await journal.Apply("b","2");backend.Fail="b";await Assert.ThrowsAsync<AggregateException>(()=>journal.Restore());Assert.Equal("original",backend.Values["a"]);Assert.Equal("restore-failed",journal.Entries().Single(x=>x.Id=="b").Status);backend.Fail=null;await journal.Restore();Assert.Null(backend.Values["b"]);await journal.Restore();Assert.All(journal.Entries(),x=>Assert.Equal("restored",x.Status));}finally{if(Directory.Exists(dir))Directory.Delete(dir,true);}}
 [Fact] public async Task DuplicateDoesNotOverwriteOriginalBackup(){var dir=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString());try{var b=new Backend();var j=new ChangeJournal(Path.Combine(dir,"changes.json"),b);await j.Apply("a","1");await Assert.ThrowsAsync<InvalidOperationException>(()=>j.Apply("a","2"));await j.Restore("a");Assert.Equal("original",b.Values["a"]);}finally{if(Directory.Exists(dir))Directory.Delete(dir,true);}}
}
