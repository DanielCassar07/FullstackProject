# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0 AS build
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o out

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out ./

EXPOSE 10000

#  Bind to Render's PORT (or default to 10000 locally)
CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-10000} dotnet FullstackProject.dll"]
