using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

record SolicitacaoTransferencia(Guid clienteIdDe, Guid clienteIdPara, decimal valor);
