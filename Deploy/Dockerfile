FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app
COPY StuDo-Back/stu-do-back-build .

FROM rtuitlab/studo-front-build AS front
COPY --from=front /usr/share/nginx/html ./wwwroot/

ENTRYPOINT ["dotnet", "studo.dll"]