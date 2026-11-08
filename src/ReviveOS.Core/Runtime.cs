using System.Diagnostics;
using System.Text;
using System.Text.Json;
namespace ReviveOS.Core;
public static class State {
 public static string Root => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ReviveOS");
 public static readonly JsonSerializerOptions Json = new(){WriteIndented=true};
 public static void Save<T>(string path,T value) { Directory.CreateDirectory(Path.GetDirectoryName(path)!); var tmp=path+".tmp"; File.WriteAllText(tmp,JsonSerializer.Serialize(value,Json)); File.Move(tmp,path,true); }
}
public record CommandResult(int ExitCode,string Output,string Error);
public static class Commands {
 public static async Task<CommandResult> Run(string executable,IEnumerable<string> args,CancellationToken ct=default) {
  var si=new ProcessStartInfo(executable){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true};
  foreach(var arg in args) si.ArgumentList.Add(arg);
  using var p=Process.Start(si) ?? throw new IOException("Não foi possível iniciar processo");
  var output=p.StandardOutput.ReadToEndAsync(ct);var error=p.StandardError.ReadToEndAsync(ct);
  try {await p.WaitForExitAsync(ct);} catch {try{p.Kill(true);}catch(InvalidOperationException){} throw;}
  return new(p.ExitCode,await output,await error);
 }
 public static Task<CommandResult> PowerShell(string script,CancellationToken ct=default) {
  if(!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Requer Windows");
  return Run("powershell.exe",new[]{"-NoLogo","-NoProfile","-NonInteractive","-EncodedCommand",Convert.ToBase64String(Encoding.Unicode.GetBytes("$ErrorActionPreference='Stop';\n"+script))},ct);
 }
 public static async Task<string> CheckedPowerShell(string script,CancellationToken ct=default) {var r=await PowerShell(script,ct); if(r.ExitCode!=0)throw new IOException(r.Error);return r.Output;}
}
