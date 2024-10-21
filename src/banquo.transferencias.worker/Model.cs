public class Configuracoes
{
    public Func<Guid, string> LimitesUrl { get; private set; }
    public string BacenUrl { get; private set; }
    public Configuracoes(string limitesUrl, string bacenUrl)
    {
        BacenUrl = bacenUrl;
        LimitesUrl = (Guid clientId) => $"{limitesUrl}/{clientId}";
    }
}

record RealizarTransferenciaCommand(
    Guid transferenciaId,
    Guid clienteIdDe,
    Guid clienteIdPara,
    decimal valor);

record TransferenciaRealizadaEvent(
    Guid transferenciaId,
    Guid transferenciaBacenId,
    Guid clienteIdDe,
    Guid clienteIdPara,
    decimal valor);

record LimiteResponseBacen(bool ValorAprovado);

record SolicitacaoTransferenciaRequestBacen(
    Guid clienteIdDe,
    Guid clienteIdPara,
    decimal valor);

record SolicitacaoTransferenciaResponseBacen(Guid transferenciaId);
