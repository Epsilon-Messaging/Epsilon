FROM mcr.microsoft.com/dotnet/sdk:8.0

RUN apt-get update && \
    apt-get install -y zip && \
    apt-get clean;