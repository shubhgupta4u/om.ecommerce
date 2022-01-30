#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["om.ecommerce.account.api/om.ecommerce.account.api.csproj", "om.ecommerce.account.api/"]
RUN dotnet restore "om.ecommerce.account.api/om.ecommerce.account.api.csproj"
COPY . .
WORKDIR "/src/om.ecommerce.account.api"
RUN dotnet build "om.ecommerce.account.api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "om.ecommerce.account.api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "om.ecommerce.account.api.dll"]