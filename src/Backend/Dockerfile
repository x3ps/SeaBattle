# --- Этап сборки (build stage) ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем .sln и .csproj файлы
COPY ["SeaBattle.Backend.sln", "./"]
COPY ["SeaBattle.Backend.Domain/SeaBattle.Backend.Domain.csproj", "SeaBattle.Backend.Domain/"]
COPY ["SeaBattle.Backend.Application/SeaBattle.Backend.Application.csproj", "SeaBattle.Backend.Application/"]
COPY ["SeaBattle.Backend.Infrastructure/SeaBattle.Backend.Infrastructure.csproj", "SeaBattle.Backend.Infrastructure/"]
COPY ["SeaBattle.Backend.WebAPI/SeaBattle.Backend.WebAPI.csproj", "SeaBattle.Backend.WebAPI/"]

# Восстанавливаем зависимости для всего решения.
RUN dotnet restore "SeaBattle.Backend.sln"

# Копируем весь остальной исходный код
COPY . .

WORKDIR "/src/SeaBattle.Backend.WebAPI"

# Публикуем приложение в режиме Release
RUN dotnet publish "SeaBattle.Backend.WebAPI.csproj" -c Release -o /app/publish --no-restore

# --- Финальный образ (final image) ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

WORKDIR /app
COPY --from=build /app/publish .

RUN adduser --system --uid 10001 --no-create-home appuser && \
    chown -R appuser:nogroup .

USER appuser

EXPOSE 8080

ENTRYPOINT ["dotnet", "SeaBattle.Backend.WebAPI.dll"]