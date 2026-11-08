using ReviveOS.Core;
namespace ReviveOS.Diagnostics;
public sealed class DiagnosticCollector {
 public async Task<string> Collect(CancellationToken ct=default) {
  var script=File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"scripts","diagnose.ps1"));
  var data=await Commands.CheckedPowerShell(script,ct);
  var path=Path.Combine(State.Root,"reports",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+".json");Directory.CreateDirectory(Path.GetDirectoryName(path)!);await File.WriteAllTextAsync(path,data,ct);return data;
 }
}
