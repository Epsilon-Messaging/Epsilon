FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

WORKDIR /app

COPY epsilon_publish.zip .

RUN apk add --update-cache unzip && \
    unzip epsilon_publish.zip && \
    cp -R epsilon_publish/. . && \
    rm -rf epsilon_publish/ && \
    rm epsilon_publish.zip 

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT="production"
EXPOSE 8080

CMD ["dotnet", "Epsilon.dll"]