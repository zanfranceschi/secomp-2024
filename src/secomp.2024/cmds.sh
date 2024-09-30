#!/usr/bin/env bash


docker build -t secomp-2024 .

docker run --rm -p 8080:8080 \
    #--add-host=host.docker.internal:host-gateway \
    --network host \
    -e DB_CONNECTION_STRING="Host=localhost;Username=admin;Password=123;Database=secomp;Minimum Pool Size=10;Maximum Pool Size=10;Application Name=Secomp 2024" \
    -e RABBITMQ_URL="amqp://guest:guest@localhost:5672/" \
    secomp-2024

docker run --rm -p 8080:8080 \
    --network="host" \
    -e DB_CONNECTION_STRING="Host=localhost;Username=admin;Password=123;Database=secomp;Minimum Pool Size=10;Maximum Pool Size=10;Application Name=Secomp 2024" \
    -e RABBITMQ_URL="amqp://guest:guest@localhost:5672/" \
    secomp-2024