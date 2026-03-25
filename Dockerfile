# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj và restore dependencies
COPY ["QL_NhaThuoc.csproj", "./"]
RUN dotnet restore "QL_NhaThuoc.csproj"

# Copy toàn bộ source code
COPY . .

# Build ứng dụng
RUN dotnet build "QL_NhaThuoc.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "QL_NhaThuoc.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published files
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables - Render will override PORT
ENV ASPNETCORE_ENVIRONMENT=Production

# Run application
ENTRYPOINT ["sh", "-c", "dotnet QL_NhaThuoc.dll --urls http://0.0.0.0:${PORT:-8080}"]
