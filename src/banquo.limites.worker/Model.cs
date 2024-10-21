record TransferenciaRealizadaEvent(
    Guid transferenciaId, 
    Guid transferenciaBacenId,
    Guid clienteIdDe,
    Guid clienteIdPara,
    decimal valor);
