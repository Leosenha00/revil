using System.Windows;
using System.Windows.Controls;
using ReviveOS.CloudEngine;
namespace ReviveOS.UI;
public sealed class AnonymousCloudDialog:Window {
 public AnonymousCloudDialog(){Title="Nuvem sem login — Binder interativo";Width=670;Height=430;var panel=new StackPanel{Margin=new Thickness(20)};panel.Children.Add(new TextBlock{Text=AnonymousNotebook.Limits,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,16)});panel.Children.Add(new TextBlock{Text="1. Salve o notebook de cálculos e análise CSV/DXF.\n2. Abra o Binder e aguarde o JupyterLab.\n3. Use Upload para abrir o notebook salvo; execute as células.\n4. Baixe o resultado e checksum pelos links da última célula.\n\nNenhum arquivo seu é enviado automaticamente. O notebook contém código Python legível para uso pessoal interativo, sem servidor residente.",TextWrapping=TextWrapping.Wrap});var save=new Button{Content="Salvar notebook interativo",Margin=new Thickness(0,16,0,8),Padding=new Thickness(8)};var launch=new Button{Content="Abrir sessão sem login",Padding=new Thickness(8)};panel.Children.Add(save);panel.Children.Add(launch);Content=panel;
  save.Click+=async(_,_)=>{var dialog=new Microsoft.Win32.SaveFileDialog{FileName="ReviveOS-Interativo.ipynb",Filter="Jupyter notebook|*.ipynb",OverwritePrompt=true};if(dialog.ShowDialog()!=true)return;try{await AnonymousNotebook.Save(dialog.FileName);MessageBox.Show("Notebook salvo. Na sessão Binder, envie-o pelo botão Upload. Se o destino já existir, escolha um novo nome.");}catch(Exception e){MessageBox.Show(e.Message);}};
  launch.Click+=(_,_)=>{try{AnonymousNotebook.Open();}catch(Exception e){MessageBox.Show(e.Message);}};
 }
}
