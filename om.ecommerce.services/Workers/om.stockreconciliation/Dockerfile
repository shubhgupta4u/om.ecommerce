#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["om.ecommerce.stockreconciliation/om.ecommerce.stockreconciliation.csproj", "om.ecommerce.stockreconciliation/"]
RUN dotnet restore "om.ecommerce.stockreconciliation/om.ecommerce.stockreconciliation.csproj"
COPY . .
WORKDIR "/src/om.ecommerce.stockreconciliation"
RUN dotnet build "om.ecommerce.stockreconciliation.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "om.ecommerce.stockreconciliation.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "om.ecommerce.stockreconciliation.dll"]