using System.Windows;
using System.Windows.Controls;
using ReviveOS.WindowsOptimizer;
namespace ReviveOS.UI;
public sealed class SettingsDialog:Window {
 readonly ListBox list=new(){Margin=new Thickness(8),DisplayMemberPath="Description",ItemsSource=Optimizer.Catalog};readonly TextBox details=new(){IsReadOnly=true,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(8)};bool busy;
 public SettingsDialog(Optimizer optimizer){Title="Ajustes documentados";Width=650;Height=480;var panel=new StackPanel();panel.Children.Add(list);panel.Children.Add(details);list.SelectionChanged+=(_,_)=>{if(list.SelectedItem is Tweak t)details.Text=$"ID: {t.Id}\nBenefício esperado: {t.Benefit}\nRisco: {t.Risk}\nBackup no diário antes de escrever; validação por leitura; reversão individual disponível.";};var apply=new Button{Content="Aplicar selecionado",Margin=new Thickness(8)};var restore=new Button{Content="Restaurar selecionado",Margin=new Thickness(8)};panel.Children.Add(apply);panel.Children.Add(restore);Content=panel;Closing+=(_,e)=>e.Cancel=busy;
  apply.Click+=async(_,_)=>{if(busy||list.SelectedItem is not Tweak t)return;if(MessageBox.Show(details.Text,"Aplicar ajuste?",MessageBoxButton.YesNo)!=MessageBoxResult.Yes)return;busy=true;try{details.Text=await optimizer.Apply(t.Id,false);}catch(Exception e){details.Text=e.Message+"; restaure a alteração pendente antes de reaplicar.";}finally{busy=false;}};
  restore.Click+=async(_,_)=>{if(busy||list.SelectedItem is not Tweak t)return;busy=true;try{await optimizer.Restore(t.Id);details.Text="Restaurado e verificado.";}catch(Exception e){details.Text=e.Message;}finally{busy=false;}};
 }
}
