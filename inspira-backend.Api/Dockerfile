FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

COPY ["inspira-backend.Api/inspira-backend.Api.csproj", "inspira-backend.Api/"]
COPY ["inspira-backend.Application/inspira-backend.Application.csproj", "inspira-backend.Application/"]
COPY ["inspira-backend.Domain/inspira-backend.Domain.csproj", "inspira-backend.Domain/"]
COPY ["inspira-backend.Infra/inspira-backend.Infra.csproj", "inspira-backend.Infra/"]
RUN dotnet restore "inspira-backend.Api/inspira-backend.Api.csproj"

COPY . .

WORKDIR "/src/inspira-backend.Api"
RUN dotnet publish "inspira-backend.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app

RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

COPY --from=build /app/publish .

COPY --from=build /src .

EXPOSE 8080

ENTRYPOINT ["dotnet", "inspira-backend.Api.dll"]