
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish Devoted.Gateway/Devoted.Gateway.csproj -c Release -o /out


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:8081

ENV ReverseProxy__Clusters__products-cluster__Destinations__productService__Address="http://devoted.api:8080/"
ENV ReverseProxy__Clusters__orders-cluster__Destinations__orderService__Address="http://devoted.api:8080/"
ENTRYPOINT ["dotnet","Devoted.Gateway.dll"]
