FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

WORKDIR /app

COPY Build .

ENV ASPNETCORE_URLS=http://*:5301

ENTRYPOINT ["dotnet", "studo.dll"]