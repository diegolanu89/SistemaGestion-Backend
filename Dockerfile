# ── Etapa 1: build ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["bdt_evm_app.csproj", "."]
RUN dotnet restore "./bdt_evm_app.csproj"

COPY . .
RUN dotnet publish "./bdt_evm_app.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ── Etapa 2: runtime ─────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5000

ENTRYPOINT ["dotnet", "bdt_evm_app.dll"]
