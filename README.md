# Como Executar 

  Para todas as opções é necessário abrir o terminal na pasta raiz do projeto

## Opção 1
  Usa o Ollama da sua máquina, com GPU se houver e não baixa/instala nada extra

### Pré-requisitos: Docker executando e Ollama com o modelo baixo:
    ollama pull qwen2.5:3b
### Executar:
    docker compose build
    docker compose run --rm --no-deps -e OLLAMA_URL=http://host.docker.internal:11434 chat

## Opção 2
  Usa somente o Docker, o Docker baixa o modelo automaticamente, porém, a primeira execução, baixa cerca de 4GB (imagem do Ollama + modelo)
e pode levar alguns minutos e pode ser que seja um pouco mais lenta nas respostas

## Pré-requisito: Docker executando
    docker compose build
    docker compose run --rm chat

## Opção 3
  Sem Docker

### Pré-requisitos: .NET 10 SDK e Ollama com o modelo baixado (ollama pull qwen2.5:3b) e na raiz do projeto rode
    dotnet build
    dotnet run --project src/ChatClient


# Detalhes da escolha do modelo
    O llama3.2:3b, sugerido no enunciado, foi testado, mas chamava tools em perguntas conceituais e confundia as duas ferramentas. 
    O qwen2.5:3b, do mesmo tamanho e com velocidade parecida, teve tool calling mais confiável e melhor desempenho em português. 
    O modelo é configurável por variável de ambiente. ex: $env:OLLAMA_MODEL="llama3.2"
