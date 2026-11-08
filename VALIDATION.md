# Verificação da versão 0.1

Ambiente: Linux x64; SDK 8.0.427. Compilação cruzada WPF Windows x64 com runtime incluído concluída.

Quatro testes xUnit passaram: custo de upload rejeita remoto; falta de autorização impede seleção remota; execução remota estimada mais rápida é selecionada; backup cria versões, restaura conteúdo, impede sobrescrita e detecta corrupção.

Instalador NSIS gerado em Linux. Não executado em Windows. Sem assinatura Authenticode. Não há números de desempenho do notebook: equipamento não conectado. Compilação não comprova execução nem ganho de desempenho.

Pendentes: diagnóstico Windows real, aplicação/reversão dos ajustes, recuperação após interrupção, teste da UI, instalação/desinstalação, testes de Wi-Fi, AutoCAD, provedores e execução remota. Nenhum deles é marcado como aprovado.
