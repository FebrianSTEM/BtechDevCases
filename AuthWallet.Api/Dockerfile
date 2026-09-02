FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["AuthWallet.Api/AuthWallet.Api.csproj", "AuthWallet.Api/"]
COPY ["AuthWallet.Application/AuthWallet.Application.csproj", "AuthWallet.Application/"]
COPY ["AuthWallet.Domain/AuthWallet.Domain.csproj", "AuthWallet.Domain/"]
COPY ["AuthWallet.Infrastructure/AuthWallet.Infrastructure.csproj", "AuthWallet.Infrastructure/"]

RUN dotnet restore "AuthWallet.Api/AuthWallet.Api.csproj"

COPY . .

WORKDIR "/src/AuthWallet.Api"
RUN dotnet publish "AuthWallet.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "AuthWallet.Api.dll"]