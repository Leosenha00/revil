using System.Diagnostics;
using System.Runtime.InteropServices;
using ReviveOS.Core;
namespace ReviveOS.Diagnostics;
/// <summary>No WMI, PowerShell, disk benchmark or network polling.</summary>
public sealed class NativeResourceSampler:IDisposable {
 [StructLayout(LayoutKind.Sequential)]struct FileTime {public uint Low,High;public ulong Value=>((ulong)High<<32)|Low;}
 [StructLayout(LayoutKind.Sequential)]struct Memory {public uint Length,Load;public ulong TotalPhys,AvailPhys,TotalPageFile,AvailPageFile,TotalVirtual,AvailVirtual,AvailExtended;}
 [StructLayout(LayoutKind.Sequential)]struct Power {public byte Ac,Battery,Percent,Reserved;public uint Life,FullLife;}
 [StructLayout(LayoutKind.Sequential)]struct Counter {public uint Status;public double Value;}
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)][DllImport("kernel32.dll",SetLastError=true)]static extern bool GetSystemTimes(out FileTime idle,out FileTime kernel,out FileTime user);
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)][DllImport("kernel32.dll",SetLastError=true)]static extern bool GlobalMemoryStatusEx(ref Memory memory);
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)][DllImport("kernel32.dll")]static extern bool GetSystemPowerStatus(out Power power);
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)][DllImport("pdh.dll",CharSet=CharSet.Unicode)]static extern uint PdhOpenQuery(string? source,IntPtr data,out IntPtr query);
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)][DllImport("pdh.dll",CharSet=CharSet.Unicode)]static extern uint PdhAddEnglishCounter(IntPtr query,string path,IntPtr data,out IntPtr counter);
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)][DllImport("pdh.dll")]static extern uint PdhCollectQueryData(IntPtr query);
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)][DllImport("pdh.dll")]static extern uint PdhGetFormattedCounterValue(IntPtr counter,uint format,out uint type,out Counter value);
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)][DllImport("pdh.dll")]static extern uint PdhCloseQuery(IntPtr query);
 readonly Process self=Process.GetCurrentProcess();readonly Stopwatch clock=Stopwatch.StartNew();IntPtr query,disk;ulong? lastIdle,lastTotal;double? lastWall,lastOwn;
 public NativeResourceSampler(){if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException();if(PdhOpenQuery(null,IntPtr.Zero,out query)==0){if(PdhAddEnglishCounter(query,@"\PhysicalDisk(_Total)\Avg. Disk Queue Length",IntPtr.Zero,out disk)!=0){PdhCloseQuery(query);query=IntPtr.Zero;}}}
 public ResourceSample Read(){if(!GetSystemTimes(out var idle,out var kernel,out var user))throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());var total=kernel.Value+user.Value;double? cpu=null;if(lastTotal.HasValue&&lastIdle.HasValue&&total>lastTotal.Value&&idle.Value>=lastIdle.Value){var dt=total-lastTotal.Value;var di=idle.Value-lastIdle.Value;if(di<=dt)cpu=(dt-di)*100d/dt;}lastTotal=total;lastIdle=idle.Value;
  var memory=new Memory{Length=(uint)Marshal.SizeOf<Memory>()};if(!GlobalMemoryStatusEx(ref memory))throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());double? queue=null;if(query!=IntPtr.Zero&&PdhCollectQueryData(query)==0&&PdhGetFormattedCounterValue(disk,0x200,out _,out var value)==0&&value.Status is 0 or 1&&double.IsFinite(value.Value)&&value.Value>=0)queue=value.Value;
  bool? battery=null;if(GetSystemPowerStatus(out var power))battery=power.Ac switch{0=>true,1=>false,_=>null};self.Refresh();var wall=clock.Elapsed.TotalSeconds;var own=self.TotalProcessorTime.TotalSeconds;double? overhead=null;if(lastWall.HasValue&&wall>lastWall.Value&&lastOwn.HasValue)overhead=Math.Max(0,(own-lastOwn.Value)/(wall-lastWall.Value)/Environment.ProcessorCount*100);lastWall=wall;lastOwn=own;
  return new(DateTime.UtcNow,cpu,(long)(memory.AvailPhys/1048576),(long)(memory.TotalPhys/1048576),queue,battery,overhead,self.WorkingSet64/1048576d);
 }
 public void Dispose(){if(query!=IntPtr.Zero){PdhCloseQuery(query);query=IntPtr.Zero;}self.Dispose();}
}
