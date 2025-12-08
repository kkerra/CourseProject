# Используем официальный образ .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Копируем файл проекта
COPY ["TelemarketingApp.WebApi.csproj", "."]

# 2. Восстанавливаем зависимости
RUN dotnet restore "TelemarketingApp.WebApi.csproj"

# 3. Копируем весь код проекта
COPY . .

# 4. Собираем проект
RUN dotnet build "TelemarketingApp.WebApi.csproj" -c Release -o /app/build

# 5. Публикуем проект
RUN dotnet publish "TelemarketingApp.WebApi.csproj" -c Release -o /app/publish

# 6. Используем образ рантайма для запуска
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# 7. Копируем собранное приложение
COPY --from=build /app/publish .

# 8. Открываем порты
EXPOSE 80
EXPOSE 443

# 9. Запускаем приложение
ENTRYPOINT ["dotnet", "TelemarketingApp.WebApi.dll"]