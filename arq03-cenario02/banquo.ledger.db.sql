
CREATE TABLE lancamentos_contabeis (
	id SERIAL PRIMARY KEY,
	operacao VARCHAR(30) NOT NULL,
	cliente_id UUID NOT NULL,
	debito MONEY NOT NULL,
	credito MONEY NOT NULL,
	realizado_em TIMESTAMP NOT NULL DEFAULT NOW()
);

