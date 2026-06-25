# Build stage
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:10.0@sha256:ea8bde36c11b6e7eec2656d0e59101d4462f6bd630730f2c8201ed0572b295d5 AS build
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
FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:7644f992230d35cf230017189d4038c0ae0f7388b13f4f7ae1900a155bafb597
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "ical2s3grpc.dll"]