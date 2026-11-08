using System.Windows;
using System.Windows.Controls;
using ReviveOS.CloudEngine;
namespace ReviveOS.UI;
public sealed class JobRecoveryDialog:Window {
 readonly ListBox list=new(){Margin=new Thickness(8),DisplayMemberPath="Id"};public string? SelectedId=>(list.SelectedItem as JobRecord)?.Id;
 public JobRecoveryDialog(IEnumerable<JobRecord> jobs){Title="Recuperar arquivo remoto confirmado";Width=520;Height=350;var panel=new DockPanel();var button=new Button{Content="Baixar resultado e verificar SHA-256",Margin=new Thickness(8),Padding=new Thickness(8)};DockPanel.SetDock(button,Dock.Bottom);panel.Children.Add(button);panel.Children.Add(list);list.ItemsSource=jobs.ToArray();Content=panel;button.Click+=(_,_)=>{if(SelectedId!=null)DialogResult=true;};}
}
