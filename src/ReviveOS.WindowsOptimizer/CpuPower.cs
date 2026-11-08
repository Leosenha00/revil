using System.Globalization;
using System.Runtime.InteropServices;
using ReviveOS.Core;
namespace ReviveOS.WindowsOptimizer;
public record CpuPowerReport(Guid Scheme,uint AcMaximumPercent,string Interpretation);
public sealed class CpuPowerBackend:ISettingsBackend {
 static readonly Guid ProcessorGroup=new("54533251-82be-4824-96c1-47b60b740d00");static readonly Guid MaximumState=new("bc5038f7-23e0-4960-96da-33abaf5935ec");
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
 [DllImport("PowrProf.dll")]static extern uint PowerGetActiveScheme(IntPtr root,out IntPtr scheme);
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
 [DllImport("PowrProf.dll")]static extern uint PowerReadACValueIndex(IntPtr root,ref Guid scheme,ref Guid subgroup,ref Guid setting,out uint value);
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
 [DllImport("PowrProf.dll")]static extern uint PowerWriteACValueIndex(IntPtr root,ref Guid scheme,ref Guid subgroup,ref Guid setting,uint value);
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
 [DllImport("PowrProf.dll")]static extern uint PowerSetActiveScheme(IntPtr root,ref Guid scheme);
 [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
 [DllImport("kernel32.dll")]static extern IntPtr LocalFree(IntPtr memory);
 static void Check(uint error){if(error!=0)throw new System.ComponentModel.Win32Exception((int)error);}
 public static Guid ActiveScheme(){if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException();Check(PowerGetActiveScheme(IntPtr.Zero,out var pointer));if(pointer==IntPtr.Zero)throw new IOException("Plano de energia ausente");try{return Marshal.PtrToStructure<Guid>(pointer);}finally{LocalFree(pointer);}}
 public static Guid Parse(string id){if(!id.StartsWith("cpu-ac-max:")||!Guid.TryParseExact(id[11..],"D",out var scheme))throw new ArgumentException("ID de CPU inválido");return scheme;}
 public Task<string?> Read(string id){if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException();var scheme=Parse(id);var group=ProcessorGroup;var setting=MaximumState;Check(PowerReadACValueIndex(IntPtr.Zero,ref scheme,ref group,ref setting,out var value));if(value>100)throw new IOException("Limite de CPU não suportado; não será alterado");return Task.FromResult<string?>(value.ToString(CultureInfo.InvariantCulture));}
 public Task Write(string id,string? value){if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException();var scheme=Parse(id);if(!uint.TryParse(value,NumberStyles.None,CultureInfo.InvariantCulture,out var max)||max>100)throw new ArgumentException("Estado máximo deve estar entre 0 e 100");var group=ProcessorGroup;var setting=MaximumState;Check(PowerWriteACValueIndex(IntPtr.Zero,ref scheme,ref group,ref setting,max));if(ActiveScheme()==scheme)Check(PowerSetActiveScheme(IntPtr.Zero,ref scheme));return Task.CompletedTask;}
}
public sealed class CpuPowerOptimizer {
 readonly CpuPowerBackend backend=new();readonly ChangeJournal journal=new(Path.Combine(State.Root,"cpu-power-changes.json"),new CpuPowerBackend());
 public bool HasPending=>journal.Entries().Any(x=>x.Status!="restored");
 public async Task<CpuPowerReport> Inspect(){var scheme=CpuPowerBackend.ActiveScheme();var value=uint.Parse((await backend.Read("cpu-ac-max:"+scheme))!,CultureInfo.InvariantCulture);return new(scheme,value,value<100?"Existe teto de estado máximo configurado abaixo de 100% na tomada. Não prova a causa da lentidão.":"O teto de estado máximo já está em 100%; não há limite desse tipo para remover. Isso não mede clock, temperatura ou ganho.");}
 public async Task<string> PermitNominalPerformance(){var info=await Inspect();if(info.AcMaximumPercent==100)return info.Interpretation;var id="cpu-ac-max:"+info.Scheme;if(journal.Entries().Any(x=>x.Id==id&&x.Status!="restored"))throw new InvalidOperationException("Restaure a alteração de CPU pendente antes de reaplicar");try{await journal.Apply(id,"100");}catch(Exception error){try{await journal.Restore(id);}catch(Exception undo){throw new AggregateException(error,undo);}throw;}return "Estado máximo AC definido em 100% e verificado no plano "+info.Scheme+". É permissão para desempenho nominal suportado; não é overclock, Turbo novo ou bypass de hardware. Bateria, mínimo de CPU, tensões e proteções térmicas não foram alterados.";}
 public Task Restore()=>journal.Restore();
}
