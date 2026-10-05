FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY bin/Release/net10.0/publish/ .
# The app runs as the image's non-root user: binaries stay root-owned (read-only to it),
# and only Logs/ — where Serilog writes — is writable.
RUN mkdir Logs && chown $APP_UID:$APP_UID Logs
USER $APP_UID
# A non-root user cannot bind ports below 1024, hence 8080 rather than 80.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "MyMechanicShop.HttpApi.Host.dll"]
