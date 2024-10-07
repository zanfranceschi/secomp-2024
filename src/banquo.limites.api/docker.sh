#!/usr/bin/env bash

#docker build -t zanfranceschi/secomp-design-04-banquo.limites.api -t secomp-design-04-banquo.limites.api .

docker build -t secomp-design-04-banquo.limites.api .

# docker push zanfranceschi/secomp-design-04-banquo.limites.api:latest

# docker run --rm -p 8080:8080 secomp-design-04-banquo.limites.api
