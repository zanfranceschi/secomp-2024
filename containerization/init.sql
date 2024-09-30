
CREATE TABLE lancamentos_contabeis (
	id SERIAL PRIMARY KEY,
	operacao VARCHAR(30) NOT NULL,
	cliente_id UUID NOT NULL,
	debito DECIMAL(10, 4) NOT NULL,
	credito DECIMAL(10, 4) NOT NULL,
	realizado_em TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE transferencias (
	id SERIAL PRIMARY KEY,
	cliente_id_de UUID NOT NULL,
	cliente_id_para UUID NOT NULL,
	valor DECIMAL(10, 4) NOT NULL,
	realizado_em TIMESTAMP NOT NULL DEFAULT NOW()
);

