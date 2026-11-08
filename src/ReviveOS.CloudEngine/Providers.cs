namespace ReviveOS.CloudEngine;
public record Provider(string Name,string Url,string Capability,string Status);
public static class Providers {
 public static Provider[] Catalog=>[
 new("Browser.lol — experimental","https://browser.lol/en/create","Chrome remoto temporário com entrada anônima documentada; não acelera inicialização do Chrome local","Página de criação inspecionada; solicitação enviada, mas sessão interativa não confirmada. Anúncios e limites; não contratado"),
 new("AutoCAD Web — assinatura existente","https://web.autocad.com/","Comandos CAD básicos pelo navegador; acesso depende de assinatura AutoCAD/AutoCAD LT ou Web","Usuário autorizou usar login Autodesk existente; licença e operação com desenhos ainda não validadas"),
 new("SageMathCell — sem login","https://sagecell.sagemath.org/","Cálculos matemáticos curtos em servidor remoto, sem persistência","Cálculo factor(84) executado e conferido em 08/11/2026; não é VM Windows ou backend pesado"),
 new("Binder — sem login","https://mybinder.readthedocs.io/en/latest/about/user-guidelines.html","Linux temporário para Python e exploração leve; sem Windows/AutoCAD/GPU prometidos","Sessão e cálculo Python testados em 08/11/2026. Sem backend automático ou persistência; botão abre uso interativo pessoal"),
 new("OCI Always Free A1","https://docs.oracle.com/en-us/iaas/Content/FreeTier/freetier_topic-Always_Free_Resources.htm","Candidato Linux ARM: até 2 OCPUs/12 GB segundo documentação consultada em 08/11/2026; sem AutoCAD Windows nativo","Não provisionado. Exige login, elegibilidade, cota total e capacidade regional; template em cloud/oracle"),
 new("Codespaces","https://docs.github.com/en/billing/managing-billing-for-your-products/managing-billing-for-github-codespaces/about-billing-for-github-codespaces","Desenvolvimento Linux; recursos e franquia dependem da conta","Sem sessão conectada; verificar cota e limite de gastos antes de criar"),
 new("GitHub Actions","https://docs.github.com/en/actions","Compilação e testes; não desktop pessoal","Workflow preparado; execução depende de repositório autorizado"),
 new("Colab","https://research.google.com/colaboratory/faq.html","Notebooks permitidos; GPU não garantida","Não conectado; limites variáveis"),
 new("SSH autorizado","https://www.openssh.com/","Comandos em máquina fornecida pelo usuário","Adaptador SSH por alias configurado pelo usuário; autenticação por chave; não fornece capacidade gratuita")];
}
