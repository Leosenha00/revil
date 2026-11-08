using System.Diagnostics;
namespace ReviveOS.AutoCAD;
public static class AutoCad {public static bool IsRunning()=>Process.GetProcessesByName("acad").Any();public const string Guidance="Preserve a instalação original. Teste GRAPHICSCONFIG com o mesmo desenho e compare aceleração habilitada/desabilitada; não há ganho garantido. Execução remota requer host Windows, GPU e licença compatíveis. Nenhum host fornecido.";}
