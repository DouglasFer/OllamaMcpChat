FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /src

COPY src/McpServer/McpServer.csproj src/McpServer/
COPY src/ChatClient/ChatClient.csproj src/ChatClient/
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore src/McpServer/McpServer.csproj -a $TARGETARCH \
 && dotnet restore src/ChatClient/ChatClient.csproj -a $TARGETARCH

COPY src/ src/
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish src/McpServer/McpServer.csproj -c Release -a $TARGETARCH --self-contained false -p:PublishSingleFile=false --no-restore -o /app/server \
 && dotnet publish src/ChatClient/ChatClient.csproj -c Release -a $TARGETARCH --self-contained false --no-restore -o /app/chat

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app ./

ENV MCP_SERVER_DLL=/app/server/McpServer.dll
ENTRYPOINT ["dotnet", "/app/chat/ChatClient.dll"]