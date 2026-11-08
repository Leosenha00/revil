using System.Security.Cryptography;
using ReviveOS.Core;
namespace ReviveOS.Backup;
public record BackupEntry(string Original,string ObjectPath,string Sha256,long Size,DateTime Utc);
public sealed class VersionStore(string root) {
 public async Task<BackupEntry> Backup(string source,CancellationToken ct=default) {
  source=Path.GetFullPath(source);Directory.CreateDirectory(root);var tmp=Path.Combine(root,Guid.NewGuid()+".partial");
  try {await using(var input=new FileStream(source,FileMode.Open,FileAccess.Read,FileShare.Read)) await using(var output=File.Create(tmp)) await input.CopyToAsync(output,ct);
   await using var read=File.OpenRead(tmp);var hash=Convert.ToHexString(await SHA256.HashDataAsync(read,ct));var size=read.Length;read.Close();var dest=Path.Combine(root,hash+".bin");
   if(!File.Exists(dest))File.Move(tmp,dest);else File.Delete(tmp);
   var entry=new BackupEntry(source,dest,hash,size,DateTime.UtcNow);State.Save(Path.Combine(root,Guid.NewGuid()+".json"),entry);return entry;
  } finally{if(File.Exists(tmp))File.Delete(tmp);}
 }
 public async Task Restore(BackupEntry entry,string destination,CancellationToken ct=default) {
  if(File.Exists(destination))throw new IOException("Destino já existe; escolha novo nome");
  var tmp=destination+"."+Guid.NewGuid()+".partial";
  try {await using(var input=File.OpenRead(entry.ObjectPath))await using(var output=File.Create(tmp))await input.CopyToAsync(output,ct);
   await using var read=File.OpenRead(tmp);var hash=Convert.ToHexString(await SHA256.HashDataAsync(read,ct));read.Close();if(hash!=entry.Sha256)throw new IOException("Checksum inválido");File.Move(tmp,destination);
  }finally{if(File.Exists(tmp))File.Delete(tmp);}
 }
}
