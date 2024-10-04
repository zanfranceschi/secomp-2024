#!/usr/bin/env bash

#docker build -t zanfranceschi/secomp-design-04-banquo.transferencias.api -t secomp-design-04-banquo.transferencias.api .

docker build -t secomp-design-04-banquo.transferencias.api .

# docker push zanfranceschi/secomp-design-04-banquo.transferencias.api:latest

# docker run --rm -p 8080:8080 secomp-design-04-banquo.transferencias.api
