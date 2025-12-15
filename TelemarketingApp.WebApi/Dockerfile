FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["TelemarketingApp.WebApi.csproj", "."]

RUN dotnet restore "TelemarketingApp.WebApi.csproj"

COPY . .

RUN dotnet build "TelemarketingApp.WebApi.csproj" -c Release -o /app/build

RUN dotnet publish "TelemarketingApp.WebApi.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "TelemarketingApp.WebApi.dll"]