namespace ReviveOS.HybridScheduler;
public record Workload(double LocalSeconds,double RemoteSeconds,long UploadBytes,long DownloadBytes,double UploadMbps,double DownloadMbps,double LatencyMs,bool Compatible,bool Authorized,bool FreeQuotaAvailable);
public record Decision(string Mode,string Reason,double EstimatedSeconds);
public static class Scheduler {
 public static Decision Choose(Workload w) {
  if(w.LocalSeconds<0||w.RemoteSeconds<0||w.UploadBytes<0||w.DownloadBytes<0||w.LatencyMs<0||!double.IsFinite(w.LocalSeconds)||!double.IsFinite(w.RemoteSeconds))throw new ArgumentException("Estimativas inválidas");
  if(!w.Compatible||!w.Authorized||!w.FreeQuotaAvailable||w.UploadMbps<=0||w.DownloadMbps<=0||!double.IsFinite(w.UploadMbps)||!double.IsFinite(w.DownloadMbps)||!double.IsFinite(w.LatencyMs))return new("LOCAL","Infraestrutura, autorização, cota ou rede indisponível",w.LocalSeconds);
  var total=w.RemoteSeconds+w.UploadBytes*8d/(w.UploadMbps*1e6)+w.DownloadBytes*8d/(w.DownloadMbps*1e6)+w.LatencyMs/1000*2;
  return total<w.LocalSeconds?new("REMOTE","Estimativa inclui transferências; confirmar upload",total):new("LOCAL","Execução local estimada mais rápida",w.LocalSeconds);
 }
}
