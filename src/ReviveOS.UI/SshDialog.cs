using System.Windows;
using System.Windows.Controls;
namespace ReviveOS.UI;
public sealed class SshDialog:Window {
 readonly TextBox alias=new(){Margin=new Thickness(8)};public string Alias=>alias.Text.Trim();
 public SshDialog(){Title="Host SSH autorizado";Width=460;Height=230;WindowStartupLocation=WindowStartupLocation.CenterOwner;var panel=new StackPanel{Margin=new Thickness(12)};panel.Children.Add(new TextBlock{Text="Alias do seu arquivo ~/.ssh/config. Autenticação por chave e host já verificado são obrigatórios. Configure pelo OpenSSH oficial. Não digite senhas aqui.",TextWrapping=TextWrapping.Wrap});panel.Children.Add(alias);var b=new Button{Content="Usar host",Margin=new Thickness(8),Padding=new Thickness(8)};b.Click+=(_,_)=>{if(System.Text.RegularExpressions.Regex.IsMatch(Alias,@"\A[a-zA-Z0-9][a-zA-Z0-9._-]{0,63}\z"))DialogResult=true;};panel.Children.Add(b);Content=panel;}
}
