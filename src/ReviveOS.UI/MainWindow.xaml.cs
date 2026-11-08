using System.IO;
using System.Windows;
using System.Text.Json;
using ReviveOS.Core;
using ReviveOS.WindowsOptimizer;
using ReviveOS.Backup;
namespace ReviveOS.UI;
public partial class MainWindow:Window {
 readonly Optimizer optimizer=new();readonly DeepOptimizer deep=new();bool busy;bool auto;
 readonly System.Windows.Threading.DispatcherTimer monitor=new(){Interval=TimeSpan.FromSeconds(5)};
 readonly ReviveOS.CloudEngine.JobRunner jobs=new(Path.Combine(State.Root,"jobs"));
 CancellationTokenSource? jobCancellation;
 bool autoApplied;
 ReviveOS.Backup.BackupMonitor? backupMonitor;
 readonly System.Windows.Threading.DispatcherTimer backupTimer=new(){Interval=TimeSpan.FromSeconds(60)};


 public MainWindow(){InitializeComponent();try{jobs.RecoverInterrupted();}catch(Exception ex){Status.Text="Histórico de tarefas inválido: "+ex.Message;}monitor.Tick+=MonitorTick;backupTimer.Tick+=BackupTick;Closing+=OnClosing;Output.Text=ReviveOS.TurboEngine.Turbo.Status+"\n\n"+ReviveOS.WiFiManager.Wifi.Status;try{if(optimizer.HasPending||deep.HasPending)Status.Text="Há ajustes pendentes de sessão anterior. Use DESATIVAR E RESTAURAR antes de aplicar novamente.";}catch(Exception ex){Status.Text="Diário inválido; ajustes bloqueados: "+ex.Message;}}
 async Task Run(Func<Task<string>> action){if(busy)return;busy=true;Status.Text="Em execução…";try{Output.Text=await action();Status.Text="Concluído. "+State.Root;}catch(Exception e){Status.Text="Falha; detalhes abaixo. Alterações pendentes podem ser restauradas.";Output.Text=e.ToString();}finally{busy=false;}}
 async void Diagnose(object s,RoutedEventArgs e)=>await Run(()=>new ReviveOS.Diagnostics.DiagnosticCollector().Collect());
 async void Simulate(object s,RoutedEventArgs e)=>await Run(async()=>await optimizer.Apply("visual")+"\n"+await optimizer.Apply("power"));
 async void Activate(object s,RoutedEventArgs e){if(busy||auto)return;if(MessageBox.Show("Aplicar redução de animações e plano Alto Desempenho? Pode aumentar calor e consumo. O plano pode não existir. Um diário permitirá restaurar inclusive após falha. Efeitos visuais podem exigir novo login.","Confirmar ajustes",MessageBoxButton.YesNo)!=MessageBoxResult.Yes)return;await Run(async()=>{try{return await optimizer.Apply("visual",false)+"\n"+await optimizer.Apply("power",false);}catch{await optimizer.Restore();throw;}});}
 async void Restore(object s,RoutedEventArgs e)=>await Run(async()=>{auto=false;monitor.Stop();autoApplied=false;var errors=new List<Exception>();try{await optimizer.Restore();}catch(Exception ex){errors.Add(ex);}try{await deep.Restore();}catch(Exception ex){errors.Add(ex);}if(errors.Count>0)throw new AggregateException(errors);return "Configurações registradas restauradas e verificadas.";});
 async void WifiClick(object s,RoutedEventArgs e)=>await Run(async()=>ReviveOS.WiFiManager.Wifi.Status+"\n"+await ReviveOS.WiFiManager.Wifi.Inspect());
 async void ProcessClick(object s,RoutedEventArgs e)=>await Run(()=>Task.FromResult(JsonSerializer.Serialize(ReviveOS.ProcessManager.Processes.Snapshot(),State.Json)));
 void CloudClick(object s,RoutedEventArgs e){if(busy)return;Output.Text=JsonSerializer.Serialize(ReviveOS.CloudEngine.Providers.Catalog,State.Json)+"\n"+ReviveOS.AutoCAD.AutoCad.Guidance;}
 async void BackupClick(object s,RoutedEventArgs e){if(busy)return;var d=new Microsoft.Win32.OpenFileDialog();if(d.ShowDialog()!=true)return;await Run(async()=>JsonSerializer.Serialize(await new VersionStore(Path.Combine(State.Root,"backups")).Backup(d.FileName),State.Json));}
 async void RecoverClick(object s,RoutedEventArgs e){if(busy)return;var d=new Microsoft.Win32.OpenFileDialog{InitialDirectory=Path.Combine(State.Root,"backups"),Filter="Manifesto de backup|*.json"};if(d.ShowDialog()!=true)return;var dest=new Microsoft.Win32.SaveFileDialog{Title="Restaurar para novo arquivo",OverwritePrompt=true};if(dest.ShowDialog()!=true)return;await Run(async()=>{var entry=JsonSerializer.Deserialize<BackupEntry>(File.ReadAllText(d.FileName))??throw new IOException("Manifesto inválido");await new VersionStore(Path.Combine(State.Root,"backups")).Restore(entry,dest.FileName);return "Arquivo restaurado com checksum verificado.";});}

 async void MetricsClick(object s,RoutedEventArgs e)=>await Run(async()=>{var sample=await ReviveOS.Diagnostics.Benchmarks.Sample();var disk=await ReviveOS.Diagnostics.Benchmarks.Storage();var report=sample+"\n"+JsonSerializer.Serialize(disk,State.Json);State.Save(Path.Combine(State.Root,"benchmarks",Guid.NewGuid()+"-samples.json"),sample);return report;});
 async void RadioClick(object s,RoutedEventArgs e){if(busy)return;if(MessageBox.Show("Ligar rádio Wi-Fi por software pela API Windows? Não altera bloqueio físico ou firmware.","Wi-Fi",MessageBoxButton.YesNo)!=MessageBoxResult.Yes)return;await Run(()=>Task.FromResult(JsonSerializer.Serialize(ReviveOS.WiFiManager.NativeWifi.EnableSoftwareRadio(),State.Json)));}
 async void RadioRestoreClick(object s,RoutedEventArgs e)=>await Run(async()=>{await ReviveOS.WiFiManager.NativeWifi.SetLoginTask(false,Environment.ProcessPath!);ReviveOS.WiFiManager.NativeWifi.RestoreSoftwareRadio();return "Tarefa removida e estado anterior de rádio restaurado quando havia backup.";});
 async void WifiTaskClick(object s,RoutedEventArgs e){if(busy)return;if(MessageBox.Show("Criar tarefa no login deste usuário com até três tentativas? Não garante funcionamento antes do login. Valide reinício, partida a frio e suspensão no notebook.","Wi-Fi automático",MessageBoxButton.YesNo)!=MessageBoxResult.Yes)return;await Run(()=>ReviveOS.WiFiManager.NativeWifi.SetLoginTask(true,Environment.ProcessPath!));}
 async void WifiRemoveClick(object s,RoutedEventArgs e)=>await Run(()=>ReviveOS.WiFiManager.NativeWifi.SetLoginTask(false,Environment.ProcessPath!));
 async void AutoClick(object s,RoutedEventArgs e){if(busy)return;if(auto){await Run(async()=>{monitor.Stop();auto=false;if(autoApplied)await optimizer.Restore();autoApplied=false;return "Monitor desativado; ajustes restaurados.";});return;}
  if(optimizer.HasPending||deep.HasPending){Status.Text="Restaure ajustes pendentes antes de ativar monitor.";return;}
  if(MessageBox.Show("Monitorar aplicativo em primeiro plano a cada 5 segundos? AutoCAD, navegadores, VS Code, Python e Blender ativam apenas redução de animações e transparência. As configurações voltam ao sair desses aplicativos ou fechar ReviveOS. Mantenha o ReviveOS aberto. Sem mudança de energia ou encerramento de processos.","Automação por aplicativo",MessageBoxButton.YesNo)!=MessageBoxResult.Yes)return;
  auto=true;monitor.Start();Status.Text="Monitor de aplicativos ativo.";
 }
 async void MonitorTick(object? sender,EventArgs e){if(busy||!auto)return;var profile=ReviveOS.TurboEngine.AppProfiles.Foreground();if(profile!=null&&!autoApplied){await Run(async()=>{autoApplied=true;try{await optimizer.Apply("visual",false);await optimizer.Apply("transparency",false);}catch{monitor.Stop();auto=false;await optimizer.Restore();autoApplied=false;throw;}return "Perfil detectado: "+profile.Profile+"; animações e transparência reduzidas.";});}else if(profile==null&&autoApplied){await Run(async()=>{await optimizer.Restore();autoApplied=false;return "Aplicativo em primeiro plano sem perfil; configurações restauradas.";});}}
 async void OnClosing(object? sender,System.ComponentModel.CancelEventArgs e){if(busy){e.Cancel=true;Status.Text="Aguarde a operação ou cancele a tarefa antes de fechar.";return;}if(autoApplied){e.Cancel=true;monitor.Stop();auto=false;await Run(async()=>{await optimizer.Restore();autoApplied=false;return "Restaurado.";});if(!autoApplied)Close();}}
 async void HistoryClick(object s,RoutedEventArgs e)=>await Run(()=>Task.FromResult(optimizer.History()+"\n"+deep.History()+"\n"+JsonSerializer.Serialize(jobs.History(),State.Json)));
 void CancelClick(object s,RoutedEventArgs e)=>jobCancellation?.Cancel();
 async void JobClick(object s,RoutedEventArgs e)=>await StartJob(null);
 async void SshClick(object s,RoutedEventArgs e){if(busy)return;var dialog=new SshDialog{Owner=this};if(dialog.ShowDialog()==true)await StartJob(dialog.Alias);}
 async Task StartJob(string? alias){if(busy)return;var d=new Microsoft.Win32.OpenFileDialog{Filter="Python (gera result.json)|*.py|Vídeo para MP4|*.mp4;*.avi;*.mkv;*.mov|Blender: renderizar frame 1|*.blend"};if(d.ShowDialog()!=true)return;var kind=d.FilterIndex==1?"python":d.FilterIndex==2?"ffmpeg":"blender";
  if(MessageBox.Show((alias==null?"Executar localmente":"Enviar o arquivo selecionado ao host SSH autorizado '"+alias+"' e executar nele")+"? Python pode executar código com as permissões da conta. Blender deve ser arquivo confiável. Ferramentas precisam estar instaladas. Nenhuma porcentagem de progresso será inventada. Cancelamento SSH não garante término do processo remoto.","Executar tarefa",MessageBoxButton.YesNo)!=MessageBoxResult.Yes)return;
  jobCancellation=new();await Run(async()=>{var job=await jobs.Run(kind,d.FileName,alias,alias!=null,jobCancellation.Token);return JsonSerializer.Serialize(job,State.Json);});jobCancellation.Dispose();jobCancellation=null;
 }

 void ProcessManageClick(object s,RoutedEventArgs e){if(!busy)new ProcessDialog{Owner=this}.ShowDialog();}
 void SettingsClick(object s,RoutedEventArgs e){if(!busy&&!auto)new SettingsDialog(optimizer){Owner=this}.ShowDialog();}
 async void BackupMonitorClick(object s,RoutedEventArgs e){if(busy)return;if(backupMonitor!=null){backupTimer.Stop();backupMonitor=null;Status.Text="Backup periódico desativado.";return;}var d=new Microsoft.Win32.OpenFileDialog();if(d.ShowDialog()!=true)return;backupMonitor=new(d.FileName,new VersionStore(Path.Combine(State.Root,"backups")));backupTimer.Start();await Run(()=>Task.FromResult("Backup periódico ativo para "+d.FileName+". Verifica alterações a cada 60 segundos com ReviveOS aberto; ignora arquivo alterado há menos de 10 segundos. Clique novamente para desativar."));}
 async void BackupTick(object? s,EventArgs e){if(busy||backupMonitor==null)return;busy=true;try{var result=await backupMonitor.Tick();if(result!=null)Status.Text="Backup confirmado: "+result.Utc;}catch(Exception ex){Status.Text="Backup periódico falhou: "+ex.Message;}finally{busy=false;}}
 async void RemoteRecoverClick(object s,RoutedEventArgs e){if(busy)return;var picker=new JobRecoveryDialog(jobs.History().Where(x=>x.Mode=="REMOTE"&&x.State!="completed"&&x.State!="recovered-file")){Owner=this};if(picker.ShowDialog()!=true)return;jobCancellation=new();await Run(async()=>JsonSerializer.Serialize(await jobs.RetrieveRemoteResult(picker.SelectedId!,jobCancellation.Token),State.Json));jobCancellation.Dispose();jobCancellation=null;}
 void RdpClick(object s,RoutedEventArgs e){if(busy)return;var dialog=new SshDialog{Owner=this,Title="Host Windows RDP autorizado (nome ou IPv4)"};if(dialog.ShowDialog()!=true)return;if(MessageBox.Show("Abrir cliente RDP para "+dialog.Alias+"? Confirme que você tem autorização. A licença do Windows remoto deve suportar RDP; nenhuma máquina é fornecida pelo ReviveOS.","RDP",MessageBoxButton.YesNo)!=MessageBoxResult.Yes)return;try{ReviveOS.RemoteApps.RemoteDesktop.OpenRdp(dialog.Alias,true);}catch(Exception ex){Status.Text=ex.Message;}}
 void DeepClick(object s,RoutedEventArgs e){if(!busy&&!auto)new DeepDialog(deep){Owner=this}.ShowDialog();}
}
