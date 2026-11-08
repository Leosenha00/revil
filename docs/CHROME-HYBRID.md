# Chrome híbrido — 0.6

Objetivo: reduzir trabalho local quando apropriado, preservar o perfil original e medir a abertura real, sem prometer que servidores anônimos aceleram a inicialização do Chrome instalado.

## Funções implementadas

CHROME BOOST localiza o Chrome instalado e permite políticas documentadas por usuário: HighEfficiencyModeEnabled, MemorySaverModeSavings, BackgroundModeEnabled e NetworkPredictionOptions. O aplicativo exige versão compatível, recusa políticas de máquina conflitantes/gerenciamento detectado, faz snapshot antes de escrever, valida o registro e oferece reversão. Não altera Safe Browsing, sandbox, segurança de sites ou atualizações. A aceitação da política deve ser conferida em chrome://policy após reabrir. Não há registro/enrollment em serviço Google ou ReviveOS. O Chrome pode exibir indicação de gerenciamento por existirem políticas locais.

Memory Saver pode liberar RAM de abas inativas; abas podem recarregar e perder estado não salvo pelo site. Desativar background mode reduz concorrência após fechar, mas elimina a vantagem de manter o navegador aquecido. Desativar previsão de páginas é opcional: pode reduzir trabalho concorrente e também tornar carregamentos mais lentos. Nenhum desses efeitos é vendido como ganho universal de abertura.

O perfil leve usa --user-data-dir em `%LOCALAPPDATA%\ReviveOS\ChromeLite`, separado de favoritos, senhas, sessões e extensões originais. Há abertura explícita e atalho gerenciado novo; o ícone habitual não é substituído. O primeiro uso pode ser mais lento. O atalho pode ser revertido, mas o perfil passa a conter dados pessoais após uso e nunca é apagado automaticamente.

A medição exige Chrome fechado normalmente. O programa não encerra processos nem descarta trabalho para medir. Cronometra até observar uma janela Chrome visível, sem afirmar que uma página já está carregada. Não expurga cache, e primeiro uso/estado do perfil são condições diferentes. Registra tempo real e condições em benchmarks. Não gera número em caso de timeout. Comparações de ganho exigem o mesmo perfil, cache/carga equivalentes e teste no notebook.

Para revisar abas de inicialização e extensões, abre páginas internas do Chrome. Não usa RestoreOnStartup como ajuste universal porque essa política possui restrições em instalações Windows não gerenciadas. Não modifica arquivos Preferences/Secure Preferences protegidos nem deleta histórico/cache para produzir um resultado artificial.

## Navegadores remotos sem cadastro

Browser.lol documenta uso anônimo com anúncios e limites. A página de criação mostrou Chrome sem exigir login; uma solicitação chegou a Requesting a session, mas navegação interativa não foi confirmada. Por isso a opção é experimental para páginas públicas, sem promessa de disponibilidade. O ReviveOS não envia perfil, cookies, credenciais ou arquivos e não reinicia sessões para contornar cotas. O usuário deve encerrar a sessão no provedor e baixar arquivos confirmados antes do fim.

Browserling oferece sandbox gratuito curto, descrito para testes/avaliação, não navegador pessoal permanente. Hyperbeam exige chave de API para integração; Kasm fornece tecnologia de streaming, mas precisa de infraestrutura/autorização. Não foram adotados como servidores gratuitos permanentes.

O navegador remoto pode executar páginas fora do notebook, mas exige abrir um cliente local, conexão de boa latência e decodificação de vídeo. Pode ser mais lento ou consumir mais recursos na máquina fraca. Não é pooling de RAM, extensão de kernel ou um Chrome Windows inteiro distribuído entre Binder e SageMathCell.

Fontes primárias consultadas em 08/11/2026:

- https://support.google.com/chrome/answer/12929150
- https://chromeenterprise.google/policies/background-mode-enabled/
- https://chromium.googlesource.com/chromium/src/+/HEAD/components/policy/resources/templates/policy_definitions/Miscellaneous/HighEfficiencyModeEnabled.yaml
- https://chromium.googlesource.com/chromium/src/+/HEAD/components/policy/resources/templates/policy_definitions/Miscellaneous/MemorySaverModeSavings.yaml
- https://chromeenterprise.google/policies/network-prediction-options/
- https://browser.lol/en/legal/fair-use
- https://browser.lol/en/legal/terms
- https://www.browserling.com/cybersecurity/pricing
- https://next.hyperbeam.com/docs/home/getting-started/
