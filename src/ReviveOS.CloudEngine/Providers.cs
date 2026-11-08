namespace ReviveOS.CloudEngine;
public record Provider(string Name,string Url,string Capability,string Status);
public static class Providers {
 public static Provider[] Catalog=>[
 new("Codespaces","https://docs.github.com/en/billing/managing-billing-for-your-products/managing-billing-for-github-codespaces/about-billing-for-github-codespaces","Desenvolvimento Linux; recursos e franquia dependem da conta","Sem sessão conectada; verificar cota e limite de gastos antes de criar"),
 new("GitHub Actions","https://docs.github.com/en/actions","Compilação e testes; não desktop pessoal","Workflow preparado; execução depende de repositório autorizado"),
 new("Colab","https://research.google.com/colaboratory/faq.html","Notebooks permitidos; GPU não garantida","Não conectado; limites variáveis"),
 new("SSH autorizado","https://www.openssh.com/","Comandos em máquina fornecida pelo usuário","Nenhum host configurado; não fornece capacidade gratuita")];
}
