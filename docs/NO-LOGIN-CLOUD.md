# Nuvem sem cadastro — 08/11/2026

A opção padrão sem conta é Binder, aberto no navegador pelo botão Nuvem sem login. O ReviveOS gera um notebook portátil para matemática e resumos de CSV/DXF ASCII. O usuário envia o notebook e dados de exemplo explicitamente no Jupyter e baixa o resultado e checksum. Não há upload silencioso, reconexão automática, uso como desktop Windows ou backend de processamento contínuo.

A documentação oficial descreve sessões sem login, recursos de memória de 1–2 GB, expiração por inatividade e duração limitada. Não há armazenamento persistente ou garantia de disponibilidade. Não é solução para AutoCAD remoto nem GPU. Limites e disponibilidade devem ser conferidos no serviço; não tentar evitá-los.

Use apenas dados não confidenciais. O repositório revil continua privado: não foi tornado público para usar Binder. A sessão utiliza o exemplo público oficial binder-examples/requirements, fixado em commit. Não são enviados os dados do cadastro Oracle, tokens ou credenciais do GitHub.

Validação real: uma sessão anônima iniciou em hub.bids.mybinder.org; um kernel Python executou sum(range(10000)), retornando 49995000 e sistema Linux. Foi enviado pedido de encerramento do kernel pela API; o teste não verificou o estado posterior. Não houve benchmark comparativo nem promessa de aceleração do notebook. O notebook de CSV/DXF tem teste automatizado de execução e checksum, separado desse teste remoto.

CoCalc foi avaliado: sua página de teste anônimo informa indisponibilidade e redireciona ao serviço atual; não foi adotado como opção sem conta. JupyterLite executa no navegador local e, portanto, não é anunciado como nuvem para descarregar CPU.

Fontes: https://mybinder.readthedocs.io/en/latest/about/user-guidelines.html e https://cocalc.com/auth/try. A infraestrutura Binder é voluntária e não deve subsidiar um produto comercial; esta integração é somente uma abertura explícita para uso pessoal interativo leve, sem filas automáticas. Para tarefas privadas/pesadas e persistência garantida, é necessário um host autorizado compatível; nenhuma opção adequada sem conta foi confirmada.
