# Administrador e limites de CPU — 0.7

A foto fornecida identifica Pentium Dual-Core T4500 a 2,30 GHz e RAM indicada de 3 GB. Ela não informa chipset/PLL, BIOS, temperaturas, condições de carga ou limites de energia atuais. Não há benchmark do notebook. A Microsoft especifica pelo menos 4 GB para Windows 11; estar instalado não demonstra adequação de desempenho.

## Operações reais implementadas

A interface padrão continua asInvoker. Um componente separado `admin/ReviveOS.Admin.exe`, com manifesto requireAdministrator, solicita UAC para operações administrativas permitidas. Suporta somente inspeção CPU/boot, teto AC, reversão e ajustes profundos já documentados. Não recebe comandos livres, DLLs, scripts ou caminho arbitrário de resultado. O SID deve ser o mesmo do usuário da interface para preservar contexto e backups; elevação com credenciais de outra conta é recusada.

O helper é distribuído com runtime próprio, não exige instalar SDK/.NET à parte e sai após o pedido. Não é serviço residente. A interface não transfere privilégios administrativos a scripts Python, navegador ou jobs por esse mecanismo. Se o próprio usuário iniciar a interface inteira como administrador externamente, seus processos terão o contexto escolhido por ele; esse não é o fluxo padrão.

PowerGetActiveScheme/PowerReadACValueIndex consultam o teto de estado máximo do processador na tomada. Se estiver abaixo de 100%, PowerWriteACValueIndex pode definir 100%, com snapshot e verificação. A alteração é registrada por GUID do plano e a reversão não força ativação de um plano diferente escolhido pelo usuário. Estado mínimo, bateria, tensões, firmware, multiplicador, MSRs e proteções térmicas permanecem fora dessa operação.

Se já estiver em 100%, o aplicativo informa que não existe esse teto para remover e não declara ganho nem grava um ajuste fictício. Um teto abaixo de 100% também não prova que ele seja o gargalo. Permitir 100% pode aumentar calor/consumo; a ação exige escolha e confirmação do usuário. Limites de firmware/temperatura podem continuar atuando.

Inspeção administrativa usa CIM e `bcdedit /enum {current}` apenas para leitura; código de saída do boot é informado no relatório. Não escreve BCD, não modifica bootloader, não altera BIOS e não instala drivers.

## Injeção/bypass solicitado

Não foi implementada injeção de código ou um bypass universal do T4500. Não foi identificado um procedimento compatível e verificável. Código injetado em processos não muda por si só características físicas da CPU. Aumentar o teto do plano de energia para 100% é uma operação do Windows, não desbloqueio de multiplicador, novos núcleos ou Turbo Boost.

Overclock e controles de energia existem em plataformas específicas. Para avaliar a técnica mencionada pelo usuário, é necessário identificar a ferramenta/procedimento e verificar suporte da CPU, placa/chipset/PLL e firmware. Uma permissão ampla de administrador não substitui esse dado técnico nem o teste no hardware real. Nenhuma funcionalidade ausente é exibida como concluída.

## Validação

Testes portáveis verificam allowlist do helper e rejeição de IDs/comandos arbitrários. O teste Windows duplica um plano de energia, modifica apenas a cópia inativa para 50%, aplica 100%, reverte a 50% e remove apenas a cópia de teste. Confirma que o plano ativo original não mudou. Isso valida as APIs e rollback, não desempenho ou suporte térmico do T4500.

O smoke test Windows do helper usa token já elevado quando disponível e verifica a resposta e rejeição de SID divergente. O prompt interativo UAC, cancelamento pelo usuário, elevação real no notebook e ganho de desempenho permanecem pendentes de validação manual. Não há senha solicitada pelo chat.

Fontes:

- https://learn.microsoft.com/en-us/windows/win32/secbp/running-with-administrator-privileges
- https://learn.microsoft.com/en-us/windows-hardware/customize/power-settings/options-for-perf-state-engine-maxperformance
- https://learn.microsoft.com/en-us/windows/win32/api/powersetting/nf-powersetting-powerwriteacvalueindex
- https://support.microsoft.com/en-us/windows/experience/compatibility/windows-11-system-requirements
- https://www.notebookcheck.net/Intel-Pentium-Dual-Core-T4500-Notebook-Processor.25727.0.html

## Esclarecimento sobre HD como RAM, GPU ou núcleos

O usuário esclareceu que projetos anteriores reservavam HD como memória e simulavam núcleos/vídeo. Não foi fornecido um repositório identificável. Paginação é real e já é gerenciada no módulo profundo; usa armazenamento para apoiar memória comprometida e pode evitar falta de memória. A RAM física continua a mesma e, no HD mecânico, acessos paginados podem aumentar travamentos.

WARP e outros renderizadores por software executam gráficos na CPU, não no HD. vCPUs/emulação usam recursos do processador real; apresentar mais processadores a um convidado não cria capacidade física. Esses mecanismos não são anunciados como aceleração garantida do Chrome/AutoCAD nem são ativados globalmente por injeção.

A inspeção administrativa inclui RAM física, paginação, contadores de memória e GPU conforme informado pelo CIM, com ressalvas e erros opcionais por seção. Não falsifica AdapterRAM ou quantidade de núcleos para produzir um desbloqueio aparente.

Referências adicionais: https://learn.microsoft.com/en-us/troubleshoot/windows-client/performance/introduction-to-the-page-file ; https://learn.microsoft.com/en-us/windows/win32/direct3darticles/directx-warp ; https://www.intel.com/content/www/us/en/support/articles/000098959/processors/intel-xeon-processors.html . A referência de vCPU explica o conceito geral, não atribui Hyper-Threading ao T4500.
