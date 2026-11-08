using System.Diagnostics;
namespace ReviveOS.CloudEngine;
public static class AnonymousNotebook {
 public const string BinderUrl="https://mybinder.org/v2/gh/binder-examples/requirements/505a59f0430cb6414a4acbdd88803ce33c7fd953?urlpath=lab";
 public const string Limits="Sessão Linux sem cadastro, login ou cartão. Temporária e limitada; não executa AutoCAD Windows, não fornece GPU e não garante ganho de desempenho. Use para exploração interativa leve. Não envie dados confidenciais ou credenciais. Baixe resultados antes de sair. Sem reconexão automática para contornar expiração.";
 public static string Notebook(){using var resource=typeof(AnonymousNotebook).Assembly.GetManifestResourceStream("anonymous.ipynb")??throw new IOException("Notebook ausente");using var reader=new StreamReader(resource);return reader.ReadToEnd();}
 public static async Task Save(string path){await using var file=new FileStream(path,FileMode.CreateNew,FileAccess.Write,FileShare.None);await using var writer=new StreamWriter(file);await writer.WriteAsync(Notebook());}
 public static void Open(){Process.Start(new ProcessStartInfo(BinderUrl){UseShellExecute=true});}
}
