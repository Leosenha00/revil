namespace ReviveOS.Backup;
/// <summary>Checks one explicitly selected file. No recursive polling or application RAM snapshots.</summary>
public sealed class BackupMonitor(string source,VersionStore store) {
 DateTime? lastWrite;long lastSize;
 public async Task<BackupEntry?> Tick(CancellationToken ct=default){var info=new FileInfo(source);if(!info.Exists)return null;if(info.LastWriteTimeUtc==lastWrite&&info.Length==lastSize)return null;
  // Do not copy an actively changing file: wait until its last write is at least 10 seconds old.
  if(DateTime.UtcNow-info.LastWriteTimeUtc<TimeSpan.FromSeconds(10))return null;
  var stamp=info.LastWriteTimeUtc;var size=info.Length;var result=await store.Backup(source,ct);info.Refresh();if(info.LastWriteTimeUtc==stamp&&info.Length==size){lastWrite=stamp;lastSize=size;}return result;
 }
}
