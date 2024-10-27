É necessário buildar o **k6** com a extensão `xk6-output-timescaledb` (`--with github.com/grafana/xk6-output-timescaledb`). Consulte https://grafana.com/docs/k6/latest/extensions/build-k6-binary-using-docker/ para mais informações.

Para executar o teste no linux/mac, use o seguinte comando:
```shell
./k6 run teste.js \
    -o timescaledb=postgresql://k6:k6@localhost:5432/k6 \
    --tag testid=TESTE-$(date '+%F-%R:%S')
```
