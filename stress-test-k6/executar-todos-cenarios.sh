#!/usr/bin/env bash

# script para rodar todos os cenários automaticamente

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

# k6 timescaledb / grafana
pushd ./k6-realtime-results
    docker compose up -d
popd

# k6 timescaledb / grafana
pushd ../bacen
    docker compose up -d
    sleep 5
popd

brave "http://localhost:3000/d/d5b60252-63b6-409f-9b17-a967db8e01f2/secomp-apresentacao?orgId=1&refresh=5s&viewPanel=2"

iniciarTeste "../arq01-cenario01" "ARQ01-CENARIO01"
iniciarTeste "../arq01-cenario02" "ARQ01-CENARIO02"
iniciarTeste "../arq02-cenario01" "ARQ02-CENARIO01"
iniciarTeste "../arq02-cenario02" "ARQ02-CENARIO02"
iniciarTeste "../arq03-cenario01" "ARQ03-CENARIO01"
iniciarTeste "../arq03-cenario02" "ARQ03-CENARIO02"

