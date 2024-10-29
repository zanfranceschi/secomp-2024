#!/usr/bin/env bash

# script para rodar todos os cenários automaticamente
# subir bacen e timescaledb antes de executar esse script

iniciarTeste() {

    pushd $1
        docker compose up -d
    popd

    for i in {1..20}; do
        curl --fail http://localhost:9999/ && \
        echo "" && \
        curl --fail http://localhost:9999/ && \
        echo "" && \
        curl --fail http://localhost:9999/ && \
        echo "" && \
        ./k6 run stress-test-realtime-results.js \
            -o timescaledb=postgresql://k6:k6@localhost:5432/k6 \
            --tag testid=$2-$(date '+%F-%R:%S') && \
        break || sleep 3;
    done

    pushd $1
        docker compose down
    popd

    echo "Teste ${2} finalizado"
}

brave "http://localhost:3000"

iniciarTeste "../arq01-cenario01" "ARQ01-CENARIO01"
iniciarTeste "../arq01-cenario02" "ARQ01-CENARIO02"
iniciarTeste "../arq02-cenario01" "ARQ02-CENARIO01"
iniciarTeste "../arq02-cenario02" "ARQ02-CENARIO02"
iniciarTeste "../arq03-cenario01" "ARQ03-CENARIO01"
iniciarTeste "../arq03-cenario02" "ARQ03-CENARIO02"

