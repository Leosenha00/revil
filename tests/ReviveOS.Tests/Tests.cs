using Xunit;
using ReviveOS.Backup;
using ReviveOS.HybridScheduler;
public class Tests {
 [Fact] public void UploadCostCanRejectRemote()=>Assert.Equal("LOCAL",Scheduler.Choose(new(20,1,1_000_000_000,0,1,10,30,true,true,true)).Mode);
 [Fact] public void UnauthorizedNeverUploads()=>Assert.Equal("LOCAL",Scheduler.Choose(new(200,1,1,0,100,100,1,true,false,true)).Mode);
 [Fact] public void CompatibleFastRemoteSelected()=>Assert.Equal("REMOTE",Scheduler.Choose(new(200,1,1,0,100,100,1,true,true,true)).Mode);
 [Fact] public async Task BackupRestoresAndDetectsCorruption(){var root=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString());Directory.CreateDirectory(root);try{var src=Path.Combine(root,"drawing.dxf");await File.WriteAllTextAsync(src,"original");var store=new VersionStore(Path.Combine(root,"versions"));var a=await store.Backup(src);await File.WriteAllTextAsync(src,"new version");var b=await store.Backup(src);Assert.NotEqual(a.Sha256,b.Sha256);var restored=Path.Combine(root,"restored.dxf");await store.Restore(a,restored);Assert.Equal("original",await File.ReadAllTextAsync(restored));await Assert.ThrowsAsync<IOException>(()=>store.Restore(a,src));await File.WriteAllTextAsync(a.ObjectPath,"corrupted");await Assert.ThrowsAsync<IOException>(()=>store.Restore(a,Path.Combine(root,"bad.dxf")));Assert.False(File.Exists(Path.Combine(root,"bad.dxf")));}finally{Directory.Delete(root,true);}}
}
