# ReviveOS Hybrid Extreme — 0.1.0 inicial

Aplicativo WPF para Windows 11 x64, publicado com runtime .NET 8 incluído. Projeto em desenvolvimento: **não atende ainda a todos os critérios do prompt master**.

## No Windows

Execute `ReviveOS-Hybrid-Extreme-Setup.exe`, depois abra pelo menu Iniciar. Comece com **Diagnosticar** e **Simular ajustes**. O diagnóstico não modifica configurações e grava JSON em `%LOCALAPPDATA%\ReviveOS\reports`. Relatórios podem conter nomes de processos, comandos de inicialização, endereços MAC e redes; revise antes de compartilhar.

O botão de aceleração aplica somente dois ajustes documentados (animações e plano de energia) após confirmação. Não representa um mecanismo completo de aceleração extrema. Pode não haver plano Alto Desempenho no equipamento. Falhas tentam restaurar ambos; se a recuperação falhar, use **DESATIVAR E RESTAURAR**. O diário persiste em `%LOCALAPPDATA%\ReviveOS\changes.json`. Efeitos visuais podem exigir novo login.

Não executa serviços residentes, não modifica AutoCAD, não desativa proteções, não envia arquivos e não encerra aplicativos automaticamente. Instalação por usuário; funções de diagnóstico que exigem elevação registram o erro por seção.

Backup copia um arquivo fechado ou disponível para leitura, com versões deduplicadas por SHA-256. Recuperação exige destino novo e verifica checksum. O backup no mesmo HD não protege contra falha física. Não há sincronização, backup agendado nem checkpoint de aplicações nesta versão. Desinstalar preserva relatórios, diário e backups; restaure os ajustes antes de desinstalar.

## Implementação e limites

| Área | Estado desta versão |
|---|---|
| Diagnóstico | CIM, processos, serviços, tarefas, hardware, rede e armazenamento; execução real no notebook pendente |
| Otimizador/Turbo | Dois ajustes com simulação, diário persistente, validação e rollback; testes Windows pendentes |
| Processos | Lista com RAM e classificação heurística; não garante identificação de todos os críticos; fechamento cooperativo disponível apenas na biblioteca |
| Wi-Fi | Inspeção netsh/adaptadores; Auto Start não implementado sem identificação do OEM e rádio |
| AutoCAD | Detecta processo na biblioteca e apresenta orientação; sem monitor automático ou Boost validado |
| Nuvem | Catálogo com links oficiais; não há provisionamento, verificação de cota em conta ou jobs remotos |
| Scheduler | Decisão testada usando estimativas fornecidas; sem execução de tarefas |
| RDP | Abertura do cliente para host autorizado na biblioteca; sem host gratuito fornecido ou streaming próprio |
| Backup | Versões locais e restauração com SHA-256 testadas em Linux |
| Browser/Multimedia/Security/Recovery | Orientações e estrutura; módulos avançados ainda pendentes |
| Benchmarks | Não foram executados no notebook; nenhum ganho declarado |
| Instalador | Compilado para Windows; instalação e abertura no Windows ainda não testadas |

Não foi identificada ou disponibilizada infraestrutura Windows gratuita para AutoCAD. Codespaces não executa AutoCAD Windows nativamente. As cotas e preços devem ser verificados nos links oficiais com a conta do usuário; o catálogo não afirma valores atuais.

## Desenvolvimento

SDK .NET 8: `dotnet test tests/ReviveOS.Tests -c Release`.
Compilar: `dotnet publish src/ReviveOS.UI -c Release -r win-x64 --self-contained true -o publish`.
Instalador no Windows: Inno Setup 6, `iscc installer/ReviveOS.iss`.
Alternativa NSIS: `makensis installer/ReviveOS.nsi` após publicar.
Devcontainer para desenvolvimento Linux incluído. WPF não executa em Linux.

Workflow Actions compila, testa, consulta vulnerabilidades e disponibiliza artefato. Não foi executado no GitHub: a credencial recusou criação do repositório. Não há release remota publicada. O código foi versionado localmente para preservar a continuidade.

## Próximos testes no notebook

1. Instalar, abrir e gerar relatório; confirmar modelo, RAM, HD e drivers.
2. Medir condições iniciais equivalentes sem ajustes (alimentação, desenho e aplicações iguais).
3. Aplicar cada ajuste separadamente e verificar restauração, inclusive após fechar e reabrir o app.
4. Identificar estado físico/software do rádio e suporte OEM antes de implementar Wi-Fi Auto Start.
5. Validar reinicialização, partida a frio, suspensão e perda de sinal; não declarar Wi-Fi concluído antes disso.
6. Medir AutoCAD com o mesmo DWG e comandos; registrar regressões e temperaturas disponíveis.

A lista completa de 15 fases continua pendente; esta entrega é uma base executável inicial, não a missão concluída.
