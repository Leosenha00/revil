# Cobertura real dos requisitos — 0.7

Nenhum conjunto de sites significa que todos os requisitos foram atendidos. Esta matriz mantém a diferença entre código implementado e validação no equipamento. Os testes citados são de runner Windows ou de serviços com exemplos, não benchmarks do notebook.

| Fase original | Implementação | Falta para concluir a aceitação |
|---|---|---|
| 1 Diagnóstico | CIM, hardware, processos, rede e armazenamento; runner testado | Coletar e interpretar o notebook real; temperatura quando disponível |
| 2 App/instalador | WPF e instalador com .NET incluído; instalação/abertura/desinstalação testadas no runner | Teste no Windows 11 do notebook |
| 3 Wi-Fi Auto Start | WLAN oficial, tarefa opt-in no login, tentativas limitadas e rollback | Identificar bloqueio Fn/OEM; reboot, partida a frio e suspensão reais |
| 4 Windows optimizer | UAC por helper, teto AC nominal com reversão, inicialização de lista opcional, SysMain/WSearch selecionados, paginação, visual/energia, snapshots e reversão | Efeitos, regressões e ajustes administrativos completos no notebook; não há otimização total de todo serviço/driver |
| 5 Processos | Lista, RAM, classificação heurística e fechamento cooperativo autorizado | CPU/I/O contínuos por processo, suspensão compatível e classificação ampliada |
| 6 Turbo | Manual e monitor por aplicativo; ajustes reversíveis | Monitor térmico confiável e ganhos reais; nem todo perfil tem ajustes próprios completos |
| 7 GitHub/Codespaces | Actions compila/testa/publica; devcontainer incluído | Uso do Codespace na conta e verificação de cota; não é desktop CAD |
| 8 Nuvem | Binder/SageMathCell testados com exemplos; SSH em loopback; Web Autodesk condicional | Host potente real, acesso à assinatura e testes de tarefas do usuário |
| 9 Scheduler | Estimativa de transferências em biblioteca; central filtra compatibilidade/privacidade | Coletar tempos/velocidades reais e integrar escolha automática à fila |
| 10 AutoCAD Boost | Detecção/perfil local; abertura oficial Web com licença condicionada | Desenho, versão, licença, fluidez e configurações do AutoCAD reais |
| 11 Execução remota | SSH compatível com resultado/checksum; abertura do cliente RDP | Infraestrutura Windows/GPU autorizada e streaming testado; nenhum host foi fornecido |
| 12 Apps universais | Perfis de apps conhecidos; Python/FFmpeg/Blender; Chrome com políticas reversíveis e perfil separado | Expandir ajustes específicos e validar os aplicativos instalados |
| 13 Backup/recuperação | Versões locais, checksum, monitor de um arquivo e recuperação de resultado confirmado | Sincronização com conflitos, retomada por blocos e checkpoints específicos de tarefas |
| 14 Benchmarks | Amostras CPU/RAM/disco, arquivo temporário e tempo observado até janela Chrome visível | Boot, abertura do AutoCAD, latência de streaming, comparação equivalente e regressões no notebook |
| 15 Distribuição | Instaladores reais, testes automatizados e releases | Assinatura Authenticode e homologação completa do hardware |

A central não instala ferramentas pagas, não transmite senhas e não envia arquivos automaticamente. Hardware/firmware, licença e infraestrutura ausentes não são simulados como resolvidos. O histórico e os relatórios locais são a base para continuar o mesmo projeto.
