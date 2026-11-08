using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ReviveOS.Core;
namespace ReviveOS.WindowsOptimizer;
public record DeepAction(string Id,string Description,string Reason,string Risk,bool RequiresAdmin,bool RebootRequired);
public record DeepInventory(bool? Hdd,double[] QueueSamples,string[] Startup,bool AutomaticPagefile,JsonElement Services);
public static class DeepPlan {
 public static readonly string[] OptionalStartup=["OneDrive","Spotify","Discord","Steam","EpicGamesLauncher"];
 public static DeepAction Describe(string id)=>id switch {
  "service:SysMain"=>new(id,"Pausar SysMain e sua inicialização","Pode reduzir I/O concorrente em HD sob pressão; pode piorar abertura de apps","Pode piorar cache, inicialização e responsividade; compare antes/depois",true,false),
  "service:WSearch"=>new(id,"Pausar indexação Windows Search","Pode reduzir escrita/leitura de indexação concorrente","Busca do Windows e Outlook podem ficar lentas ou incompletas até reativação; não é selecionado automaticamente",true,false),
  "pagefile"=>new(id,"Paginação gerenciada pelo Windows","Evita limite manual inadequado de memória comprometida","Pode aumentar espaço e I/O de disco; requer reinicialização e espaço livre; não transforma HD em RAM",true,true),
  _ when id.StartsWith("startup:")&&OptionalStartup.Contains(id[8..])=>new(id,"Remover da inicialização: "+id[8..],"Reduz apps concorrentes no login sem desinstalar","Aplicativo deixa de iniciar sozinho; OneDrive deixa de sincronizar até abertura manual",false,false),
  _=>throw new ArgumentException("Alteração não permitida")};
 public static bool HasService(DeepInventory inventory,string name)=>inventory.Services.ValueKind==JsonValueKind.Array&&inventory.Services.EnumerateArray().Any(x=>x.GetProperty("Name").GetString()==name);
 public static DeepAction[] Recommend(DeepInventory inventory){var plan=inventory.Startup.Where(OptionalStartup.Contains).Select(x=>Describe("startup:"+x)).ToList();if(!inventory.AutomaticPagefile)plan.Add(Describe("pagefile"));if(HasService(inventory,"SysMain")&&inventory.Hdd==true&&inventory.QueueSamples.Length>=3&&inventory.QueueSamples.Average()>=1)plan.Add(Describe("service:SysMain"));return plan.ToArray();}
}
public sealed class DeepBackend:ISettingsBackend {
 static string Script {get{using var stream=typeof(DeepBackend).Assembly.GetManifestResourceStream("deep.ps1")??throw new IOException("Recurso deep.ps1 ausente");using var reader=new StreamReader(stream);return reader.ReadToEnd();}}
 public static Task<string> Execute(string operation,string id="",string? value=null){if(operation!="Inventory")_ = DeepPlan.Describe(id);var encoded=Convert.ToBase64String(Encoding.UTF8.GetBytes(value??"null"));return Commands.CheckedPowerShell("& {\n"+Script+"\n} -Operation '"+operation+"' -Id '"+id+"' -ValueBase64 '"+encoded+"'");}
 public async Task<string?> Read(string id)=>(await Execute("Read",id)).Trim();
 public async Task Write(string id,string? value){if(value==null)throw new IOException("Backup profundo ausente; alteração recusada");await Execute("Write",id,value);}
 public static string DisabledAfter(string id,string before){_ =DeepPlan.Describe(id);if(id.StartsWith("startup:"))return "{\"Exists\":false,\"Kind\":null,\"Value\":null}";if(id=="pagefile")return "{\"Automatic\":true,\"Settings\":[]}";var node=JsonNode.Parse(before)!.AsObject();node["Start"]=4;node["Running"]=false;return node.ToJsonString(new JsonSerializerOptions{WriteIndented=false});}
}
public sealed class DeepOptimizer {
 readonly DeepBackend backend=new();readonly ChangeJournal journal=new(Path.Combine(State.Root,"deep-changes.json"),new DeepBackend());
 public bool HasPending=>journal.Entries().Any(x=>x.Status!="restored");
 public string History()=>JsonSerializer.Serialize(journal.Entries(),State.Json);
 public async Task<DeepInventory> Inventory()=>JsonSerializer.Deserialize<DeepInventory>(await DeepBackend.Execute("Inventory"),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})??throw new IOException("Inventário inválido");
 public async Task Apply(IEnumerable<string> ids){var selected=ids.ToArray();foreach(var id in selected)_=DeepPlan.Describe(id);if(selected.Any(id=>DeepPlan.Describe(id).RequiresAdmin))await Commands.CheckedPowerShell("$p=New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent()); if(!$p.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Abra o ReviveOS como administrador para serviços e paginação; nenhuma alteração foi iniciada'}");foreach(var id in selected){_ =DeepPlan.Describe(id);var before=await backend.Read(id)??throw new IOException("Snapshot ausente");await journal.Apply(id,DeepBackend.DisabledAfter(id,before));}}
 public Task Restore(string? id=null)=>journal.Restore(id);
}
