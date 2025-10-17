# Build stage
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:10.0@sha256:ed46c853a4f100ba7aab2394e9fb9f9b8989e93346b04632dbce11e5da7623d8 AS build
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
FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:5b201bc57156a9f70d58a86112dc7a8217cf3a885740a8fe914b0782ee09647e
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "ical2s3grpc.dll"]