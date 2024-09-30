record SolicitacaoTransferenciaRequest(Guid clienteId, decimal valor);

record SolicitacaoTransferenciaRequestBacen(Guid clienteIdDe, Guid clienteIdPara, decimal valor);

record SolicitacaoTransferenciaResponseBacen(Guid transferenciaId);
