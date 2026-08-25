# 1. Этап сборки (SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Кэшируем зависимости
COPY ["WebForApplications.csproj", "./"]
RUN dotnet restore

# Собираем и публикуем проект
COPY . .
RUN dotnet publish -c Release -o /app/publish

# 2. Финальный легкий образ для запуска (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# По умолчанию слушает порт 8080 внутри контейнера
EXPOSE 8080
ENTRYPOINT ["dotnet", "WebForApplications.dll"]