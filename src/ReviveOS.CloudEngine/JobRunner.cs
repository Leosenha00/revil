using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using ReviveOS.Core;
namespace ReviveOS.CloudEngine;
public record JobRecord(string Id,string Mode,string Kind,string Input,string State,DateTime UpdatedUtc,string? Result,string? Error,string? RemoteHost=null,string? RemoteDirectory=null);
public sealed class JobRunner(string root) {
 readonly SemaphoreSlim gate=new(1,1);
 public IEnumerable<JobRecord> History()=>Directory.Exists(root)?Directory.GetFiles(root,"job.json",SearchOption.AllDirectories).Select(p=>System.Text.Json.JsonSerializer.Deserialize<JobRecord>(File.ReadAllText(p))!).OrderByDescending(x=>x.UpdatedUtc):[];
 public void RecoverInterrupted(){foreach(var job in History().Where(x=>x.State is "running" or "uploading" or "downloading")){Save(job with{State="interrupted",Error="Aplicativo interrompido. Arquivos confirmados preservados; estado do processo remoto desconhecido. Não repetir tarefa sem verificar efeitos."});}}
 void Save(JobRecord j)=>State.Save(Path.Combine(root,j.Id,"job.json"),j with{UpdatedUtc=DateTime.UtcNow});
 static string Hash(string path){using var f=File.OpenRead(path);return Convert.ToHexString(SHA256.HashData(f));}
 public async Task<JobRecord> Run(string kind,string input,string? sshAlias,bool uploadAuthorized,CancellationToken ct=default) {
  if(kind is not ("python" or "ffmpeg" or "blender"))throw new ArgumentException("Adaptador desconhecido");
  if(sshAlias!=null&&(!uploadAuthorized||!Regex.IsMatch(sshAlias,@"\A[a-zA-Z0-9][a-zA-Z0-9._-]{0,63}\z")))throw new ArgumentException("Autorize upload e configure um alias SSH válido");
  await gate.WaitAsync(ct);try {
   var id=Guid.NewGuid().ToString("N");var dir=Path.GetFullPath(Path.Combine(root,id));Directory.CreateDirectory(dir);
   var inputName=kind switch{"python"=>"input.py","blender"=>"input.blend",_=>"input.media"};var resultName=kind=="python"?"result.json":kind=="blender"?"result.png":"result.mp4";
   var staged=Path.Combine(dir,inputName);File.Copy(input,staged);State.Save(Path.Combine(dir,"input.sha256.json"),new{Sha256=Hash(staged),Original=Path.GetFullPath(input)});
   var remote=sshAlias==null?null:".reviveos/jobs/"+id;
   var job=new JobRecord(id,sshAlias==null?"LOCAL":"REMOTE",kind,Path.GetFullPath(input),"running",DateTime.UtcNow,null,null,sshAlias,remote);Save(job);
   async Task Execute(string exe,IEnumerable<string> args){await ExecuteLogged(exe,args,dir,Path.Combine(dir,"execution.log"),ct);}
   try {
    var command=kind=="python"?(OperatingSystem.IsWindows()?"python":"python3"):kind;
    string[] arguments=kind switch{"python"=>[inputName],"ffmpeg"=>["-nostdin","-n","-i",inputName,"-c:v","libx264","-preset","fast","-c:a","aac",resultName],_=>["-b",inputName,"-o","result","-F","PNG","-f","1"]};
    if(sshAlias==null) await Execute(command,arguments);
    else {
     job=job with{State="uploading"};Save(job);
     string[] ssh=["-o","ServerAliveInterval=15","-o","ServerAliveCountMax=2","-o","ConnectTimeout=10","-o","BatchMode=yes","-o","StrictHostKeyChecking=yes","-o","ConnectTimeout=10",sshAlias];
     await Execute("ssh",ssh.Concat(["mkdir -p "+remote]));
     await Execute("scp",new[]{"-o","ServerAliveInterval=15","-o","ServerAliveCountMax=2","-o","ConnectTimeout=10","-o","BatchMode=yes","-o","StrictHostKeyChecking=yes",staged,sshAlias+":"+remote+"/"+inputName});
     job=job with{State="running"};Save(job);
     // All remote shell words below are constants, generated identifiers or validated aliases.
     await Execute("ssh",ssh.Concat(["cd "+remote+" && "+(kind=="python"?"python3":kind)+" "+string.Join(' ',arguments)]));
     job=job with{State="downloading"};Save(job);
     var remoteFile=kind=="blender"?"result0001.png":resultName;
     await Execute("scp",new[]{"-o","ServerAliveInterval=15","-o","ServerAliveCountMax=2","-o","ConnectTimeout=10","-o","BatchMode=yes","-o","StrictHostKeyChecking=yes",sshAlias+":"+remote+"/"+remoteFile,Path.Combine(dir,resultName+".partial")});
     await Execute("ssh",ssh.Concat(["cd "+remote+" && sha256sum "+remoteFile]));
     // Obtain checksum separately for strict parsing, leaving a trace in the session log.
     var checksum=await Commands.Run("ssh",ssh.Concat(["cd "+remote+" && sha256sum "+remoteFile]),ct);if(checksum.ExitCode!=0)throw new IOException(checksum.Error);
     var hash=checksum.Output.Split(' ',StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();if(!string.Equals(hash,Hash(Path.Combine(dir,resultName+".partial")),StringComparison.OrdinalIgnoreCase))throw new IOException("Checksum remoto divergiu");
     File.Move(Path.Combine(dir,resultName+".partial"),Path.Combine(dir,resultName));
    }
    var output=Path.Combine(dir,sshAlias==null&&kind=="blender"?"result0001.png":resultName);if(!File.Exists(output))throw new IOException("Processo terminou sem arquivo de resultado esperado: "+output);
    State.Save(Path.Combine(dir,"result.sha256.json"),new{Sha256=Hash(output)});job=job with{State="completed",Result=output};Save(job);return job;
   }catch(Exception e){job=job with{State=e is OperationCanceledException?(sshAlias==null?"cancelled":"remote-state-unknown"):"failed",Error=e.Message};Save(job);throw;}
  }finally{gate.Release();}
 }
 public async Task<JobRecord> RetrieveRemoteResult(string id,CancellationToken ct=default){
  if(!Regex.IsMatch(id,@"\A[a-f0-9]{32}\z"))throw new ArgumentException("Identificador inválido");await gate.WaitAsync(ct);try {
   var job=History().Single(x=>x.Id==id);var host=job.RemoteHost;if(job.Mode!="REMOTE"||host==null||!Regex.IsMatch(host,@"\A[a-zA-Z0-9][a-zA-Z0-9._-]{0,63}\z"))throw new IOException("Host inválido");
   var remote=".reviveos/jobs/"+id;var file=job.Kind switch{"python"=>"result.json","ffmpeg"=>"result.mp4","blender"=>"result0001.png",_=>throw new IOException("Adaptador desconhecido")};var dir=Path.GetFullPath(Path.Combine(root,id));var destination=Path.Combine(dir,file);if(File.Exists(destination))throw new IOException("Resultado local já existe; consulte histórico");
   // Only retrieve a confirmed file. No execution or resume of the original process.
   var before=await Commands.Run("ssh",["-o","ServerAliveInterval=15","-o","ServerAliveCountMax=2","-o","ConnectTimeout=10","-o","BatchMode=yes","-o","StrictHostKeyChecking=yes","-o","ConnectTimeout=10",host,"cd "+remote+" && sha256sum "+file],ct);if(before.ExitCode!=0)throw new IOException(before.Error);
   await ExecuteLogged("scp",["-o","ServerAliveInterval=15","-o","ServerAliveCountMax=2","-o","ConnectTimeout=10","-o","BatchMode=yes","-o","StrictHostKeyChecking=yes",host+":"+remote+"/"+file,destination+".partial"],dir,Path.Combine(dir,"recovery.log"),ct);
   var expected=before.Output.Split(' ',StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();if(!string.Equals(expected,Hash(destination+".partial"),StringComparison.OrdinalIgnoreCase))throw new IOException("Resultado mudou ou checksum divergiu; aguarde término do processo remoto");
   File.Move(destination+".partial",destination);State.Save(Path.Combine(dir,"result.sha256.json"),new{Sha256=Hash(destination)});job=job with{State="recovered-file",Result=destination,Error=null};Save(job);return job;
  }finally{gate.Release();}
 }
 static async Task ExecuteLogged(string exe,IEnumerable<string> args,string cwd,string log,CancellationToken ct){
  var si=new ProcessStartInfo(exe){WorkingDirectory=cwd,UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true};foreach(var argument in args)si.ArgumentList.Add(argument);
  using var p=Process.Start(si)??throw new IOException("Não foi possível iniciar "+exe);
  // Bounded log: consumes all streams while retaining at most 4 MiB per invocation.
  using var writer=new StreamWriter(log,true);long written=0;var sync=new SemaphoreSlim(1,1);
  async Task Drain(StreamReader reader){var buffer=new char[4096];int n;while((n=await reader.ReadAsync(buffer))>0){await sync.WaitAsync();try{if(written<4*1024*1024){await writer.WriteAsync(buffer.AsMemory(0,n));written+=n;}}finally{sync.Release();}}}
  var a=Drain(p.StandardOutput);var b=Drain(p.StandardError);
  try{await p.WaitForExitAsync(ct);}catch{try{p.Kill(true);}catch(InvalidOperationException){}await Task.WhenAll(a,b);throw;}
  await Task.WhenAll(a,b);if(p.ExitCode!=0)throw new IOException($"{exe}: código {p.ExitCode}; consulte {log}");
 }
}
