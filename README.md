# revil — ReviveOS Hybrid Extreme 0.7.0

Aplicativo Windows 11 x64 em C#/.NET 8 e WPF. Instalador com runtime incluído; não exige instalar .NET separadamente. Código e distribuição: https://github.com/Leosenha00/revil.

**Projeto em desenvolvimento: ainda não atende a todos os critérios do prompt master.** Não há acesso ao notebook e não há ganho de desempenho medido nele.

## Usar no notebook

Baixe o instalador na release ou em Actions → Windows build and installer → Artifacts. Execute e abra pelo menu Iniciar. Comece por **Diagnosticar**, depois **Simular ajustes**. Não é necessário executar como administrador para os ajustes de usuário. Algumas informações ou tarefas agendadas podem exigir permissões adicionais; falhas são exibidas.

Dados e logs ficam em `%LOCALAPPDATA%\ReviveOS`. Relatórios podem conter nomes, MAC, redes e comandos de inicialização: revise antes de compartilhar. Não há upload automático.

## Funções disponíveis

| Área | Implementação | Limite de validação |
|---|---|---|
| Diagnóstico | CIM, CPU, memória, discos, GPU, BIOS, rede, drivers, tarefas, inicialização, aplicações e processos | Executado no runner Windows pelo workflow; notebook pendente |
| Ajustes | Animações, transparência, Game DVR por usuário e plano de energia; riscos, simulação e reversão individual | Leitura/escrita real no Windows; efeito e suporte no notebook pendentes |
| Recuperação | Diário antes de escrever, validação, erros individuais e retentativa; restaura também diário 0.1 | Testes com falha injetada e reinício do objeto; desligamento real pendente |
| Turbo manual | Plano de energia + animações, confirmação e rollback em caso de falha | Alto Desempenho pode não existir; maior calor e consumo |
| Monitor por aplicativo | Detecta primeiro plano a cada 5 s (AutoCAD, Chrome, Edge, Firefox, VS Code, Python, Blender); reduz apenas animações/transparência e restaura ao sair | Opt-in, ReviveOS aberto; não altera executáveis e não promete fluidez |
| Wi-Fi | API WLAN oficial consulta rádio e liga software quando hardware permite; tarefa opt-in com 3 tentativas no login; log e restauração do rádio | Não controla firmware, BIOS, tecla Fn, adaptador desabilitado ou bloqueio físico; testes no notebook pendentes |
| Processos | RAM e classificação heurística; fechamento cooperativo confirmado; bloqueio de processos críticos conhecidos | Não força término, não suspende processos; classificação não é diagnóstico de segurança |
| Benchmarks | 5 amostras de CPU/RAM/fila de disco; teste temporário de 4 MiB, tempos reais e condições registradas | Leitura pode estar em cache; não mede ganho de AutoCAD nem IOPS físicas |
| Tarefas | Adaptadores Python, FFmpeg e Blender; fila serial, histórico persistente, log limitado, resultado e SHA-256; cancelamento local | Ferramentas devem estar instaladas; processo sem progresso numerado fica sem porcentagem |
| SSH | Upload explícito de um arquivo, execução, download e checksum remoto; chave e host previamente verificados; recuperação de resultado confirmado | Exige máquina autorizada do usuário; cancelamento da conexão não garante término remoto; sem cotas inventadas |
| RDP | Abre cliente oficial para host confirmado | Exige host Windows com RDP suportado e licenças adequadas; não fornece infraestrutura |
| Backup | Versões deduplicadas por SHA-256; restauração sem sobrescrever; limite de espaço; monitor opt-in a cada 60 s para um arquivo estável | Só enquanto ReviveOS aberto; não captura RAM ou alterações ainda não salvas; mesmo HD não protege contra falha física |
| Scheduler | Seleção LOCAL/REMOTE por estimativas de rede, compatibilidade, autorização, cota e transferências | Biblioteca testada; ainda não integrada à seleção automática de jobs |
| Nuvem | Catálogo de links oficiais e adaptador SSH | Sem provisionamento Codespaces/Colab, sem GPU gratuita garantida ou consulta automática de cotas |

## Wi-Fi no login

1. Use **Wi-Fi** para diagnóstico e **Rádio Wi-Fi: ligar** para tentativa manual suportada.
2. Se o hardware permitir, **Wi-Fi no login** cria uma tarefa por usuário para `ReviveOS.exe --wifi-recover`. São três tentativas com intervalo de cinco segundos, sem loop permanente. Login é necessário: não promete rádio ligado antes da tela de login.
3. Valide reinício, desligamento/partida a frio, retorno de suspensão e perda de sinal. Confira `%LOCALAPPDATA%\ReviveOS\wifi-last-attempt.json`.
4. **Remover Wi-Fi no login** remove a tarefa. **Restaurar rádio** remove a tarefa e restaura o estado de software anterior quando existe backup.

A API pode retornar acesso negado conforme versão do Windows e permissões de rede/localização. Não há alteração de BIOS/firmware ou drivers OEM automática. Ativação do rádio não garante conexão: use o Windows para marcar conexão automática de uma rede conhecida. Não são coletadas senhas Wi-Fi.

## Processamento local e SSH

Selecione `.py`, vídeo ou `.blend` em **Tarefa local** ou **Tarefa SSH**. Scripts Python devem produzir `result.json` no diretório de trabalho. FFmpeg converte vídeo para MP4 H.264/AAC; Blender renderiza o frame 1 em PNG. Execute apenas arquivos confiáveis. São processos normais com permissões da conta, não sandbox.

No SSH, configure um alias OpenSSH em `~/.ssh/config` com chave privada protegida e host key validada por canal confiável. O aplicativo usa `BatchMode=yes` e `StrictHostKeyChecking=yes`, sem aceitar novas chaves automaticamente. O alias aceita letras, números, ponto, hífen e underscore. Não há campos para senhas. O host precisa de shell POSIX, `sha256sum` e das ferramentas escolhidas. Windows remoto sem ambiente POSIX não é suportado neste adaptador.

Arquivos ficam em `.reviveos/jobs/<id>` no host e `%LOCALAPPDATA%\ReviveOS\jobs/<id>` localmente. Resultados parciais ficam como `.partial` até verificação. O histórico registra falhas e sessões interrompidas. **Recuperar resultado SSH** baixa um arquivo existente e verifica SHA-256; não reinicia processos nem repete tarefas automaticamente. Não há retomada por blocos de transferências nem garantia de preservar RAM remota.

**Monitorar backup** seleciona um arquivo, verifica data/tamanho a cada minuto e copia apenas após pelo menos dez segundos sem gravação. Clique novamente para parar. Não aciona salvamento dentro do AutoCAD; use salvamento automático oficial do aplicativo.

## Segurança e recuperação

Não desativa Defender, firewall, Windows Update ou BitLocker. Não encerra processos automaticamente, não altera firmware nem apaga arquivos pessoais. Não armazena credenciais de nuvem. Logs de tarefas podem conter dados gerados pelo seu script; não compartilhe sem revisar.

Antes de aplicar novos ajustes, restaure pendências. O app avisa se o diário contém mudanças de uma sessão anterior. Ao fechar, restaura ajustes de monitor automático; ajustes manuais permanecem até restauração explícita. Desinstalar preserva dados em `%LOCALAPPDATA%\ReviveOS`; restaure ajustes e remova tarefa Wi-Fi antes de desinstalar. Instalador não tem assinatura Authenticode.

## Compilar e testar

SDK .NET 8 e Python para integração dos testes:

```sh
dotnet test tests/ReviveOS.Tests -c Release
dotnet publish src/ReviveOS.UI -c Release -r win-x64 --self-contained true -o publish
```

Inno Setup 6: `iscc installer/ReviveOS.iss`. Alternativa NSIS: `makensis installer/ReviveOS.nsi`. O workflow Windows executa testes, consulta dependências, publica, valida diagnóstico e abertura/fechamento da janela WPF e gera instalador. O devcontainer suporta desenvolvimento Linux, mas WPF exige Windows para execução.

## Ainda pendente

- Validação real de Wi-Fi, AutoCAD, temperaturas e ganhos no notebook.
- Ampliar a lista validada de inicialização, indexação por pasta e serviços opcionais; a versão 0.3 já gerencia apenas a lista explícita documentada.
- Integração UI completa do scheduler e estimativas de tarefas.
- Provisionamento oficial de provedores com consulta autenticada de limites e custos; OAuth/credenciais seguras quando houver integração.
- Medição de abertura de programas, regressões de benchmark e latência de streaming.
- Streaming próprio, RustDesk/Sunshine/Moonlight, controle FPS e resolução.
- Retomada de transferências por blocos, sincronização de diretórios com conflitos e recuperação de jobs específicos por checkpoint.
- Ajustes internos do AutoCAD condicionados à versão e aos resultados reais.

Não há infraestrutura Windows gratuita identificada ou disponibilizada para AutoCAD; Codespaces não executa AutoCAD Windows nativamente. Não há hardware virtual adicional transformando HD em RAM.

## Releases

Tags `vX.Y.Z` acionam compilação Windows, testes e smoke test; somente após aprovação o job de release publica o instalador e checksum no GitHub. O job de publicação tem permissão de escrita apenas para esse passo.

Teste SSH opcional: `dotnet run --project tests/ReviveOS.SshIntegration -- <alias> --authorize-test-upload`. Usa um script de teste mínimo e deixa seus arquivos em `.reviveos/jobs/<id>` no host autorizado.

## Otimização profunda (0.7.0)

O botão **OTIMIZAÇÃO PROFUNDA** prepara um plano real e permite selecionar alterações antes de executar. Suporta remover da inicialização por usuário OneDrive, Spotify, Discord, Steam e Epic Games sem desinstalar; pausar/desabilitar SysMain e Windows Search individualmente; e restaurar paginação gerenciada pelo sistema. Essas mudanças gravam snapshots, validam a leitura e oferecem restauração individual/completa. Serviços e paginação exigem executar o aplicativo como administrador. Paginação requer reboot tanto para efeito quanto para restauração completa.

O plano só sugere SysMain se o serviço existe e há HDD confirmado com três amostras de fila de disco cuja média é pelo menos 1. Não significa que SysMain seja a causa: meça antes/depois e reverta regressões. Windows Search não é selecionado automaticamente; sua suspensão prejudica busca e indexação, inclusive de Outlook. Em HD, SysMain também pode beneficiar a abertura de apps. OneDrive deixa de sincronizar até aberto manualmente ao ser retirado do login.

A configuração da paginação é gerenciada via CIM; não são apagados arquivos pagefile.sys por operações de arquivos. Configurações de caminho não padronizadas são recusadas para preservar reversão. O diário fica em deep-changes.json. Antes de desinstalar, use **DESATIVAR E RESTAURAR** como administrador quando houver alterações administrativas.

Decisão de nuvem e template Oracle: [docs/CLOUD-DECISION.md](docs/CLOUD-DECISION.md). O template foi preparado para uma conta do usuário; não foi aplicado e não implica máquina já provisionada ou cobrança zero. O usuário precisa fazer login e verificar elegibilidade/cotas pelo site oficial.

## Sem cadastro ou login (0.4)

O botão **Nuvem sem login** abre Binder no navegador e salva um notebook portátil de cálculos, CSV e contagem de entidades DXF ASCII. O uso é pessoal, interativo e leve: envie o notebook pelo Upload do Jupyter, execute e baixe os resultados no HD antes de encerrar. Nenhum arquivo é enviado automaticamente. Não é um backend contínuo nem executa Windows/AutoCAD. Veja [limites e teste real](docs/NO-LOGIN-CLOUD.md). O repositório privado não foi exposto.

## Central híbrida (0.5)

O botão **CENTRAL HÍBRIDA** reúne roteamento por tarefa e privacidade, AutoCAD local/Web, Binder, SageMathCell, ferramentas locais, host SSH/RDP próprio e backup de resultados baixados. AutoCAD Web usa somente o login Autodesk existente autorizado e exige confirmação de benefício na assinatura. Nenhuma assinatura é comprada. Binder/SageMathCell são excluídos de tarefas privadas e de processamento pesado.

Veja [componentes e operação](docs/HYBRID-COMPONENTS.md) e [o que ainda falta nos 15 requisitos](docs/REQUIREMENT-COVERAGE.md). A integração não soma RAM de serviços e não declara a missão completa.

## Chrome local e híbrido (0.6)

**CHROME BOOST** oferece políticas de memória/background com rollback, um perfil leve separado e medição de abertura até janela visível. Não apaga perfil/cache nem encerra Chrome para medir. A Central Híbrida inclui navegação web com alternativa remota experimental e consentimento para páginas públicas. Veja [condições, riscos e pesquisa](docs/CHROME-HYBRID.md). Rapidez no notebook não foi validada; navegador remoto não acelera diretamente o lançamento local.

## Administrador e CPU (0.7)

**CPU E ADMINISTRADOR** oferece inspeção de limites suportados e solicitação por UAC para ações reais. Um helper separado executa operações fixas, valida o mesmo usuário e encerra após responder. Ajustes profundos também pedem UAC quando necessário. `dotnet publish src/ReviveOS.Admin -c Release -r win-x64 --self-contained true -o publish/admin` deve ser executado antes de gerar o instalador; o workflow já faz isso.

O ajuste de CPU é somente estado máximo AC de 100%, com snapshot e reversão. Não é overclock nem injeção/desbloqueio do processador. O procedimento de bypass alegado ainda não foi identificado; não foi implementado ou marcado como funcional. Leia [escopo e testes](docs/ADMIN-CPU.md).
