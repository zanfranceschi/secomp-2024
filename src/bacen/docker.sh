#!/usr/bin/env bash

docker build -t zanfranceschi/secomp-design-01-bacen -t secomp-design-01-bacen .

docker build -t secomp-design-01-bacen .

docker run --rm -p 8080:8080 secomp-design-01-bacen

# docker push zanfranceschi/secomp-design-01-bacen:latest
