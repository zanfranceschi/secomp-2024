public record Configuracoes(string limitesUrl, string bacenUrl);


record RealizarTransferenciaCommand(Guid transferenciaId, Guid clienteIdDe, Guid clienteIdPara, decimal valor);

record TransferenciaRealizadaEvent(Guid transferenciaId, Guid clienteIdDe, Guid clienteIdPara, decimal valor);


record SolicitacaoTransferenciaRequest(Guid clienteIdDe, Guid clienteIdPara, decimal valor);

enum TransferenciaStatus
{
    Pendente,
    Sucesso,
    Falha
}

record SolicitacaoTransferenciaResponse
{
    internal Guid? transferenciaId { get; set; }
    internal required TransferenciaStatus status { get; set; }
}

record SolicitacaoTransferenciaRequestBacen(Guid clienteIdDe, Guid clienteIdPara, decimal valor);
record SolicitacaoTransferenciaResponseBacen(Guid transferenciaId);