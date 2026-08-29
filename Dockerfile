# Stage 1: Build the Vue 3 Frontend
FROM node:22-alpine AS frontend-build
WORKDIR /app
COPY cloudpulse-ui/package*.json ./
RUN npm install
COPY cloudpulse-ui/ ./
RUN npm run build

# Stage 2: Build the ASP.NET Core 8 Web API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src
COPY ["CloudPulse.Api/CloudPulse.Api.csproj", "CloudPulse.Api/"]
RUN dotnet restore "CloudPulse.Api/CloudPulse.Api.csproj"
COPY CloudPulse.Api/ CloudPulse.Api/
WORKDIR "/src/CloudPulse.Api"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final Production Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=backend-build /app/publish ./
# Copy compiled Vue SPA into the wwwroot directory of the .NET app
COPY --from=frontend-build /app/dist ./wwwroot

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "CloudPulse.Api.dll"]