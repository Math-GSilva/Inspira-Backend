# Estágio 1: Build da Aplicação
# Este estágio usa o SDK completo para compilar seu código e prepará-lo para publicação.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Instala a ferramenta EF Core e a adiciona ao PATH do sistema.
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copia os arquivos de projeto (.csproj) e restaura as dependências.
# Fazer isso em um passo separado otimiza o cache do Docker.
COPY ["inspira-backend.Api/inspira-backend.Api.csproj", "inspira-backend.Api/"]
COPY ["inspira-backend.Application/inspira-backend.Application.csproj", "inspira-backend.Application/"]
COPY ["inspira-backend.Domain/inspira-backend.Domain.csproj", "inspira-backend.Domain/"]
COPY ["inspira-backend.Infra/inspira-backend.Infra.csproj", "inspira-backend.Infra/"]
RUN dotnet restore "inspira-backend.Api/inspira-backend.Api.csproj"

# Copia o restante do código-fonte para o contêiner.
COPY . .

# Define o diretório de trabalho para o projeto da API e publica a aplicação.
WORKDIR "/src/inspira-backend.Api"
RUN dotnet publish "inspira-backend.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# --- FIM DO ESTÁGIO DE BUILD ---


# Estágio 2: Imagem Final
# Esta será a imagem que de fato irá rodar no seu ambiente.
# Usamos a imagem do SDK para que as ferramentas de desenvolvimento (como 'dotnet ef') estejam disponíveis.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app

# Instala a ferramenta EF Core e a adiciona ao PATH na imagem final também.
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copia os arquivos publicados (a aplicação pronta para rodar) do estágio de build.
COPY --from=build /app/publish .

# --- SOLUÇÃO AQUI ---
# Copia TODO o código-fonte do estágio de build para a imagem final.
# Isso permite que os comandos 'dotnet ef' encontrem os arquivos .csproj e o DbContext.
# As pastas (inspira-backend.Api, etc.) serão criadas dentro do diretório /app.
COPY --from=build /src .

# Expõe a porta que a aplicação irá escutar.
EXPOSE 8080

# Define o comando padrão para iniciar a API quando o contêiner for iniciado.
ENTRYPOINT ["dotnet", "inspira-backend.Api.dll"]