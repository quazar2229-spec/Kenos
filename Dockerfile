# ── Этап 1: сборка ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Копируем только .csproj сначала — кэш слоя восстановления пакетов
COPY KENOS_Bot.csproj ./
RUN dotnet restore KENOS_Bot.csproj

# Копируем весь исходник и публикуем явно с именем проекта
COPY . .
RUN dotnet publish KENOS_Bot.csproj -c Release -o /out --no-restore

# ── Этап 2: рантайм — минимальный образ ─────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Только артефакты сборки — без SDK
COPY --from=build /out .

# Переменные окружения
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

EXPOSE 5000
ENV URLS=http://0.0.0.0:5000

ENTRYPOINT ["./KENOS.Bot"]
