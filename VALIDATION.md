# Verificação 0.2.1

Linux x64, SDK 8.0.427: compilação cruzada WPF Windows x64 concluída. Onze testes passaram: custo de transferência, bloqueio remoto sem autorização, seleção remota compatível, backup/versionamento/corrupção, diário com escrita interrompida e recuperação em nova instância, restauração continua após falha e permite retentativa, preservação do backup contra reaplicação, Python local com resultado verificável e falha registrada, consentimento e validação de alias SSH, recuperação conservadora de sessão interrompida.

O workflow testa adicionalmente o coletor CIM e abertura/fechamento de uma janela WPF no runner Windows. Consulte o resultado do commit na aba Actions antes de considerar esse teste aprovado. Logs de execução estão em validation/v02-* e no artefato Actions.

Não testados no notebook: Wi-Fi e sua tarefa no login, alterações de registro/energia, temperatura, responsividade, AutoCAD, backup de arquivo aberto. Adaptador SSH implementado, com teste ponta a ponta concluído em servidor OpenSSH local (loopback), usando chaves temporárias e host key previamente configurada; host externo ainda pendente. RDP não testado em host remoto. Não há benchmark de ganhos nem teste de reboot físico. Nenhuma dessas validações é declarada concluída.

Integração SSH adicional: servidor OpenSSH real em 127.0.0.1:22229, chaves temporárias, StrictHostKeyChecking=yes. Upload, execução de Python, download, SHA-256 e recuperação do resultado passaram. Esse teste valida o protocolo e o adaptador em loopback; não mede latência externa nem comprova disponibilidade de provedor. Harness reproduzível: `dotnet run --project tests/ReviveOS.SshIntegration -- <alias> --authorize-test-upload`. O comando envia e executa somente o script de teste criado pelo harness; requer host autorizado POSIX com Python 3.

0.2.1 acrescenta dois testes Windows: preservação de valor vizinho no registro ao aplicar/reverter animações e leitura de GUID do plano ativo. Eles são explicitamente ignorados em Linux e executados no runner Windows. Workflow também valida instalação silenciosa, abertura da aplicação instalada e desinstalação. A revisão corrigiu criação desnecessária de chave existente; release 0.2.0 convertida em rascunho antes de qualquer download registrado.

Resultado confirmado no workflow Windows da tag v0.2.1: https://github.com/Leosenha00/revil/actions/runs/42481130710. Todos os 13 testes passaram no Windows. Diagnóstico CIM sem erros opcionais, janela WPF abriu/fechou, instalador instalou, aplicativo instalado abriu/fechou e desinstalação passou. Log em validation/v021-windows-smoke.txt. Instalador da release baixado e SHA-256 conferido: 955f9075bf51be24358f576a789b061b7bbec6f3ffc56501b6be02fd51d697a0. Esses resultados são do runner Windows, não do notebook do usuário.

## 0.3.0 — plano profundo e nuvem

Compilação cruzada Windows concluída; 14 testes passaram no Linux e 4 testes Windows foram explicitamente ignorados nesse ambiente. Inclui novas verificações: plano sem HDD/sem pressão não sugere serviços, filtro de serviços críticos e decisão condicionada por HDD/filas. Os testes Windows adicionais exercitam remoção/restauração de startup com comando contendo espaços/apóstrofo/variável de ambiente e preservação do tipo ExpandString; também consultam serviços opcionais/paginação sem alterar configurações administrativas. O resultado Windows é publicado em Actions. Alteração de serviços/paginação e ganho no notebook ainda não estão validados.

Template Terraform Oracle formatado e validado com Terraform 1.11.4 e provider oficial oracle/oci. Não houve plan autenticado, apply, criação de conta, VM ou cobrança. O template é preparação revisável, não infraestrutura disponível.

Verificação concluída da tag v0.3.0: https://github.com/Leosenha00/revil/actions/runs/42482140918. Os 18 testes passaram no Windows; diagnóstico, abertura/fechamento da janela, instalação silenciosa, abertura instalada e desinstalação passaram. SHA-256 do instalador baixado confirmado: 3f8eb7f0077d68698cb10c76e9b50d0f3616b7beb6967cc6bca54d0f7d748d88. As alterações administrativas de serviços/paginação e os ganhos no notebook continuam pendentes de validação real.

## 0.4.0 — opção sem login

Uma sessão real mybinder.org iniciou sem conta/cartão/login. Kernel Python no host hub.bids.mybinder.org executou soma conhecida, retornando Linux e 49995000; pedido de exclusão do kernel enviado e token temporário removido. Evidência em validation/v04-binder-live-test.json, sem token ou URL privada da sessão. Não foi medido ganho de desempenho. A sessão não foi usada como backend contínuo.

Local: 16 testes passaram; quatro testes exclusivos de Windows foram ignorados no Linux. Novos testes executam todas as células do notebook com CSV/DXF de exemplo, conferem exportação e SHA-256 e impedem sobrescrita de arquivo ao salvar notebook. O teste de notebook é separado do teste remoto e não equivale a benchmark remoto de DXF/AutoCAD. Workflow Windows executa os 20 testes, compila e valida instalação.

Tag v0.4.0 aprovada: https://github.com/Leosenha00/revil/actions/runs/42493043094. Os 20 testes passaram no Windows; diagnóstico, janela WPF, instalação, execução instalada e desinstalação passaram. Instalador baixado e SHA-256 verificado: 593d2724c82a76bb4d2e1e8b7fcb8ca00169907df6cd490360303fb89768ea49.

## 0.5.0

Local: 21 testes passaram; 4 testes Windows foram ignorados no Linux. Cinco testes novos verificam privacidade, exigência de benefício Web e autorização, ausência de host fictício, bloqueio de tarefas pesadas em serviços anônimos e uso de ferramentas compatíveis. Compilação cruzada WPF concluída. O workflow Windows executa os 25 testes e valida instalação/janela/desinstalação.

SageMathCell: cálculo remoto real sem login, factor(84) retornou 2^2 * 3 * 7. Evidência em validation/v05-sage-live-test.json. AutoCAD Web: documentação e abertura oficial integradas; acesso da conta do usuário e desenho real não testados. Nenhuma alegação de ganho no notebook é feita.

Tag v0.5.0 aprovada: https://github.com/Leosenha00/revil/actions/runs/42494281729. Os 25 testes passaram no Windows. Diagnóstico, janela WPF, instalação, execução instalada e desinstalação passaram. Instalador baixado e SHA-256 confirmado: 001dd8066937ad10350e1319f963239a8f7c5d38138d88cfccee9d1c7a97425b. Os fluxos com licença/desenho Autodesk e desempenho no notebook continuam pendentes.

## 0.6.0

Testes novos: catálogo restrito de políticas sem desativar proteção; argumentos de perfil separado; reversão de política preservando valores vizinhos; medição de uma janela Chrome real no runner Windows; roteamento remoto somente com consentimento e dados públicos. Testes Windows não rodam no Linux e são explicitamente ignorados nesse ambiente. A aceitação das políticas pelo Chrome do notebook e o ganho real ainda não foram validados.

Browser.lol: homepage e criação inspecionadas; Chrome apareceu sem login e uma solicitação chegou a Requesting a session. A sessão interativa não foi confirmada e não é marcada como testada/funcional. Não houve reinício de sessão para contornar quotas, nem envio de dados pessoais.

Primeira execução Windows 0.6 (42495749135): 29 testes passaram, e o teste de janela real detectou a janela, mas falhou no teardown porque Chrome continuou ativo após CloseMainWindow. A limpeza foi corrigida para terminar somente a instância vazia e isolada criada pelo teste no runner, com timeout; a medição de produção não encerra processos. O teste será reexecutado antes da publicação.
