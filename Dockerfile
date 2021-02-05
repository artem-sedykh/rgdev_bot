FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS publish

ADD ./src /src
WORKDIR /src

# Restore
RUN dotnet restore RgDevBot.sln
RUN dotnet build RgDevBot.sln -c Release -o /app
RUN dotnet test RgDevBot.sln -c Release
RUN dotnet publish RgDevBot.sln -c Release -o /app

# Final image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "RgDevBot.dll"]
