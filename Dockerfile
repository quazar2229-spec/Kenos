# ── Этап 1: сборка ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY KENOS_Bot.csproj ./
RUN dotnet restore KENOS_Bot.csproj

COPY . .
RUN dotnet publish KENOS_Bot.csproj -c Release -o /out --no-restore

# ── Этап 2: рантайм ─────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /out .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
ENV URLS=http://0.0.0.0:5000

EXPOSE 5000
ENTRYPOINT ["dotnet", "KENOS.Bot.dll"]
