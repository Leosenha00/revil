using System.Diagnostics;
using ReviveOS.Core;
namespace ReviveOS.CloudEngine;
public static class ComplementaryServices {
 public const string RemoteBrowserUrl="https://browser.lol/en/create";
 public const string SageCellUrl="https://sagecell.sagemath.org/";
 public const string AutoCadWebUrl="https://web.autocad.com/";
 public const string AutodeskProductsUrl="https://manage.autodesk.com/products";
 public const string SageExample="var('x')\nprint(solve(x^2 == 1, x))\nprint(factor(84))\n";
 public static void Open(string engine){var url=engine switch{"REMOTE_BROWSER"=>RemoteBrowserUrl,"SAGECELL"=>SageCellUrl,"AUTOCAD_WEB"=>AutoCadWebUrl,"AUTODESK_PRODUCTS"=>AutodeskProductsUrl,"BINDER"=>AnonymousNotebook.BinderUrl,_=>throw new ArgumentException("Serviço não permitido")};Process.Start(new ProcessStartInfo(url){UseShellExecute=true});}
}
public record LocalTools(string? Python,string? FFmpeg,string? Blender,bool Sympy);
public static class ToolProbe {
 static string? Find(params string[] names){foreach(var folder in (Environment.GetEnvironmentVariable("PATH")??"").Split(Path.PathSeparator)){if(string.IsNullOrWhiteSpace(folder))continue;foreach(var name in names){try{var path=Path.Combine(folder.Trim('"'),name);if(File.Exists(path))return Path.GetFullPath(path);}catch(ArgumentException){}catch(NotSupportedException){}}}return null;}
 public static string? PythonPath()=>OperatingSystem.IsWindows()?Find("python.exe","python3.exe"):Find("python3","python");
 public static async Task<LocalTools> Inspect(){var python=PythonPath();bool sympy=false;if(python!=null){using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(8));try{var r=await Commands.Run(python,["-c","import importlib.util; print('yes' if importlib.util.find_spec('sympy') else 'no')"],timeout.Token);sympy=r.ExitCode==0&&r.Output.Trim()=="yes";if(r.ExitCode!=0||r.Output.Trim() is not ("yes" or "no"))python=null;}catch(Exception e) when(e is IOException or OperationCanceledException or System.ComponentModel.Win32Exception){python=null;}}
  return new(python,OperatingSystem.IsWindows()?Find("ffmpeg.exe"):Find("ffmpeg"),OperatingSystem.IsWindows()?Find("blender.exe"):Find("blender"),sympy);
 }
}
