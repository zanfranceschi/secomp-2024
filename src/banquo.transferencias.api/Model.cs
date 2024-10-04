class Api { }

record SolicitacaoTransferenciaRequest(Guid clienteIdDe, Guid clienteIdPara, decimal valor);

record RealizarTransferenciaCommand(Guid transferenciaId, Guid clienteIdDe, Guid clienteIdPara, decimal valor);
