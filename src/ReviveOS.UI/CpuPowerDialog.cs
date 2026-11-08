using System.Windows;
using System.Windows.Controls;
using ReviveOS.Security;
using ReviveOS.WindowsOptimizer;
namespace ReviveOS.UI;
public sealed class CpuPowerDialog:Window {
 readonly TextBox report=new(){IsReadOnly=true,TextWrapping=TextWrapping.Wrap,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,Margin=new Thickness(8)};bool busy;
 public CpuPowerDialog(){Title="CPU — limites suportados e administrador";Width=800;Height=650;var dock=new DockPanel{Margin=new Thickness(12)};var top=new StackPanel();top.Children.Add(new TextBlock{Text="O Windows pede administrador via UAC para operações administrativas. Este módulo consulta e ajusta somente o teto AC do plano de energia, até 100% do desempenho suportado. Não aumenta a frequência física, não desbloqueia núcleos e não altera BIOS/MSR/drivers ou proteções térmicas. Consulte o relatório do hardware real. Frequência informada pelo CIM pode ser estática e não comprova throttling.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(8)});var actions=new WrapPanel();top.Children.Add(actions);DockPanel.SetDock(top,Dock.Top);dock.Children.Add(top);dock.Children.Add(report);Content=dock;
  void Button(string text,Func<Task<string>> action){var b=new System.Windows.Controls.Button{Content=text,Padding=new Thickness(8),Margin=new Thickness(4)};b.Click+=async(_,_)=>{if(busy)return;busy=true;report.Text="Executando…";try{report.Text=await action();}catch(Exception ex){report.Text=ex.Message;}finally{busy=false;}};actions.Children.Add(b);}
  Button("Ver limite AC sem elevar",async()=>System.Text.Json.JsonSerializer.Serialize(await new CpuPowerOptimizer().Inspect(),ReviveOS.Core.State.Json));
  Button("Inspeção CPU/boot (UAC, somente leitura)",()=>AdminBroker.Run("cpu-inspect"));
  Button("Permitir desempenho nominal na tomada (UAC)",async()=>{if(MessageBox.Show("Se houver teto AC abaixo de 100%, permitir 100% pode aumentar calor/consumo. Isso não é overclock e não contorna limites térmicos ou de firmware. Aplicar com snapshot e reversão?","CPU",MessageBoxButton.YesNo)!=MessageBoxResult.Yes)return "Nenhuma alteração aplicada.";return await AdminBroker.Run("cpu-max");});
  Button("Restaurar limite AC (UAC)",()=>AdminBroker.Run("cpu-restore"));Button("Testar solicitação de administrador",()=>AdminBroker.Run("check"));Closing+=(_,e)=>e.Cancel=busy;
 }
}
