using ReviveOS.Core;
using ReviveOS.Security;
using ReviveOS.WindowsOptimizer;
using System.Text;
using System.Text.Json;
// No command shell, DLL injection or arbitrary executable requests are accepted here.
string? Argument(string name){var index=Array.IndexOf(args,name);return index>=0&&index+1<args.Length?args[index+1]:null;}
if(!OperatingSystem.IsWindows()||!AdminBroker.IsAdministrator())return 16;
if(Argument("--expected-sid")!=AdminBroker.UserSid())return 17;
if(!Guid.TryParse(Argument("--request"),out var request))return 18;
using var mutex=new Mutex(true,"Local\\ReviveOS-Admin-"+AdminBroker.UserSid(),out var acquired);if(!acquired)return 19;
try {
 var operation=Argument("--operation")??throw new ArgumentException("Operação ausente");var ids=JsonSerializer.Deserialize<string[]>(Encoding.UTF8.GetString(Convert.FromBase64String(Argument("--ids")??"")))??[];AdminBroker.Validate(operation,ids);string message;
 switch(operation){
  case "check":message="Pedido administrativo autorizado pelo UAC para esta operação. A interface permanece com privilégios normais; novas ações podem pedir consentimento novamente.";break;
  case "cpu-inspect":
   var power=await new CpuPowerOptimizer().Inspect();
   var inventory=await Commands.CheckedPowerShell("""
$report=[ordered]@{}
$report.CPU=@(Get-CimInstance Win32_Processor | Select-Object Name,NumberOfCores,NumberOfLogicalProcessors,MaxClockSpeed,CurrentClockSpeed)
$report.TotalPhysicalMemory=(Get-CimInstance Win32_ComputerSystem).TotalPhysicalMemory
$report.Errors=@()
try {$report.PageFile=@(Get-CimInstance Win32_PageFileUsage | Select-Object Name,AllocatedBaseSize,CurrentUsage,PeakUsage)} catch {$report.Errors+=@{Section='PageFile';Error=$_.Exception.Message}}
try {$report.Memory=@(Get-CimInstance Win32_PerfFormattedData_PerfOS_Memory | Select-Object AvailableMBytes,CommittedBytes,CommitLimit,PagesPersec)} catch {$report.Errors+=@{Section='Memory';Error=$_.Exception.Message}}
try {$report.GPU=@(Get-CimInstance Win32_VideoController | Select-Object Name,DriverVersion,AdapterRAM)} catch {$report.Errors+=@{Section='GPU';Error=$_.Exception.Message}}
$report.BootReadOnly=bcdedit /enum '{current}' 2>&1 | Out-String
$report.BootExitCode=$LASTEXITCODE
$report.MemoryCaution='Pagina de disco amplia compromisso de memoria, nao RAM fisica. Nao cria nucleos ou GPU. AdapterRAM CIM pode ser impreciso; nao e HD transformado em VRAM.'
$report.ClockCaution='Clock CIM pode ser estatico; nao confirma throttling termico.'
$report | ConvertTo-Json -Depth 6
""");
   message=JsonSerializer.Serialize(power,State.Json)+"\n"+inventory;break;
  case "cpu-max":message=await new CpuPowerOptimizer().PermitNominalPerformance();break;
  case "cpu-restore":await new CpuPowerOptimizer().Restore();message="Estado máximo AC anterior restaurado e verificado nos planos registrados.";break;
  case "deep-apply":
   var deep=new DeepOptimizer();try{await deep.Apply(ids);}catch(Exception error){try{await deep.Restore();}catch(Exception undo){throw new AggregateException(error,undo);}throw;}message="Ajustes selecionados aplicados e verificados no contexto administrativo do mesmo usuário. Paginação requer reboot.";break;
  case "deep-restore":var restore=new DeepOptimizer();if(ids.Length==0)await restore.Restore();else {var errors=new List<Exception>();foreach(var id in ids){try{await restore.Restore(id);}catch(Exception error){errors.Add(error);}}if(errors.Count>0)throw new AggregateException(errors);}message="Ajustes profundos selecionados restaurados; paginação pode requerer reinício.";break;
  default:throw new ArgumentException("Operação não permitida");
 }
 State.Save(AdminBroker.ResultPath(request),new AdminResult(true,message));return 0;
}catch(Exception e){State.Save(AdminBroker.ResultPath(request),new AdminResult(false,e.ToString()));return 1;}
