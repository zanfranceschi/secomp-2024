
CREATE TABLE transferencias (
	id SERIAL PRIMARY KEY,
	bacen_transferencia_id UUID NOT NULL,
	cliente_id_de UUID NOT NULL,
	cliente_id_para UUID NOT NULL,
	valor MONEY NOT NULL,
	realizada_em TIMESTAMP NOT NULL DEFAULT NOW()
);
