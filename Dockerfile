```dockerfile id="jlwmzx"
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore "KiranaStoreUI/KiranaStoreUI.csproj"

RUN dotnet publish "KiranaStoreUI/KiranaStoreUI.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "KiranaStoreUI.dll"]
```
