using System.Diagnostics;
namespace ReviveOS.RemoteApps;
public static class RemoteDesktop {
 public static void OpenRdp(string host,bool authorized) {if(!authorized)throw new InvalidOperationException("Confirme autorização do host");if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException();if(Uri.CheckHostName(host)==UriHostNameType.Unknown||host.StartsWith('-'))throw new ArgumentException("Host inválido");var si=new ProcessStartInfo("mstsc.exe"){UseShellExecute=false};si.ArgumentList.Add("/v:"+host);Process.Start(si);}
}
