FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install Node.js
RUN apt-get update && apt-get install -y curl gnupg
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash -
RUN apt-get install -y nodejs

# Create certificate directory
RUN mkdir -p /usr/local/share/ca-certificates/aspnet

# Generate development certificate
RUN dotnet dev-certs https -ep /usr/local/share/ca-certificates/aspnet/https.pfx -p testpassword123!
RUN update-ca-certificates

FROM base AS build
WORKDIR /src
COPY ["Frontend/Frontend.csproj", "Frontend/"]
RUN dotnet restore "Frontend/Frontend.csproj"
COPY . .
WORKDIR "/src/Frontend"

# Install npm packages
COPY package*.json ./
RUN npm install

RUN dotnet build "Frontend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Frontend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy node_modules
COPY --from=build /src/Frontend/node_modules ./node_modules

ENV ASPNETCORE_URLS=https://+:443;http://+:80
ENV ASPNETCORE_HTTPS_PORT=443
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/usr/local/share/ca-certificates/aspnet/https.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=testpassword123!

ENTRYPOINT ["dotnet", "Frontend.dll"]