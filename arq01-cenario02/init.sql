
CREATE TABLE lancamentos_contabeis (
	id SERIAL PRIMARY KEY,
	operacao VARCHAR(30) NOT NULL,
	cliente_id UUID NOT NULL,
	debito MONEY NOT NULL,
	credito MONEY NOT NULL,
	realizado_em TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE transferencias (
	id SERIAL PRIMARY KEY,
	cliente_id_de UUID NOT NULL,
	cliente_id_para UUID NOT NULL,
	valor MONEY NOT NULL,
	realizada_em TIMESTAMP NOT NULL DEFAULT NOW()
);
