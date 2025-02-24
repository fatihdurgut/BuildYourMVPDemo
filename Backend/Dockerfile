FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Create certificate directory
RUN mkdir -p /usr/local/share/ca-certificates/aspnet

# Generate development certificate
RUN dotnet dev-certs https -ep /usr/local/share/ca-certificates/aspnet/https.pfx -p testpassword123!
RUN update-ca-certificates

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Backend/Backend.csproj", "Backend/"]
RUN dotnet restore "Backend/Backend.csproj"
COPY . .
WORKDIR "/src/Backend"
RUN dotnet build "Backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=https://+:443;http://+:80
ENV ASPNETCORE_HTTPS_PORT=443
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/usr/local/share/ca-certificates/aspnet/https.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=testpassword123!

ENTRYPOINT ["dotnet", "Backend.dll"]