# --- Этап сборки (build stage) ---
# Используем официальный образ SDK для .NET для сборки приложения.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем .sln и .csproj файлы
COPY ["SeaBattle.Backend.sln", "./"]
COPY ["SeaBattle.Backend.Domain/SeaBattle.Backend.Domain.csproj", "SeaBattle.Backend.Domain/"]
COPY ["SeaBattle.Backend.Application/SeaBattle.Backend.Application.csproj", "SeaBattle.Backend.Application/"]
COPY ["SeaBattle.Backend.Infrastructure/SeaBattle.Backend.Infrastructure.csproj", "SeaBattle.Backend.Infrastructure/"]
COPY ["SeaBattle.Backend.WebAPI/SeaBattle.Backend.WebAPI.csproj", "SeaBattle.Backend.WebAPI/"]

# Восстанавливаем зависимости. Это кэшируется, если проекты не менялись.
RUN dotnet restore "SeaBattle.Backend.sln"

# Копируем весь остальной исходный код
COPY . .
WORKDIR "/src/SeaBattle.Backend.WebAPI"
# Публикуем приложение в режиме Release
RUN dotnet publish "SeaBattle.Backend.WebAPI.csproj" -c Release -o /app/publish --no-restore

# --- Финальный образ (final image) ---
# Используем гораздо меньший образ ASP.NET Core Runtime для запуска приложения.
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Устанавливаем переменные окружения
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

WORKDIR /app
# Копируем опубликованные файлы из этапа 'build'
COPY --from=build /app/publish .

# Создаем пользователя без прав root
RUN adduser --system --uid 10001 --no-create-home appuser
# Передаем владение файлами этому пользователю
RUN chown -R appuser:appuser .
# Указываем, что приложение будет запускаться от имени этого пользователя
USER appuser

# Открываем порт
EXPOSE 8080

# Определяем точку входа
ENTRYPOINT ["dotnet", "SeaBattle.Backend.WebAPI.dll"]