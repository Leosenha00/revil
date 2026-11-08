using Xunit;
using ReviveOS.Security;
using ReviveOS.WindowsOptimizer;
using ReviveOS.Core;
using System.Runtime.InteropServices;
public class AdminCpuTests {
 [Fact]public void BrokerRejectsArbitraryCommandsAndTargets(){Assert.Throws<ArgumentException>(()=>AdminBroker.Validate("inject-dll",[]));Assert.Throws<ArgumentException>(()=>AdminBroker.Validate("powershell",[]));Assert.Throws<ArgumentException>(()=>AdminBroker.Validate("deep-apply",["service:WinDefend"]));Assert.Throws<ArgumentException>(()=>AdminBroker.Validate("check",["pagefile"]));AdminBroker.Validate("deep-apply",["service:SysMain","pagefile"]);}
 [Fact]public void CpuIdMustNameExplicitScheme(){var id=Guid.NewGuid();Assert.Equal(id,CpuPowerBackend.Parse("cpu-ac-max:"+id));Assert.Throws<ArgumentException>(()=>CpuPowerBackend.Parse("cpu-ac-max:scheme_current"));Assert.Throws<ArgumentException>(()=>CpuPowerBackend.Parse("firmware:unlock"));}
 [DllImport("PowrProf.dll")]static extern uint PowerDuplicateScheme(IntPtr root,ref Guid source,out IntPtr copy);
 [DllImport("PowrProf.dll")]static extern uint PowerDeleteScheme(IntPtr root,ref Guid scheme);
 [DllImport("kernel32.dll")]static extern IntPtr LocalFree(IntPtr pointer);
 [WindowsFact]public async Task CpuLimitRoundtripUsesOnlyAnInactiveTestScheme(){var active=CpuPowerBackend.ActiveScheme();Assert.Equal(0u,PowerDuplicateScheme(IntPtr.Zero,ref active,out var pointer));var copy=Marshal.PtrToStructure<Guid>(pointer);LocalFree(pointer);var dir=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N"));try{var backend=new CpuPowerBackend();var id="cpu-ac-max:"+copy;await backend.Write(id,"50");var journal=new ChangeJournal(Path.Combine(dir,"changes.json"),backend);await journal.Apply(id,"100");Assert.Equal("100",await backend.Read(id));await journal.Restore();Assert.Equal("50",await backend.Read(id));Assert.Equal(active,CpuPowerBackend.ActiveScheme());}finally{Assert.Equal(0u,PowerDeleteScheme(IntPtr.Zero,ref copy));if(Directory.Exists(dir))Directory.Delete(dir,true);}}
}
