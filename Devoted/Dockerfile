
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish Devoted.API/Devoted.API.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__PostgresConnection="Host=postgres;Port=5432;Database=devoted;Username=postgres;Password=postgres"
ENTRYPOINT ["dotnet","Devoted.API.dll"]
