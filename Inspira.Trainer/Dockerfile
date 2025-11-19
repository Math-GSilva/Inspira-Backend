# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Inspira.Trainer/Inspira.Trainer.csproj", "Inspira.Trainer/"]
COPY ["inspira-backend.Infra/inspira-backend.Infra.csproj", "inspira-backend.Infra/"]
COPY ["inspira-backend.Domain/inspira-backend.Domain.csproj", "inspira-backend.Domain/"]

RUN dotnet restore "Inspira.Trainer/Inspira.Trainer.csproj"

COPY . .
WORKDIR "/src/Inspira.Trainer"
RUN dotnet publish "Inspira.Trainer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0
WORKDIR /home/site/wwwroot
COPY --from=build /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true