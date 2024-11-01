#!/usr/bin/env bash

# script pra buildar todas as imagens das aplicações localizadas no diretório `src` – necessário para rodar os testes

root_dir=$(pwd)

for service_dir in "$root_dir/../src/banquo" \
                   "$root_dir/../src/bacen" \
                   "$root_dir/../src/banquo.ledger.worker" \
                   "$root_dir/../src/banquo.limites.api" \
                   "$root_dir/../src/banquo.limites.worker" \
                   "$root_dir/../src/banquo.transferencias.api" \
                   "$root_dir/../src/banquo.transferencias.worker"
do
    cd $service_dir
    current_dir=$(basename $service_dir)
    docker_image_name="${current_dir//./-}"
    docker build -t $docker_image_name .
done
