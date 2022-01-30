#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["om.ecommerce.payment.api/om.ecommerce.payment.api.csproj", "om.ecommerce.payment.api/"]
RUN dotnet restore "om.ecommerce.payment.api/om.ecommerce.payment.api.csproj"
COPY . .
WORKDIR "/src/om.ecommerce.payment.api"
RUN dotnet build "om.ecommerce.payment.api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "om.ecommerce.payment.api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "om.ecommerce.payment.api.dll"]