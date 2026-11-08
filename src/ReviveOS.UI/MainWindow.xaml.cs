using System.IO;
using System.Windows;
using System.Text.Json;
using ReviveOS.Core;
using ReviveOS.WindowsOptimizer;
using ReviveOS.Backup;
namespace ReviveOS.UI;
public partial class MainWindow:Window {
 readonly Optimizer optimizer=new();bool busy;
 public MainWindow(){InitializeComponent();Output.Text=ReviveOS.TurboEngine.Turbo.Status+"\n\n"+ReviveOS.WiFiManager.Wifi.Status;}
 async Task Run(Func<Task<string>> action){if(busy)return;busy=true;Status.Text="Em execução…";try{Output.Text=await action();Status.Text="Concluído. "+State.Root;}catch(Exception e){Status.Text="Falha; detalhes abaixo. Alterações pendentes podem ser restauradas.";Output.Text=e.ToString();}finally{busy=false;}}
 async void Diagnose(object s,RoutedEventArgs e)=>await Run(()=>new ReviveOS.Diagnostics.DiagnosticCollector().Collect());
 async void Simulate(object s,RoutedEventArgs e)=>await Run(async()=>await optimizer.Apply("visual")+"\n"+await optimizer.Apply("power"));
 async void Activate(object s,RoutedEventArgs e){if(busy)return;if(MessageBox.Show("Aplicar redução de animações e plano Alto Desempenho? Pode aumentar calor e consumo. O plano pode não existir. Um diário permitirá restaurar inclusive após falha. Efeitos visuais podem exigir novo login.","Confirmar ajustes",MessageBoxButton.YesNo)!=MessageBoxResult.Yes)return;await Run(async()=>{try{return await optimizer.Apply("visual",false)+"\n"+await optimizer.Apply("power",false);}catch{await optimizer.Restore();throw;}});}
 async void Restore(object s,RoutedEventArgs e)=>await Run(async()=>{await optimizer.Restore();return "Configurações registradas restauradas e verificadas.";});
 async void WifiClick(object s,RoutedEventArgs e)=>await Run(async()=>ReviveOS.WiFiManager.Wifi.Status+"\n"+await ReviveOS.WiFiManager.Wifi.Inspect());
 async void ProcessClick(object s,RoutedEventArgs e)=>await Run(()=>Task.FromResult(JsonSerializer.Serialize(ReviveOS.ProcessManager.Processes.Snapshot(),State.Json)));
 void CloudClick(object s,RoutedEventArgs e){if(busy)return;Output.Text=JsonSerializer.Serialize(ReviveOS.CloudEngine.Providers.Catalog,State.Json)+"\n"+ReviveOS.AutoCAD.AutoCad.Guidance;}
 async void BackupClick(object s,RoutedEventArgs e){if(busy)return;var d=new Microsoft.Win32.OpenFileDialog();if(d.ShowDialog()!=true)return;await Run(async()=>JsonSerializer.Serialize(await new VersionStore(Path.Combine(State.Root,"backups")).Backup(d.FileName),State.Json));}
 async void RecoverClick(object s,RoutedEventArgs e){if(busy)return;var d=new Microsoft.Win32.OpenFileDialog{InitialDirectory=Path.Combine(State.Root,"backups"),Filter="Manifesto de backup|*.json"};if(d.ShowDialog()!=true)return;var dest=new Microsoft.Win32.SaveFileDialog{Title="Restaurar para novo arquivo",OverwritePrompt=true};if(dest.ShowDialog()!=true)return;await Run(async()=>{var entry=JsonSerializer.Deserialize<BackupEntry>(File.ReadAllText(d.FileName))??throw new IOException("Manifesto inválido");await new VersionStore(Path.Combine(State.Root,"backups")).Restore(entry,dest.FileName);return "Arquivo restaurado com checksum verificado.";});}
}
