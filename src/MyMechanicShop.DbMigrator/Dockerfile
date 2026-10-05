FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY bin/Release/net10.0/publish/ .
# The app runs as the image's non-root user: binaries stay root-owned (read-only to it),
# and only Logs/ — where Serilog writes — is writable.
RUN mkdir Logs && chown $APP_UID:$APP_UID Logs
USER $APP_UID
ENTRYPOINT ["dotnet", "MyMechanicShop.DbMigrator.dll"]
