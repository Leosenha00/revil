# Escolha de nuvem — verificação em 08/11/2026

Atualização 0.4: para o requisito sem cadastro/login, a opção padrão passa a ser Binder interativo e temporário; veja NO-LOGIN-CLOUD.md. Oracle permanece somente como alternativa opcional com conta, sem provisionamento.

Escolha técnica para tarefas Linux: Oracle Cloud Infrastructure Always Free, shape Ampere A1, por SSH. É um candidato condicionado à elegibilidade da conta, não uma VM já disponível. Nenhuma conta foi criada e nenhum recurso foi provisionado nesta sessão.

## Comparação

| Oferta oficial | Recursos/limites consultados | Adequação |
|---|---|---|
| OCI Always Free A1 | 1.500 OCPU-h e 9.000 GB-h/mês; equivale a até 2 OCPUs ARM/12 GB contínuos; armazenamento combinado até 200 GB na região principal | Python/FFmpeg e Blender quando houver binário ARM64; sem GPU prometida; não executa AutoCAD Windows nativamente |
| OCI micro AMD | CPU compartilhada limitada e 1 GB RAM por VM | Inadequada como workstation AutoCAD |
| Azure conta gratuita | VMs burstable selecionadas, 750 h/mês por 12 meses; B2ts_v2 possui 1 GiB RAM; verificar discos, IP e tráfego separadamente | Oferta temporária e insuficiente como estação CAD pesada; não escolher Windows só por constar como gratuito |
| Google Cloud gratuito | e2-micro; Windows Server excluído da camada gratuita | Não atende desktop Windows CAD gratuito |
| Codespaces/Actions | Desenvolvimento/testes segundo política e cota da conta | Não usar como desktop pessoal de AutoCAD |

A oferta Oracle depende de capacidade regional, pode recolher instâncias ociosas e exige verificação de identidade/cartão. Créditos de teste expiram; não são capacidade gratuita permanente. Não há promessa de disponibilidade ou manutenção de limites. Não atualizar automaticamente para conta paga ou gerar carga artificial para evitar recolhimento.

## Preparação concreta

`cloud/oracle/main.tf` descreve uma única VM Ubuntu ARM64 de 2 OCPUs, 12 GB, boot de 50 GB e SSH restrito ao IPv4 /32 do usuário. Cloud-init instala Python e FFmpeg. Blender depende da disponibilidade de pacote ARM64 e não é anunciado como instalado.

1. Login e verificação de identidade devem ser feitos no site oficial Oracle, sem enviar senhas ou cartão pelo chat.
2. Configurar autenticação OCI oficial em arquivo local protegido. Copiar o exemplo tfvars e preencher IDs regionais e somente chave SSH pública.
3. Confirmar no console que imagem e recursos são elegíveis e que o total da conta permanece dentro das cotas gratuitas, incluindo discos, IP e tráfego. O template não conhece recursos externos a ele e não garante cobrança zero.
4. `terraform init`, `terraform validate`, `terraform plan -out=worker.tfplan`. A variável de confirmação vem falsa para impedir criação sem essa verificação. Revisar plano e custo; somente então aplicar conscientemente. Nenhum apply foi executado aqui.
5. Conferir fingerprint do host por canal confiável, configurar alias OpenSSH e executar `probe-worker.py` no host. O ReviveOS usa esse alias em Tarefa SSH; confirma upload e verifica resultado.

A nuvem só executa tarefas transferidas a ela. Não aumenta automaticamente CPU, RAM ou velocidade do HD local. Para AutoCAD remoto interativo, é necessária uma máquina Windows compatível com recursos gráficos, licença adequada e conexão de baixa latência. Nenhuma oferta permanente gratuita adequada foi confirmada.

## Fontes primárias

- https://docs.oracle.com/en-us/iaas/Content/FreeTier/freetier_topic-Always_Free_Resources.htm
- https://www.oracle.com/cloud/free/
- https://azure.microsoft.com/en-us/pricing/details/virtual-machines/windows/
- https://github.com/MicrosoftDocs/azure-compute-docs/blob/main/articles/virtual-machines/sizes/general-purpose/bsv2-series.md
- https://docs.cloud.google.com/compute/docs/instances/windows/creating-managing-windows-instances
- https://www.autodesk.com/support/technical/article/caas/sfdcarticles/sfdcarticles/System-requirements-for-AutoCAD-2026-including-Specialized-Toolsets.html
