using System.Windows;
using System.Windows.Controls;
using ReviveOS.ProcessManager;
namespace ReviveOS.UI;
public sealed class ProcessDialog:Window {
 readonly ListBox list=new(){Margin=new Thickness(8),DisplayMemberPath="Name"};
 public ProcessDialog(){Title="Processos — fechamento cooperativo";Width=600;Height=500;var panel=new DockPanel();var close=new Button{Content="Solicitar fechamento do selecionado",Padding=new Thickness(10),Margin=new Thickness(8)};DockPanel.SetDock(close,Dock.Bottom);panel.Children.Add(close);panel.Children.Add(list);Content=panel;list.ItemsSource=Processes.Snapshot();close.Click+=(_,_)=>{if(list.SelectedItem is not ProcessInfo p)return;if(MessageBox.Show($"Solicitar fechamento de {p.Name} (PID {p.Pid})? Salve seus trabalhos primeiro. O aplicativo poderá pedir confirmação. Nunca será forçado o encerramento.","Confirmar fechamento",MessageBoxButton.YesNo)!=MessageBoxResult.Yes)return;try{MessageBox.Show(Processes.RequestClose(p.Pid,p.Name)?"Pedido enviado. O aplicativo pode continuar aguardando salvamento.":"Aplicativo sem janela compatível; nenhum encerramento forçado.");}catch(Exception e){MessageBox.Show(e.Message);}list.ItemsSource=Processes.Snapshot();};}
}
