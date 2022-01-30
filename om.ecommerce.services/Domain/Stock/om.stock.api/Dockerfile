#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["om.ecommerce.stock.api/om.ecommerce.stock.api.csproj", "om.ecommerce.stock.api/"]
RUN dotnet restore "om.ecommerce.stock.api/om.ecommerce.stock.api.csproj"
COPY . .
WORKDIR "/src/om.ecommerce.stock.api"
RUN dotnet build "om.ecommerce.stock.api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "om.ecommerce.stock.api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "om.ecommerce.stock.api.dll"]