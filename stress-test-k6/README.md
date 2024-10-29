## Execução Completa com Métricas

É necessário buildar o **k6** com a extensão `xk6-output-timescaledb` (`--with github.com/grafana/xk6-output-timescaledb`).

Por exemplo, para buildar para Linux, use o seguinte comando:
```shell
docker run --rm -it -u "$(id -u):$(id -g)" -v "${PWD}:/xk6" \
    grafana/xk6 build v0.54.0 \
    --with github.com/grafana/xk6-output-timescaledb
```

Para MacOS, o seguinte comando:
```shell
docker run --rm -it -u "$(id -u):$(id -g)" -v "${PWD}:/xk6" \
    -e GOOS=darwin \
    grafana/xk6 build v0.54.0 \
    --with github.com/grafana/xk6-output-timescaledb
```

e para Windows, o seguinte comando:
```cmd
docker run --rm -e GOOS=windows -v "%cd%:/xk6" ^
  grafana/xk6 build v0.54.0 --output k6.exe ^
  --with github.com/grafana/xk6-output-timescaledb
```

Consulte https://grafana.com/docs/k6/latest/extensions/build-k6-binary-using-docker/ para mais informações.

---

Para rodar o teste no linux/mac com emissão de métricas para o timescaledb/grafana, use o seguinte comando:
```shell
./k6 run stress-test-realtime-results.js \
    -o timescaledb=postgresql://k6:k6@localhost:5432/k6 \
    --tag testid=TESTE-$(date '+%F-%R:%S')
```
O arquivo [stress-test-realtime-results.sh](./stress-test-realtime-results.sh) contém o script acima para facilitar a execução do teste.

### preparo

Antes de executar o teste, é necessário, subir os contêineres para capturar/acompanhar as métricas declarados em [./k6-realtime-results](./k6-realtime-results) via `docker compose up`. Para acessar o Grafana, vá para [http://localhost:3000](http://localhost:3000) após os contêineres terem iniciado.

## Execução Simples sem Métricas

Caso queria executar o teste de forma mais simples – sem emissão de métricas –, execute o seguinte comando:

```shell
# necessário ter o k6 instalado
k6 run stress-test.js
```
O arquivo [stress-test.sh](./stress-test.sh) contém o script acima para facilitar a execução do teste.
