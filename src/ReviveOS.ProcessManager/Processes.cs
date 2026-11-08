using System.Diagnostics;
namespace ReviveOS.ProcessManager;
public record ProcessInfo(int Pid,string Name,long RamBytes,string Category);
public static class Processes {
 public static ProcessInfo[] Snapshot() {var result=new List<ProcessInfo>();foreach(var p in Process.GetProcesses()){using(p){try{result.Add(new(p.Id,p.ProcessName,p.WorkingSet64,p.SessionId==0?"ESSENCIAL":p.WorkingSet64>512L*1024*1024?"PESADO":"IMPORTANTE"));}catch(System.ComponentModel.Win32Exception){}catch(InvalidOperationException){}}}return result.OrderByDescending(x=>x.RamBytes).ToArray();}
 // Somente fechamento cooperativo. O aplicativo pode apresentar confirmação de salvamento.
 public static bool RequestClose(int pid,string expectedName) {using var p=Process.GetProcessById(pid);if(p.SessionId==0||p.Id==Environment.ProcessId||p.ProcessName!=expectedName||new[]{"system","registry","smss","csrss","wininit","services","lsass","svchost","winlogon","dwm","explorer","securityhealthservice","msmpeng"}.Contains(p.ProcessName.ToLowerInvariant()))throw new InvalidOperationException("Processo protegido ou identidade alterada");return p.CloseMainWindow();}
}
