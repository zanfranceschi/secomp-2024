# Escalabilidade: Uma Demonstração Prática
## SECOMP XII / 2024

**Repositório com o código fonte e artefatos da apresentação para SECOMP XII realizada em 01/11/2024 na UFSCar.**

## Organização do Repositório

### Diretórios e Arquivos

[./bacen](./bacen) - Artefatos para executar a API Bacen.

[./design-01](./design-01) - Artefatos para executar o cenário 01.

[./design-02](./design-02) - Artefatos para executar o cenário 02.

[./design-03](./design-03) - Artefatos para executar o cenário 03.

[./design-04](./design-04) - Artefatos para executar o cenário 04.

[./design-05](./design-05) - Artefatos para executar o cenário 05.

[./design-06](./design-06) - Artefatos para executar o cenário extra 06.

[./docker](./docker) - Contém script para construir todas as imagens docker.

[./src](./src) - Diretório com o código fonte dos serviços usados nos cenários. A linguagem de programação usada foi C# com o framework dotnet8.

[./stress-test](./stress-test) - Contém o código fonte para execução do cenário de teste de stress.

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

https://workspace.google.com/products/slides/ - ferramenta usada para apresentação de slides

https://github.com/ - ferramenta para versionamento de arquivos através do git

https://app.diagrams.net/ - ferramenta para desenhar diagramas

https://nixos.org/ - sistema operacional usado na apresentação e confecção de todos os artefatos da apresentação


\* *Disclaimer: O nginx foi usado de proxy reverso e não para balanceamento de carga HTTP. O balanceamento de carga de fato foi feito através de uma técnica chamada [DNS Load Balancing](https://www.cloudflare.com/pt-br/learning/performance/what-is-dns-load-balancing/) usando o [DNS do docker](https://docs.docker.com/engine/network/#dns-services).*


## Contato / Redes Sociais

Se quiser se conectar ou entrar em contato comigo, me encontre nas redes sociais:

- https://github.com/zanfranceschi
- https://bsky.app/profile/zanfranceschi.bsky.social
- https://www.linkedin.com/in/francisco-zanfranceschi/
- https://www.youtube.com/@zanfranceschi
- https://www.instagram.com/zanfranceschi/
- https://dev.to/zanfranceschi
- https://twitter.com/zanfranceschi
