# Build stage
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:10.0@sha256:127d7d4d601ae26b8e04c54efb37e9ce8766931bded0ee59fcd799afd21d6850 AS build
WORKDIR /source

# Copy csproj and restore dependencies
COPY *.csproj .
RUN dotnet restore

# Copy source code  
COPY Program.cs .
COPY Protos/ ./Protos/
COPY Infrastructure/ ./Infrastructure/
COPY Services/ ./Services/
COPY Models/ ./Models/
COPY appsettings*.json ./

# Build and publish
RUN dotnet publish -c Release -o /app --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:c433886fdfe33c6427966a412328867b2be9a64f540a105d08943c2dc6fba39b
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "ical2s3grpc.dll"]