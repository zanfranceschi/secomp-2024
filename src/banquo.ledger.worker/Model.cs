public record Configuracoes(string DbConnectionString);

record RealizarTransferenciaCommand(Guid transferenciaId, Guid clienteIdDe, Guid clienteIdPara, decimal valor);

record TransferenciaRealizadaEvent(Guid transferenciaId, Guid transferenciaBacenId, Guid clienteIdDe, Guid clienteIdPara, decimal valor);

record LimiteResponseBacen(bool ValorAprovado);

record SolicitacaoTransferenciaRequest(Guid clienteIdDe, Guid clienteIdPara, decimal valor);

enum TransferenciaStatus
{
    Pendente,
    Sucesso,
    Falha
}

record SolicitacaoTransferenciaResponse
{
    public Guid? transferenciaId { get; set; }
    public required TransferenciaStatus status { get; set; }
}

record SolicitacaoTransferenciaRequestBacen(Guid clienteIdDe, Guid clienteIdPara, decimal valor);
record SolicitacaoTransferenciaResponseBacen(Guid transferenciaId);