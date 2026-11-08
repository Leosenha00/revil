using Microsoft.Win32;
using System.Security.Principal;
using ReviveOS.Core;
using ReviveOS.TurboEngine;
using ReviveOS.ProcessManager;
if(!OperatingSystem.IsWindows()||args.Length!=3)return 2;
var executable=Path.GetFullPath(args[1]);
using var user=WindowsIdentity.GetCurrent();
if(new WindowsPrincipal(user).IsInRole(WindowsBuiltInRole.Administrator))throw new Exception("Probe must run as a standard user");
const string run=@"Software\Microsoft\Windows\CurrentVersion\Run";
if(args[0]=="start"){
 await ContinuousClient.Start(new(){Enabled=true,StartAtLogin=true,FavorForeground=false},executable);
 using var key=Registry.CurrentUser.OpenSubKey(run);
 if((string?)key?.GetValue("ReviveOSContinuous")!="\""+executable+"\"")throw new Exception("Startup registration missing");
 State.Save(args[2],new{Started=true,Elevated=false,StartupRegistered=true});
 return 0;
}
if(args[0]=="verify-stop"){
 var status=ContinuousClient.Status()??throw new Exception("No status after parent exited");
 if(status.State!="running"||DateTime.UtcNow-status.Utc>TimeSpan.FromSeconds(45))throw new Exception("Agent not running after parent exit");
 var identity=new NativePriorityBackend().Describe(status.Pid)?.Identity;
 if(identity?.Sid!=user.User?.Value||!string.Equals(identity.Path,executable,StringComparison.OrdinalIgnoreCase))throw new Exception("Wrong process/user");
 await ContinuousClient.Stop();
 using(var lease=ContinuousClient.Lease()){}
 using var key=Registry.CurrentUser.OpenSubKey(run);
 if(key?.GetValue("ReviveOSContinuous")!=null||ContinuousClient.Config().Enabled)throw new Exception("Stop did not disable startup");
 if(ContinuousClient.Status()?.State!="stopped")throw new Exception("Agent did not report stop");
 State.Save(args[2],new{Started=true,Elevated=false,StartupRegistered=true,SurvivedParentExit=true,Stopped=true,StartupRemoved=true,Sample=status.Sample});
 return 0;
}
return 2;
