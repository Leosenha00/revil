ReviveOS Hybrid Extreme 0.7.0 implementa solicitação administrativa por UAC com componente separado requireAdministrator e operações restritas. Ajustes profundos solicitam elevação quando necessária; o helper verifica o mesmo SID e grava resultado local estruturado.

Acrescenta CPU E ADMINISTRADOR: inspeção real de plano de energia, CPU/RAM e boot somente leitura; opção de permitir estado máximo AC de 100% quando havia teto inferior, usando APIs Win32, snapshot e reversão. Se o teto já era 100%, informa ausência desse limite sem declarar aceleração.

Não implementa injeção de código, desbloqueio de multiplicador ou bypass universal do T4500. Nenhum procedimento compatível foi identificado. Não modifica BIOS/MSRs/bootloader nem proteções térmicas. Autorizar administrador não cria novos recursos físicos.

Testes Windows verificam alteração e reversão em uma cópia inativa de plano, preservando o ativo. Smoke do helper usa token já elevado; prompt UAC e resultados no notebook ainda precisam ser testados. Nenhum ganho foi medido na máquina da foto. Veja docs/ADMIN-CPU.md.
