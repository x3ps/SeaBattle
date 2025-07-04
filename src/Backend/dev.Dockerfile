# --- Этап сборки (build stage) - все еще нужен для начального восстановления зависимостей ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем .csproj и .sln файлы по отдельности для кэширования restore
COPY ["SeaBattle.Backend.sln", "./"]
COPY ["SeaBattle.Backend.Domain/SeaBattle.Backend.Domain.csproj", "SeaBattle.Backend.Domain/"]
COPY ["SeaBattle.Backend.Infrastructure/SeaBattle.Backend.Infrastructure.csproj", "SeaBattle.Backend.Infrastructure/"]
COPY ["SeaBattle.Backend.Application/SeaBattle.Backend.Application.csproj", "SeaBattle.Backend.Application/"]
COPY ["SeaBattle.Backend.WebAPI/SeaBattle.Backend.WebAPI.csproj", "SeaBattle.Backend.WebAPI/"]

# Восстанавливаем зависимости
RUN dotnet restore "SeaBattle.Backend.sln" --force-evaluate

# --- Этап разработки (development stage) ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS develop
WORKDIR /app

# Копируем восстановленные зависимости из этапа build
COPY --from=build /src /app

# Открытие порта
EXPOSE 8080

# Определение точки входа для запуска в режиме watch
# Это запустит приложение и будет автоматически перезагружать его при изменениях файлов
ENTRYPOINT ["dotnet", "watch", "--project", "SeaBattle.Backend.WebAPI/SeaBattle.Backend.WebAPI.csproj", "run", "--urls", "http://+:8080"]