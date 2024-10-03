#!/usr/bin/env bash

# Use este script para executar testes locais

RESULTS_WORKSPACE="$(pwd)/user-files/results"
GATLING_BIN_DIR=$HOME/gatling/3.10.5/bin
GATLING_WORKSPACE="$(pwd)/user-files"

echo $GATLING_WORKSPACE

runGatling() {
    sh $GATLING_BIN_DIR/gatling.sh -rm local -s SecompSimulation \
        -rd "design 03 - banquo: 3x 0.50/100MB | db: 0.50/100MB | bacen: 1.0/50MB" \
        -rf $RESULTS_WORKSPACE \
        -sf "$GATLING_WORKSPACE/simulations"
}

startTest() {
    for i in {1..20}; do
        # 2 requests to wake the 2 api instances up :)
        curl --fail http://localhost:9999/ && \
        echo "" && \
        curl --fail http://localhost:9999/ && \
        echo "" && \
        runGatling && \
        break || sleep 2;
    done
}

startTest
