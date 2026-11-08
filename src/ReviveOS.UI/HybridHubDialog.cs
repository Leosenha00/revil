using System.Windows;
using System.Windows.Controls;
using ReviveOS.CloudEngine;
using ReviveOS.HybridScheduler;
namespace ReviveOS.UI;
public sealed class HybridHubDialog:Window {
 record TaskChoice(TaskKind Kind,string Title);
 static string Label(TaskKind task)=>task switch{TaskKind.CadWebEditing=>"CAD: edição básica no local/Web",TaskKind.FullAutoCadDesktop=>"AutoCAD completo para Windows",TaskKind.PythonInteractive=>"Python: exploração interativa",TaskKind.SymbolicMath=>"Matemática simbólica e cálculos",TaskKind.CsvDxfAnalysis=>"Análise de CSV/DXF",TaskKind.VideoConversion=>"Conversão de vídeo",TaskKind.BlenderRendering=>"Renderização Blender",_=>task.ToString()};
 readonly ComboBox task=new(){ItemsSource=Enum.GetValues<TaskKind>().Select(x=>new TaskChoice(x,Label(x))).ToArray(),DisplayMemberPath="Title",SelectedValuePath="Kind",SelectedIndex=0,Margin=new Thickness(6)};
 readonly CheckBox publicData=new(){Content="Dados podem ser públicos (Binder/SageCell)",Margin=new Thickness(6)};
 readonly CheckBox privateUpload=new(){Content="Autorizo fluxo remoto privado no meu host/Autodesk; envios ainda serão confirmados",Margin=new Thickness(6)};
 readonly CheckBox license=new(){Content="Confirmei que minha assinatura inclui AutoCAD Web",Margin=new Thickness(6)};
 readonly CheckBox ssh=new(){Content="Tenho host SSH autorizado com ferramentas compatíveis",Margin=new Thickness(6)};
 readonly CheckBox windows=new(){Content="Tenho host Windows autorizado para RDP",Margin=new Thickness(6)};
 readonly ListBox routes=new(){Margin=new Thickness(6),DisplayMemberPath="Title",MinHeight=90};
 readonly TextBox details=new(){Margin=new Thickness(6),IsReadOnly=true,TextWrapping=TextWrapping.Wrap,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,Height=130};
 EngineContext? evaluated;TaskKind evaluatedTask;bool busy;
 public HybridHubDialog(Func<TaskKind,Task> localJob,Func<TaskKind,Task> sshJob,Action openRdp,Action openDeep,Action cancel){Title="Central híbrida — motores complementares";Width=850;Height=800;
  var panel=new StackPanel{Margin=new Thickness(14)};var scroll=new ScrollViewer{Content=panel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};Content=scroll;
  panel.Children.Add(new TextBlock{Text="Escolha a tarefa. A central filtra motores por compatibilidade e privacidade; não soma RAM de serviços nem presume ganho de velocidade. Nenhum arquivo é enviado automaticamente.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(6)});
  foreach(var element in new UIElement[]{task,publicData,privateUpload,license,ssh,windows})panel.Children.Add(element);
  var actions=new WrapPanel();panel.Children.Add(actions);
  Button Add(string label,RoutedEventHandler handler){var b=new Button{Content=label,Margin=new Thickness(4),Padding=new Thickness(8)};b.Click+=handler;actions.Children.Add(b);return b;}
  Add("Verificar minha assinatura Autodesk",(_,_)=>OpenService("AUTODESK_PRODUCTS"));
  Add("Avaliar combinação",async(_,_)=>{if(busy)return;busy=true;try{var tools=await ToolProbe.Inspect();evaluated=new(publicData.IsChecked==true,privateUpload.IsChecked==true,license.IsChecked==true,ssh.IsChecked==true,windows.IsChecked==true,tools.Python!=null,tools.Sympy,tools.FFmpeg!=null,tools.Blender!=null);evaluatedTask=(TaskKind)task.SelectedValue!;routes.ItemsSource=EngineRouter.Routes(evaluatedTask,evaluated);details.Text=$"Ferramentas encontradas no PATH: Python={(tools.Python!=null?"sim":"não")}, SymPy={(tools.Sympy?"sim":"não")}, FFmpeg={(tools.FFmpeg!=null?"sim":"não")}, Blender={(tools.Blender!=null?"sim":"não")}."+"\nFerramentas ausentes não serão instaladas silenciosamente. Se não houver motor compatível, a tarefa fica indisponível.";}catch(Exception ex){details.Text=ex.Message;}finally{busy=false;}});
  Add("Executar/abrir selecionado",async(_,_)=>{if(busy||evaluated==null||routes.SelectedItem is not EngineRoute route)return;
   var current=new EngineContext(publicData.IsChecked==true,privateUpload.IsChecked==true,license.IsChecked==true,ssh.IsChecked==true,windows.IsChecked==true,evaluated.PythonInstalled,evaluated.SympyInstalled,evaluated.FFmpegInstalled,evaluated.BlenderInstalled);
   if((TaskKind)task.SelectedValue!!=evaluatedTask||!EngineRouter.Routes(evaluatedTask,current).Any(x=>x.Engine==route.Engine)){details.Text="As condições mudaram. Avalie novamente antes de executar.";return;}
   busy=true;try{switch(route.Engine){case "BINDER":new AnonymousCloudDialog{Owner=this}.ShowDialog();break;case "SAGECELL":case "AUTOCAD_WEB":OpenService(route.Engine);break;case "RDP":openRdp();break;case "SSH":await sshJob(evaluatedTask);details.Text="Operação encerrada; consulte o histórico na janela principal.";break;case "LOCAL_CAD":openDeep();details.Text="Use o ícone habitual do AutoCAD; o monitor por aplicativo pode aplicar o perfil já existente. Nenhum executável original foi substituído.";break;default:await localJob(evaluatedTask);details.Text="Operação encerrada; consulte o histórico na janela principal. A central não verifica a semântica do script escolhido.";break;}}catch(Exception ex){details.Text=ex.Message;}finally{busy=false;}});
  Add("Cancelar tarefa",(_,_)=>cancel());
  Add("Copiar exemplo Sage",(_,_)=>{Clipboard.SetText(ComplementaryServices.SageExample);details.Text="Exemplo copiado. Abra SageMathCell, cole e clique Evaluate. Não contém dados pessoais.";});
  Add("Proteger resultado baixado",async(_,_)=>{if(busy)return;var picker=new Microsoft.Win32.OpenFileDialog{Title="Selecione um resultado já baixado no HD"};if(picker.ShowDialog()!=true)return;busy=true;try{var entry=await new ReviveOS.Backup.VersionStore(System.IO.Path.Combine(ReviveOS.Core.State.Root,"backups")).Backup(picker.FileName);details.Text=System.Text.Json.JsonSerializer.Serialize(entry,ReviveOS.Core.State.Json);}catch(Exception ex){details.Text=ex.Message;}finally{busy=false;}});
  panel.Children.Add(routes);panel.Children.Add(details);routes.SelectionChanged+=(_,_)=>{if(routes.SelectedItem is EngineRoute r)details.Text=r.Mode+": "+r.Reason+"\nLimite: "+r.Limitation;};
  panel.Children.Add(new TextBlock{Text="Os serviços têm papéis diferentes. A lista contém opções compatíveis com a tarefa e com suas autorizações, sem estimativa de aceleração. Para processamento local/SSH, selecione um script ou arquivo compatível; o histórico mostra o resultado real.\n\nO AutoCAD Web exige login Autodesk e acesso incluído na assinatura. Não verifica sua licença por API e não compra planos. SageMathCell e Binder são temporários. Um host SSH/RDP não é fornecido pelo ReviveOS.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(6)});Closing+=(_,e)=>e.Cancel=busy;
 }
 void OpenService(string engine){try{ComplementaryServices.Open(engine);}catch(Exception e){details.Text=e.Message;}}
}
