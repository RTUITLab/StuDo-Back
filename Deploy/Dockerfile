FROM mcr.microsoft.com/dotnet/core/aspnet:2.2

WORKDIR /app

COPY StuDo-Back/stu-do-back-build .
COPY StuDo-Front/stu-do-front-build ./wwwroot/

ENTRYPOINT ["dotnet", "studo.dll"]