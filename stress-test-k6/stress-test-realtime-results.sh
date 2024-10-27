#!/usr/bin/env bash

./k6 run stress-test-realtime-results.js \
    -o timescaledb=postgresql://k6:k6@localhost:5432/k6 \
    --tag testid=TESTE-$(date '+%F-%R:%S')