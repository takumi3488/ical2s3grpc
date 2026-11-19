# Build stage
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:10.0@sha256:bef136a371963aaa46ec97882845ef6f54dec15728bb4b128b1c2a400ffbc9b4 AS build
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
FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:12b1135f5f8b3c0a214f2a680ff2033dff4f684f445d6c697d9524ceeb303586
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "ical2s3grpc.dll"]