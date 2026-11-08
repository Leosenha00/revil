using System.Windows;
namespace ReviveOS.UI;
public partial class App:Application {
 System.Threading.Mutex? instance;
 protected override async void OnStartup(StartupEventArgs e){base.OnStartup(e);instance=new System.Threading.Mutex(true,"Local\\ReviveOS-"+Environment.UserName+(e.Args.Contains("--wifi-recover")?"-wifi":""),out var acquired);if(!acquired){Shutdown();return;}
  if(e.Args.Contains("--wifi-recover")){try{await ReviveOS.WiFiManager.NativeWifi.RecoverBounded();}catch(Exception ex){ReviveOS.Core.State.Save(System.IO.Path.Combine(ReviveOS.Core.State.Root,"wifi-fatal.json"),ex.Message);}Shutdown();return;}
  new MainWindow().Show();
 }
 protected override void OnExit(ExitEventArgs e){instance?.Dispose();base.OnExit(e);}
}
