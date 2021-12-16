FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
COPY . /source/

WORKDIR /source/LOIN.Server

# copy csproj and restore as distinct layers
#COPY *.csproj .
#COPY LOIN.Server/*.csproj .
RUN dotnet restore

#WORKDIR /source
# copy and publish app and libraries
#COPY . .
RUN dotnet publish -c release -o /app --no-restore

# final stage/image
#FROM mcr.microsoft.com/dotnet/runtime:3.1
FROM mcr.microsoft.com/dotnet/aspnet:3.1
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "LOIN.Server.dll"]
