# Motores complementares — 08/11/2026

A central híbrida combina funções, não memória ou CPUs como se fossem uma máquina única. Filtra opções por tipo de tarefa, ferramentas locais e autorizações de privacidade. Não escolhe o mais rápido sem medições e não considera um host disponível apenas por existir um cliente RDP/SSH.

| Componente | Papel | Acesso e limite |
|---|---|---|
| Windows + AutoCAD local | Aplicativos nativos, Wi-Fi, ajustes reversíveis e trabalho protegido | Instalação original preservada; ganho depende do notebook |
| AutoCAD Web | Comandos CAD básicos e edição pelo navegador | Login Autodesk existente autorizado pelo usuário; depende de acesso incluído na assinatura; não é o desktop Windows completo |
| Binder | Python/notebooks e exemplos CSV/DXF | Sem cadastro; somente uso pessoal interativo leve, temporário e com dados não confidenciais |
| SageMathCell | Matemática simbólica e cálculos curtos | Sem cadastro; cálculo remoto real testado; sessões limitadas e sem Internet livre |
| Python/SymPy locais | Cálculo e scripts privados | Exigem instalação encontrada; continuam usando hardware local |
| FFmpeg/Blender locais | Vídeo e renderização | Exigem executáveis instalados; não são processamento remoto por serem gratuitos |
| Host SSH ou Windows RDP próprio | Tarefas pesadas ou aplicativo completo, quando compatível | Exige máquina autorizada; nenhuma máquina potente foi provisionada |
| VersionStore local | Preservar versões confirmadas | Protege arquivos já baixados; não recupera alterações que nunca saíram da sessão remota |

## Operação integrada

1. Abra CENTRAL HÍBRIDA e escolha a tarefa. Confirme se os dados podem ser públicos. Dados privados excluem Binder e SageMathCell.
2. Para AutoCAD Web, use Verificar minha assinatura Autodesk. O site oficial deve confirmar o benefício antes de marcar a opção correspondente. O aplicativo não consulta licença por API, não recebe senha e não compra/ativa trial.
3. Avaliar combinação identifica ferramentas no PATH e filtra motores. Abrir/executar selecionado chama o fluxo existente de tarefa local/SSH, abre a sessão interativa ou o serviço oficial. O tipo de arquivo é restringido ao adaptador selecionado para vídeo, Blender ou Python.
4. Em SageMathCell, Copiar exemplo Sage fornece um exemplo público; cole no editor e clique Evaluate. Para Binder, salve o notebook e envie-o manualmente no Jupyter. Não há uploads silenciosos.
5. Baixe resultados no HD e use Proteger resultado baixado para criar uma versão com SHA-256. Essa função não sincroniza arquivos ainda na nuvem.

O acesso local a arquivos do AutoCAD Web depende do navegador, permissões e recursos disponibilizados pela Autodesk. O ReviveOS não assume que dados privados deixem de ser processados pelo serviço só por terem sido abertos do HD.

## Evidência e limitações

SageMathCell executou factor(84) no navegador, retornando 2^2 * 3 * 7 sem login. Binder já teve sessão Python remota testada na versão 0.4. AutoCAD Web foi pesquisado em documentação oficial e integrado como abertura do serviço; login, licença, desenho real e desempenho não foram validados na conta do usuário. A central não demonstra aceleração por simplesmente abrir sites.

JupyterLite foi avaliado, mas executa no navegador local; não é anunciado como CPU remota. Onshape exige conta própria e o plano gratuito possui documentos públicos, portanto não foi adotado para substituir a instalação original.

Não há solução confirmada que, juntando esses serviços, entregue Windows 11/AutoCAD completo com GPU, potência garantida, persistência e ausência de cadastro. Confira REQUIREMENT-COVERAGE.md: requisitos ainda condicionais não são marcados como concluídos.

Fontes primárias:

- https://www.autodesk.com/products/autocad-web/overview
- https://help.autodesk.com/view/ACADWEB/ENU/?guid=access_web
- https://www.sagemath.org/eval.html
- https://sagecell.sagemath.org/
- https://mybinder.readthedocs.io/en/latest/about/user-guidelines.html
- https://jupyterlite.readthedocs.io/en/stable/
- https://cad.onshape.com/help/Content/Plans/managing_your_onshape_account_and_subscription.htm
