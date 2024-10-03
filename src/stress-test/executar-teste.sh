#!/usr/bin/env bash

# Use este script para executar testes locais

RESULTS_WORKSPACE="$(pwd)/user-files/results"
GATLING_BIN_DIR=$HOME/gatling/3.10.5/bin
GATLING_WORKSPACE="$(pwd)/user-files"

echo $GATLING_WORKSPACE

runGatling() {
    sh $GATLING_BIN_DIR/gatling.sh -rm local -s SecompSimulation \
        -rd "design 01 - banquo: 0.25/50MB | db: 0.25/50MB | bacen: 1.0/50MB" \
        -rf $RESULTS_WORKSPACE \
        -sf "$GATLING_WORKSPACE/simulations"
}

startTest() {
    for i in {1..20}; do
        # 2 requests to wake the 2 api instances up :)
        curl --fail http://localhost:8080/ && \
        echo "" && \
        curl --fail http://localhost:8080/ && \
        echo "" && \
        runGatling && \
        break || sleep 2;
    done
}

startTest