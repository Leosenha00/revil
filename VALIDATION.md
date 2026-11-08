# Verificação 0.2.0

Linux x64, SDK 8.0.427: compilação cruzada WPF Windows x64 concluída. Dez testes passaram: custo de transferência, bloqueio remoto sem autorização, seleção remota compatível, backup/versionamento/corrupção, diário com escrita interrompida e recuperação em nova instância, restauração continua após falha e permite retentativa, preservação do backup contra reaplicação, Python local com resultado verificável e falha registrada, consentimento e validação de alias SSH, recuperação conservadora de sessão interrompida.

O workflow testa adicionalmente o coletor CIM e abertura/fechamento de uma janela WPF no runner Windows. Consulte o resultado do commit na aba Actions antes de considerar esse teste aprovado. Logs de execução estão em validation/v02-* e no artefato Actions.

Não testados no notebook: Wi-Fi e sua tarefa no login, alterações de registro/energia, temperatura, responsividade, AutoCAD, backup de arquivo aberto. Adaptador SSH implementado, mas ainda sem teste ponta a ponta em host autorizado com chaves reais. RDP não testado em host remoto. Não há benchmark de ganhos nem teste de reboot físico. Nenhuma dessas validações é declarada concluída.
