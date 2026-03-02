FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["HRHiringSystem.sln", "./"]
COPY ["HRHiringSystem.API/HRHiringSystem.API.csproj", "HRHiringSystem.API/"]
COPY ["HRHiringSystem.Application/HRHiringSystem.Application.csproj", "HRHiringSystem.Application/"]
COPY ["HRHiringSystem.Domain/HRHiringSystem.Domain.csproj", "HRHiringSystem.Domain/"]
COPY ["HRHiringSystem.Infrastructure/HRHiringSystem.Infrastructure.csproj", "HRHiringSystem.Infrastructure/"]

RUN dotnet restore "HRHiringSystem.API/HRHiringSystem.API.csproj"

COPY ["HRHiringSystem.API/", "HRHiringSystem.API/"]
COPY ["HRHiringSystem.Application/", "HRHiringSystem.Application/"]
COPY ["HRHiringSystem.Domain/", "HRHiringSystem.Domain/"]
COPY ["HRHiringSystem.Infrastructure/", "HRHiringSystem.Infrastructure/"]
WORKDIR "/src/HRHiringSystem.API"
RUN dotnet publish "HRHiringSystem.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "HRHiringSystem.API.dll"]