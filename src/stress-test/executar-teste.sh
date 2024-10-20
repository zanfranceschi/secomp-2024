#!/usr/bin/env bash

# Use este script para executar testes locais

RESULTS_WORKSPACE="$(pwd)/user-files/results"
GATLING_BIN_DIR=$HOME/gatling/3.10.5/bin
GATLING_WORKSPACE="$(pwd)/user-files"

echo $GATLING_WORKSPACE

runGatling() {
    sh $GATLING_BIN_DIR/gatling.sh -rm local -s SecompSimulation \
        -rd "Design 04" \
        -rf $RESULTS_WORKSPACE \
        -sf "$GATLING_WORKSPACE/simulations"
}

startTest() {
    for i in {1..20}; do
        # some requests to wake the api instances up :)
        for a in {1..6}; do
            curl --fail http://localhost:9999/ && \
            echo "" && \
            break || sleep 2;
        done
        runGatling && \
        break || sleep 1;
    done
}

startTest
