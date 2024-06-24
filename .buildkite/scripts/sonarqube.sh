dotnet tool restore
echo '--- :sonarqube: Running Sonarqube'

dotnet sonarscanner begin \
  /o:"epsilon-messaging" \
  /k:"Epsilon-Messaging_Epsilon" \
  /d:sonar.host.url="https://sonarcloud.io" \
  /d:sonar.token="$SONARQUBE_TOKEN"
  
  dotnet build
  
  dotnet sonarscanner end /d:sonar.token="$SONARQUBE_TOKEN"