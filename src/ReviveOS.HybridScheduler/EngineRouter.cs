namespace ReviveOS.HybridScheduler;
public enum TaskKind { CadWebEditing,FullAutoCadDesktop,PythonInteractive,SymbolicMath,CsvDxfAnalysis,VideoConversion,BlenderRendering }
public record EngineContext(bool DataCanBePublic,bool PrivateUploadApproved,bool AutodeskWebEntitlementConfirmed,bool AuthorizedSshHost,bool AuthorizedWindowsHost,bool PythonInstalled,bool SympyInstalled,bool FFmpegInstalled,bool BlenderInstalled);
public record EngineRoute(string Engine,string Mode,string Reason,string Limitation){public string Title=>Engine switch{"LOCAL_CAD"=>"AutoCAD local + ajustes reversíveis","AUTOCAD_WEB"=>"AutoCAD Web — assinatura existente","RDP"=>"Host Windows próprio por RDP","LOCAL_PYTHON"=>"Python instalado no notebook","LOCAL_SYMPY"=>"SymPy instalado no notebook","LOCAL_FFMPEG"=>"FFmpeg instalado no notebook","LOCAL_BLENDER"=>"Blender instalado no notebook","BINDER"=>"Binder — exploração temporária","SAGECELL"=>"SageMathCell — matemática curta","SSH"=>"Host SSH próprio",_=>Engine};}
/// <summary>Compatibility/privacy routing only. Does not claim measured speed or provider availability.</summary>
public static class EngineRouter {
 public static EngineRoute[] Routes(TaskKind task,EngineContext c) {
  var result=new List<EngineRoute>();var privateRemote=c.DataCanBePublic||c.PrivateUploadApproved;
  if(task==TaskKind.CadWebEditing){result.Add(new("LOCAL_CAD","LOCAL","AutoCAD original permanece disponível pelo ícone habitual","Instalação/licença e fluidez no notebook precisam de validação"));if(c.AutodeskWebEntitlementConfirmed&&privateRemote)result.Add(new("AUTOCAD_WEB","HYBRID","Usuário confirmou acesso pela assinatura e autorizou fluxo remoto","Abre serviço oficial; sem API de licença verificada, upload automático ou equivalência ao desktop completo"));}
  if(task==TaskKind.FullAutoCadDesktop){result.Add(new("LOCAL_CAD","LOCAL","Preserva a instalação original local","Hardware pode limitar desempenho"));if(c.AuthorizedWindowsHost&&privateRemote)result.Add(new("RDP","REMOTE","Usuário informou host Windows autorizado","CPU/RAM/GPU, licenças e conectividade do host ainda devem ser validadas; não provisiona VM"));}
  if(task is TaskKind.PythonInteractive or TaskKind.CsvDxfAnalysis && c.PythonInstalled)result.Add(new("LOCAL_PYTHON","LOCAL","Python encontrado no PATH","Dependências do script precisam estar instaladas; não é sandbox"));
  if(task==TaskKind.SymbolicMath&&c.PythonInstalled&&c.SympyInstalled)result.Add(new("LOCAL_SYMPY","LOCAL","Python e módulo SymPy encontrados","Execução usa CPU/RAM locais"));
  if(task==TaskKind.VideoConversion&&c.FFmpegInstalled)result.Add(new("LOCAL_FFMPEG","LOCAL","FFmpeg encontrado no PATH","Usa recursos locais; sem ganho presumido"));
  if(task==TaskKind.BlenderRendering&&c.BlenderInstalled)result.Add(new("LOCAL_BLENDER","LOCAL","Blender encontrado no PATH","Renderização pode ser lenta sem GPU apropriada"));
  if(c.DataCanBePublic&&task is TaskKind.PythonInteractive or TaskKind.CsvDxfAnalysis)result.Add(new("BINDER","REMOTE","Sessão interativa leve sem cadastro","Temporária, sem armazenamento permanente; não usar para dados confidenciais, jobs pesados ou serviço contínuo"));
  if(c.DataCanBePublic&&task==TaskKind.SymbolicMath)result.Add(new("SAGECELL","REMOTE","Cálculo matemático curto sem cadastro","Sessão efêmera e limitada, sem rede livre nem armazenamento persistente"));
  if(c.AuthorizedSshHost&&privateRemote&&task is TaskKind.PythonInteractive or TaskKind.CsvDxfAnalysis or TaskKind.VideoConversion or TaskKind.BlenderRendering)result.Add(new("SSH","REMOTE","Host autorizado informado pelo usuário","Ferramentas, arquitetura e limites precisam ser verificados; o ReviveOS não fornece esse host"));
  return result.ToArray();
 }
}
