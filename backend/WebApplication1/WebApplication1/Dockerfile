# 1. Dùng Image SDK để build code
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file csproj và restore thư viện
COPY ["WebApplication1.csproj", "./"]
RUN dotnet restore "WebApplication1.csproj"

# Copy toàn bộ code còn lại
COPY . .

# Build ra file DLL (Release)
RUN dotnet publish "WebApplication1.csproj" -c Release -o /app/publish

# 2. Dùng Image Runtime để chạy (nhẹ hơn)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .

# Chạy file DLL chính của bạn
ENTRYPOINT ["dotnet", "WebApplication1.dll"]