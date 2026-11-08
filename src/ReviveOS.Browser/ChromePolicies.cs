using System.Globalization;
using Microsoft.Win32;
using ReviveOS.Core;
namespace ReviveOS.Browser;
public record ChromeTweak(string Id,string Name,string After,string Benefit,string Risk,int MinimumMajorVersion);
public sealed class ChromePolicyBackend:ISettingsBackend {
 public const string KeyPath=@"Software\Policies\Google\Chrome";
 public static readonly ChromeTweak[] Catalog=[
  new("HighEfficiencyModeEnabled","Ativar economia de memória","1","Permite descartar abas inativas para recuperar RAM","Abas inativas podem recarregar e perder estado não salvo pelo site; não garante abertura mais rápida",108),
  new("MemorySaverModeSavings","Economia de memória equilibrada","1","Seleciona heurística equilibrada com Memory Saver ativo","Depende do modo anterior; abas inativas podem recarregar",126),
  new("BackgroundModeEnabled","Desativar apps após fechar Chrome","0","Reduz concorrência quando o navegador está fechado","Apps/extensões deixam de funcionar após fechar; pode perder benefício de sessão aquecida",19),
  new("NetworkPredictionOptions","Desativar pré-carregamento especulativo","2","Pode reduzir tráfego e trabalho concorrente","Pode tornar navegação e carregamento de páginas mais lentos; opcional",38)];
 public static ChromeTweak Get(string id)=>Catalog.SingleOrDefault(x=>x.Id==id)??throw new ArgumentException("Política não permitida");
 public static void ValidateValue(string id,string? value){_ =Get(id);if(value==null)return;var max=id is "MemorySaverModeSavings" or "NetworkPredictionOptions"?2:1;if(!int.TryParse(value,NumberStyles.None,CultureInfo.InvariantCulture,out var n)||n<0||n>max)throw new IOException("Valor de política incompatível");}
 public static void CheckMachinePolicy(string id){if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException();_=Get(id);foreach(var view in new[]{RegistryView.Registry64,RegistryView.Registry32}){using var root=RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,view);using var key=root.OpenSubKey(KeyPath);if(key?.GetValueNames().Contains(id,StringComparer.OrdinalIgnoreCase)==true||key?.GetValueNames().Contains("CloudManagementEnrollmentToken",StringComparer.OrdinalIgnoreCase)==true)throw new IOException("Há política de máquina/gerenciamento existente; ajuste recusado para não sobrepor administração externa");}}
 public Task<string?> Read(string id){if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException();_=Get(id);using var key=Registry.CurrentUser.OpenSubKey(KeyPath);if(key==null||!key.GetValueNames().Contains(id,StringComparer.OrdinalIgnoreCase))return Task.FromResult<string?>(null);if(key.GetValueKind(id)!=RegistryValueKind.DWord)throw new IOException("Tipo existente incompatível; não alterado");var value=Convert.ToString(key.GetValue(id),CultureInfo.InvariantCulture);ValidateValue(id,value);return Task.FromResult(value);}
 public Task Write(string id,string? value){if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException();ValidateValue(id,value);if(value==null){using var existing=Registry.CurrentUser.OpenSubKey(KeyPath,true);existing?.DeleteValue(id,false);}else{using var key=Registry.CurrentUser.CreateSubKey(KeyPath);key.SetValue(id,int.Parse(value,CultureInfo.InvariantCulture),RegistryValueKind.DWord);}return Task.CompletedTask;}
}
public sealed class ChromeOptimizer {
 readonly ChangeJournal journal=new(Path.Combine(State.Root,"chrome-changes.json"),new ChromePolicyBackend());
 public bool HasPending=>journal.Entries().Any(x=>x.Status!="restored");
 public string History()=>System.Text.Json.JsonSerializer.Serialize(journal.Entries(),State.Json);
 public async Task Apply(IEnumerable<string> ids){var selected=ids.Distinct().Select(ChromePolicyBackend.Get).ToArray();var chrome=ChromeRuntime.FindChrome()??throw new IOException("Chrome não encontrado");var version=System.Diagnostics.FileVersionInfo.GetVersionInfo(chrome).FileMajorPart;foreach(var tweak in selected){if(version<tweak.MinimumMajorVersion)throw new IOException(tweak.Name+": versão do Chrome sem suporte; atualize pelo próprio Chrome");ChromePolicyBackend.CheckMachinePolicy(tweak.Id);}foreach(var tweak in selected)await journal.Apply(tweak.Id,tweak.After);}
 public Task Restore(string? id=null)=>journal.Restore(id);
}
