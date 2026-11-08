using System.Diagnostics;
using System.Runtime.InteropServices;
namespace ReviveOS.TurboEngine;
public record AppProfile(string Executable,Profile Profile);
public static class AppProfiles {
 public static readonly AppProfile[] Known=[new("acad",Profile.AUTOCAD_BOOST),new("chrome",Profile.BROWSER_BOOST),new("msedge",Profile.BROWSER_BOOST),new("firefox",Profile.BROWSER_BOOST),new("Code",Profile.DEVELOPMENT_BOOST),new("python",Profile.DEVELOPMENT_BOOST),new("blender",Profile.MULTIMEDIA_BOOST)];
 [DllImport("user32.dll")]static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")]static extern uint GetWindowThreadProcessId(IntPtr window,out uint pid);
 public static AppProfile? Foreground(){if(!OperatingSystem.IsWindows())return null;GetWindowThreadProcessId(GetForegroundWindow(),out var pid);if(pid==0)return null;try{using var p=Process.GetProcessById((int)pid);return Known.FirstOrDefault(x=>string.Equals(x.Executable,p.ProcessName,StringComparison.OrdinalIgnoreCase));}catch(ArgumentException){return null;}catch(System.ComponentModel.Win32Exception){return null;}}
}
