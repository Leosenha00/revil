using Xunit;
using ReviveOS.Core;
using ReviveOS.WindowsOptimizer;
public sealed class WindowsFactAttribute:FactAttribute {public WindowsFactAttribute(){if(!OperatingSystem.IsWindows())Skip="Requires Windows registry and powercfg";}}
public class WindowsSettingsTests {
 [WindowsFact] public async Task ExistingRegistryValuesSurviveApplyAndRestore(){
  if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException();
  using var key=Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Control Panel\Desktop\WindowMetrics");
  var sentinel="ReviveOSTest_"+Guid.NewGuid().ToString("N");key.SetValue(sentinel,"preserve-this");
  var dir=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N"));var backend=new WindowsSettings();var original=await backend.Read("visual");
  var journal=new ChangeJournal(Path.Combine(dir,"changes.json"),backend);
  try{await journal.Apply("visual",original=="0"?"1":"0");Assert.Equal("preserve-this",key.GetValue(sentinel));await journal.Restore();Assert.Equal(original,await backend.Read("visual"));Assert.Equal("preserve-this",key.GetValue(sentinel));}
  finally{try{await journal.Restore();}finally{key.DeleteValue(sentinel,false);if(Directory.Exists(dir))Directory.Delete(dir,true);}}
 }
 [WindowsFact] public async Task ActivePowerPlanHasRestorableGuid(){var value=await new WindowsSettings().Read("power");Assert.True(Guid.TryParse(value,out _));}
}
