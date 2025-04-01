FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

WORKDIR /
COPY ./ .

RUN dotnet restore 
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT="production"
EXPOSE 8080

CMD ["dotnet", "Epsilon.dll"]
