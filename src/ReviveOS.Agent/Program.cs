using System.Diagnostics;
using System.Security.Principal;
using System.Text.Json;
using ReviveOS.Core;
using ReviveOS.Diagnostics;
using ReviveOS.ProcessManager;
using ReviveOS.TurboEngine;
if(!OperatingSystem.IsWindows())return 2;
Directory.CreateDirectory(ContinuousClient.Root);
if(args.SequenceEqual(new[]{"--sample-once"})){using var sampler=new NativeResourceSampler();sampler.Read();await Task.Delay(1200);State.Save(Path.Combine(ContinuousClient.Root,"native-sample.json"),sampler.Read());return 0;}
if(args.SequenceEqual(new[]{"--stop"})){try{await ContinuousClient.Stop();return 0;}catch{return 1;}}
if(args.Length!=0)return 2;
FileStream lease;try{lease=ContinuousClient.Lease();}catch(IOException){return 0;}
using(lease){var backend=new NativePriorityBackend();var journal=new PrioritySession(ContinuousClient.JournalFile,backend);ResourceSample? sample=null;ContinuousDecision? decision=null;var actions=new List<string>();DateTime lastStatus=DateTime.MinValue;string? lastSignature=null;
 void Report(string state,string? error=null){State.Save(Path.Combine(ContinuousClient.Root,"status.json"),new ContinuousStatus(Environment.ProcessId,DateTime.UtcNow,state,sample,decision,actions.ToArray(),error));}
 void Log(string message){var file=Path.Combine(ContinuousClient.Root,"events.log");if(File.Exists(file)&&new FileInfo(file).Length>1048576)File.Move(file,file+".1",true);File.AppendAllText(file,DateTime.UtcNow.ToString("O")+" "+message+Environment.NewLine);}
 try{
  using var identity=WindowsIdentity.GetCurrent();if(new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator))throw new InvalidOperationException("Execute o agente pela interface sem elevação; não precisa de administrador");
  journal.Restore();if(journal.Entries().Any(x=>x.State=="restore-failed"))throw new IOException("Restauração anterior incompleta");
  using var sampler=new NativeResourceSampler();var policy=new ContinuousPolicy();var cpuHistory=new Dictionary<string,(ulong Cpu,DateTime Time)>();var blocked=new HashSet<string>();var owner=backend.Describe(Environment.ProcessId)!.Identity;
  while(ContinuousClient.Config().Enabled&&!File.Exists(Path.Combine(ContinuousClient.Root,"stop"))){var config=ContinuousClient.Config();config.Validate();sample=sampler.Read();decision=policy.Evaluate(sample,config);actions.Clear();journal.ReconcileExternalChanges();foreach(var item in journal.Entries().Where(x=>x.State is "external-change" or "restore-failed"))blocked.Add(item.Identity.Key);
   var wanted=new Dictionary<string,(ProcessDescriptor Process,uint Priority)>();int foreground=NativePriorityBackend.ForegroundPid();
   bool Eligible(ProcessDescriptor p)=>p.Identity.Sid==owner.Sid&&p.Identity.Session==owner.Session&&p.Identity.Pid!=owner.Pid&&!blocked.Contains(p.Identity.Key);
   try{if(decision.BoostForeground&&foreground>0){var p=backend.Describe(foreground);if(p!=null&&Eligible(p)&&ContinuousConfig.ForegroundNames.Contains(Path.GetFileNameWithoutExtension(p.Identity.Path),StringComparer.OrdinalIgnoreCase))wanted[p.Identity.Key]=(p,NativePriorityBackend.AboveNormal);}}catch(System.ComponentModel.Win32Exception){}
   // Enumerate only after explicit background authorization; never terminate processes.
   if(config.ApprovedBackgroundPaths.Length>0){var seen=new HashSet<string>();foreach(var process in Process.GetProcesses()){using(process){try{if(!ContinuousConfig.OptionalNames.Contains(process.ProcessName,StringComparer.OrdinalIgnoreCase))continue;var p=backend.Describe(process.Id);if(p==null||!Eligible(p)||!config.ApprovedBackgroundPaths.Contains(p.Identity.Path,StringComparer.OrdinalIgnoreCase))continue;seen.Add(p.Identity.Key);bool busy=false;if(cpuHistory.TryGetValue(p.Identity.Key,out var previous)&&p.CpuTime>=previous.Cpu){var seconds=(sample.Utc-previous.Time).TotalSeconds;if(seconds>0)busy=(p.CpuTime-previous.Cpu)/10000000d/seconds/Environment.ProcessorCount*100>=2;}cpuHistory[p.Identity.Key]=(p.CpuTime,sample.Utc);if(decision.LowerApprovedBackground&&busy&&p.Identity.Pid!=foreground)wanted[p.Identity.Key]=(p,NativePriorityBackend.BelowNormal);}catch(System.ComponentModel.Win32Exception){}catch(InvalidOperationException){}}}foreach(var key in cpuHistory.Keys.Where(x=>!seen.Contains(x)).ToArray())cpuHistory.Remove(key);}
   var restored=journal.Restore(keepKeys:wanted.Keys.ToHashSet());if(restored.Length>0)actions.Add(restored);if(journal.Entries().Any(x=>x.State=="restore-failed"))throw new IOException("Restauração falhou; agente interrompido para evitar novas alterações");
   foreach(var item in wanted.Values){try{var outcome=journal.Apply(item.Process,item.Priority);if(outcome=="aplicado")actions.Add(item.Process.Identity.Pid+": prioridade "+item.Priority+" aplicada");}catch(Exception ex){blocked.Add(item.Process.Identity.Key);actions.Add(item.Process.Identity.Pid+": preservado: "+ex.Message);}}
   var signature=JsonSerializer.Serialize(new{decision,actions});if(signature!=lastSignature){Log(signature);lastSignature=signature;}
   if(DateTime.UtcNow-lastStatus>TimeSpan.FromSeconds(30)||actions.Count>0){Report("running");lastStatus=DateTime.UtcNow;}
   var secondsToWait=decision.ResourceBudgetLimited?60:config.PollSeconds;for(int second=0;second<secondsToWait;second++){if(File.Exists(Path.Combine(ContinuousClient.Root,"stop")))break;await Task.Delay(1000);}
  }
  actions.Clear();actions.Add(journal.Restore());if(journal.Entries().Any(x=>x.State=="restore-failed"))throw new IOException("Prioridades não restauradas integralmente");Report("stopped");return 0;
 }catch(Exception ex){try{actions.Add(journal.Restore());Report("faulted",ex.ToString());Log(ex.ToString());}catch{}return 1;}
}
