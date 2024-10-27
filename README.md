# Escalabilidade: Uma Demonstração Prática
## SECOMP XII / 2024

**Repositório com o código fonte e artefatos da apresentação para SECOMP XII realizada em 01/11/2024 na UFSCar.**

## Organização do Repositório

### Diretórios e Arquivos

[./arq01-cenario01](./arq01-cenario01) - Artefatos para executar o cenário 01 da arquitetura 01.

[./arq01-cenario02](./arq01-cenario02) - Artefatos para executar o cenário 02 da arquitetura 01.

[./arq02-cenario01](./arq02-cenario01) - Artefatos para executar o cenário 01 da arquitetura 02.

[./arq02-cenario02](./arq02-cenario02) - Artefatos para executar o cenário 02 da arquitetura 02.

[./arq03-cenario01](./arq03-cenario01) - Artefatos para executar o cenário 01 da arquitetura 03.

[./arq03-cenario02](./arq03-cenario02) - Artefatos para executar o cenário 02 da arquitetura 03.

[./bacen](./bacen) - Artefatos para executar a API Bacen.

[./docker](./docker) - Contém script para construir todas as imagens docker.

[./src](./src) - Diretório com o código fonte dos serviços usados nos cenários. A linguagem de programação usada foi C# com o framework dotnet8.

[./stress-test-gatling](./stress-test-gatling) - Contém o código fonte para execução do cenário de teste de stress usando a ferramenta Gatling.

[./stress-test-k6](./stress-test-k6) - Contém o código fonte para execução do cenário de teste de stress usando a ferramenta Grafana K6.

[diagramas.drawio](./diagramas.drawio) - Contém todos os diagramas usados nos [slides](./SECOMP-2024-slides.pdf). Pode ser aberto com o programa [draw.io](https://app.diagrams.net/) diretamente do browser.

[SECOMP-2024-slides.pdf](./SECOMP-2024-slides.pdf) - Slides da apresentação em formato PDF.

## Tecnologias e Ferramentas Usadas

https://www.docker.com/ - conteinerização

https://docs.docker.com/compose/ - ferramenta para execução de múltiplos contêineres integrados

https://nginx.org/ - balanceador de carga*

https://www.postgresql.org/ - banco de dados relacional

https://www.rabbitmq.com/ - message broker

https://dotnet.microsoft.com/ - plataforma de desenvolvimento

https://learn.microsoft.com/dotnet/csharp/ - linguagem de programação

https://code.visualstudio.com/ - editor de código multiplataforma

https://gatling.io/ - ferramenta de teste de performance

https://k6.io/ - ferramenta de teste de performance

https://app.diagrams.net/ - ferramenta para desenhar diagramas


\* *O nginx foi usado de proxy reverso e não para balanceamento de carga HTTP. O balanceamento de carga de fato foi feito através de uma técnica chamada [DNS Load Balancing](https://www.cloudflare.com/pt-br/learning/performance/what-is-dns-load-balancing/) usando o [DNS do docker](https://docs.docker.com/engine/network/#dns-services).*


## Contato / Redes Sociais

Se quiser se conectar ou entrar em contato comigo, me encontre nas redes sociais:

- https://github.com/zanfranceschi
- https://bsky.app/profile/zanfranceschi.bsky.social
- https://www.linkedin.com/in/francisco-zanfranceschi/
- https://www.youtube.com/@zanfranceschi
- https://www.instagram.com/zanfranceschi/
- https://dev.to/zanfranceschi
- https://twitter.com/zanfranceschi


### Rinha de Backend:
- https://bsky.app/profile/rinhadebackend.bsky.social
- https://twitter.com/rinhadebackend
- https://www.youtube.com/results?search_query=rinha+de+backend
- https://www.google.com/search?q=rinha+de+backend
