using System.IO;
using System.Windows;
using System.Windows.Controls;
using ReviveOS.ProcessManager;
using ReviveOS.TurboEngine;
namespace ReviveOS.UI;
public sealed class ContinuousDialog:Window {
 readonly CheckBox startup=new(){Content="Iniciar automaticamente no meu login",IsChecked=true,Margin=new Thickness(4)};
 readonly CheckBox foreground=new(){Content="Favorecer aplicativo em uso sob disputa sustentada de CPU (na tomada)",IsChecked=true,Margin=new Thickness(4)};
 readonly List<(CheckBox Box,string Path)> approved=[];
 public ContinuousConfig Config=>new(){Enabled=true,StartAtLogin=startup.IsChecked==true,FavorForeground=foreground.IsChecked==true,ApprovedBackgroundPaths=approved.Where(x=>x.Box.IsChecked==true).Select(x=>x.Path).ToArray()};
 public ContinuousDialog(){Title="Otimização contínua";Width=680;Height=560;var panel=new StackPanel{Margin=new Thickness(16)};Content=new ScrollViewer{Content=panel};panel.Children.Add(new TextBlock{Text="Amostragem a cada 15 s de CPU, RAM e fila do disco. Após três amostras de pressão, ajusta prioridades temporariamente e restaura ao aliviar a carga, trocar de aplicativo ou desativar. Continua com a janela fechada.\nNão fecha programas, não limpa RAM, não altera limites térmicos. Temperatura/GPU não estão disponíveis neste agente. Não garante ganho em tarefas limitadas pelo HD ou RAM.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(4,4,4,12)});panel.Children.Add(startup);panel.Children.Add(foreground);panel.Children.Add(new TextBlock{Text="Opcional: autorizar prioridade reduzida nestes aplicativos de fundo quando consumirem CPU. Nenhum vem pré-autorizado.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(4,12,4,4)});
  var paths=new HashSet<string>(StringComparer.OrdinalIgnoreCase);var backend=new NativePriorityBackend();foreach(var process in System.Diagnostics.Process.GetProcesses()){using(process){try{if(!ContinuousConfig.OptionalNames.Contains(process.ProcessName,StringComparer.OrdinalIgnoreCase))continue;var p=backend.Describe(process.Id);if(p==null||!paths.Add(p.Identity.Path)||paths.Count>8)continue;var box=new CheckBox{Content=p.Identity.Path,Margin=new Thickness(4),IsChecked=false};approved.Add((box,p.Identity.Path));panel.Children.Add(box);}catch(System.ComponentModel.Win32Exception){}catch(InvalidOperationException){}}}
  var start=new Button{Content="Autorizar e iniciar agente",Padding=new Thickness(12),Margin=new Thickness(4,16,4,4)};start.Click+=(_,_)=>{DialogResult=true;};panel.Children.Add(start);
 }
}
