# Build stage
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:10.0@sha256:d1823fecac3689a2eb959e02ee3bfe1c2142392808240039097ad70644566190 AS build
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
FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:eaa79205c3ade4792a7f7bf310a3aac51fe0e1d91c44e40f70b7c6423d475fe0
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "ical2s3grpc.dll"]