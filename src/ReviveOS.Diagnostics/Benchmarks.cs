using System.Diagnostics;
using ReviveOS.Core;
namespace ReviveOS.Diagnostics;
public record StorageBenchmark(DateTime Utc,string Volume,int Bytes,double WriteMs,double ReadMs,string Conditions);
public static class Benchmarks {
 public static async Task<StorageBenchmark> Storage(CancellationToken ct=default){
  Directory.CreateDirectory(State.Root);var path=Path.Combine(State.Root,Guid.NewGuid()+".benchmark");var data=new byte[4*1024*1024];Random.Shared.NextBytes(data);
  try {var timer=Stopwatch.StartNew();await using(var output=new FileStream(path,FileMode.CreateNew,FileAccess.Write,FileShare.None,65536,FileOptions.Asynchronous|FileOptions.WriteThrough)){await output.WriteAsync(data,ct);await output.FlushAsync(ct);}var write=timer.Elapsed.TotalMilliseconds;timer.Restart();await using(var input=File.OpenRead(path))await input.CopyToAsync(Stream.Null,ct);var read=timer.Elapsed.TotalMilliseconds;
   var result=new StorageBenchmark(DateTime.UtcNow,Path.GetPathRoot(path)!,data.Length,write,read,"Arquivo temporário de 4 MiB; leitura pode estar em cache. Não mede IOPS nem latência física isolada. Compare apenas mesmo volume e carga.");State.Save(Path.Combine(State.Root,"benchmarks",DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff")+".json"),result);return result;
  }finally{if(File.Exists(path))File.Delete(path);}
 }
 public static Task<string> Sample(CancellationToken ct=default)=>Commands.CheckedPowerShell("$samples=1..5 | ForEach-Object {$cpu=Get-CimInstance Win32_PerfFormattedData_PerfOS_Processor -Filter \"Name='_Total'\"; $m=Get-CimInstance Win32_PerfFormattedData_PerfOS_Memory; $d=Get-CimInstance Win32_PerfFormattedData_PerfDisk_PhysicalDisk -Filter \"Name='_Total'\"; [pscustomobject]@{Utc=[DateTime]::UtcNow;CPUPercent=$cpu.PercentProcessorTime;AvailableMB=$m.AvailableMBytes;DiskQueue=$d.CurrentDiskQueueLength;DiskBytesPerSec=$d.DiskBytesPersec}; Start-Sleep -Seconds 1}; $samples | ConvertTo-Json",ct);
}
