FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY bin/Release/net10.0/publish/ .

RUN mkdir Logs && chown $APP_UID:$APP_UID Logs
USER $APP_UID
ENTRYPOINT ["dotnet", "MyMechanicShop.DbMigrator.dll"]
