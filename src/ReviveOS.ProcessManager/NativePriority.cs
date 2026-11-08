using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32.SafeHandles;
namespace ReviveOS.ProcessManager;
public record ProcessIdentity(int Pid,long CreationTime,string Path,string Sid,int Session){public string Key=>Pid+":"+CreationTime;}
public record ProcessDescriptor(ProcessIdentity Identity,uint Priority,ulong CpuTime);
public interface IPriorityBackend {ProcessDescriptor? Describe(int pid);void Change(ProcessIdentity identity,uint expected,uint target);}
public sealed class NativePriorityBackend:IPriorityBackend {
 public const uint Normal=0x20,BelowNormal=0x4000,AboveNormal=0x8000;
 [StructLayout(LayoutKind.Sequential)]struct FileTime {public uint Low,High;public ulong Value=>((ulong)High<<32)|Low;}
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)][DllImport("kernel32.dll",SetLastError=true)]static extern SafeProcessHandle OpenProcess(uint access,bool inherit,uint pid);
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)][DllImport("kernel32.dll",SetLastError=true)]static extern bool GetProcessTimes(SafeProcessHandle process,out FileTime creation,out FileTime exit,out FileTime kernel,out FileTime user);
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)][DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern bool QueryFullProcessImageName(SafeProcessHandle process,uint flags,StringBuilder path,ref uint size);
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)][DllImport("kernel32.dll",SetLastError=true)]static extern bool ProcessIdToSessionId(uint pid,out uint session);
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)][DllImport("kernel32.dll",SetLastError=true)]static extern bool IsProcessCritical(SafeProcessHandle process,out bool critical);
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)][DllImport("kernel32.dll",SetLastError=true)]static extern uint GetPriorityClass(SafeProcessHandle process);
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)][DllImport("kernel32.dll",SetLastError=true)]static extern bool SetPriorityClass(SafeProcessHandle process,uint priority);
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)][DllImport("advapi32.dll",SetLastError=true)]static extern bool OpenProcessToken(SafeProcessHandle process,uint access,out SafeAccessTokenHandle token);
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)][DllImport("advapi32.dll",SetLastError=true)]static extern bool GetTokenInformation(SafeAccessTokenHandle token,int info,IntPtr data,uint size,out uint needed);
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)][DllImport("user32.dll")]static extern IntPtr GetForegroundWindow();
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)][DllImport("user32.dll")]static extern uint GetWindowThreadProcessId(IntPtr window,out uint pid);
 static void Check(bool success){if(!success)throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());}
 public static int ForegroundPid(){if(!OperatingSystem.IsWindows())return 0;GetWindowThreadProcessId(GetForegroundWindow(),out var pid);return (int)pid;}
 static ProcessDescriptor Read(SafeProcessHandle handle,int pid){if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException();Check(GetProcessTimes(handle,out var creation,out _,out var kernel,out var user));uint size=32768;var path=new StringBuilder((int)size);Check(QueryFullProcessImageName(handle,0,path,ref size));Check(ProcessIdToSessionId((uint)pid,out var session));Check(OpenProcessToken(handle,8,out var token));string sid;using(token){GetTokenInformation(token,1,IntPtr.Zero,0,out var needed);if(needed==0||needed>65536)throw new IOException("Token não consultável");var data=Marshal.AllocHGlobal((int)needed);try{Check(GetTokenInformation(token,1,data,needed,out _));sid=new SecurityIdentifier(Marshal.ReadIntPtr(data)).Value;}finally{Marshal.FreeHGlobal(data);}}
  var priority=GetPriorityClass(handle);if(priority==0)throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());return new(new(pid,(long)creation.Value,path.ToString(),sid,(int)session),priority,kernel.Value+user.Value);
 }
 public ProcessDescriptor? Describe(int pid){if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException();using var handle=OpenProcess(0x1000,false,(uint)pid);if(handle.IsInvalid){var error=Marshal.GetLastWin32Error();if(error is 87 or 1168)return null;throw new System.ComponentModel.Win32Exception(error);}return Read(handle,pid);}
 public static bool Same(ProcessIdentity a,ProcessIdentity b)=>a.Pid==b.Pid&&a.CreationTime==b.CreationTime&&a.Session==b.Session&&a.Sid==b.Sid&&string.Equals(a.Path,b.Path,StringComparison.OrdinalIgnoreCase);
 public void Change(ProcessIdentity identity,uint expected,uint target){if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException();if(target is not (Normal or BelowNormal or AboveNormal))throw new ArgumentException("Prioridade não permitida");if(identity.Pid==Environment.ProcessId)throw new InvalidOperationException("Processo do próprio agente protegido");using var handle=OpenProcess(0x1000|0x200,false,(uint)identity.Pid);if(handle.IsInvalid)throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());var current=Read(handle,identity.Pid);if(!Same(current.Identity,identity)||current.Priority!=expected)throw new IOException("Identidade/prioridade mudou; processo preservado");using var owner=WindowsIdentity.GetCurrent();Check(ProcessIdToSessionId((uint)Environment.ProcessId,out var session));if(current.Identity.Sid!=owner.User?.Value||current.Identity.Session!=session)throw new InvalidOperationException("Outra conta/sessão protegida");Check(IsProcessCritical(handle,out var critical));var windows=Environment.GetFolderPath(Environment.SpecialFolder.Windows)+Path.DirectorySeparatorChar;if(critical||current.Identity.Path.StartsWith(windows,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Processo do Windows/critico protegido");Check(SetPriorityClass(handle,target));if(GetPriorityClass(handle)!=target)throw new IOException("Prioridade não confirmada");
 }
}
