dotnet tool restore
echo '--- :sonarqube: Running Sonarqube'

if [[ "${BUILDKITE_PULL_REQUEST}" == "false" ]]; then
  dotnet sonarscanner begin \
    /o:"epsilon-messaging" \
    /k:"Epsilon-Messaging_Epsilon" \
    /d:sonar.host.url="https://sonarcloud.io" \
    /d:sonar.token="$SONARQUBE_TOKEN" \
    /d:sonar.pullrequest.branch="${BUILDKITE_BRANCH}" \
else
  dotnet sonarscanner begin \
    /o:"epsilon-messaging" \
    /k:"Epsilon-Messaging_Epsilon" \
    /d:sonar.host.url="https://sonarcloud.io" \
    /d:sonar.token="$SONARQUBE_TOKEN" \
    /d:sonar.pullrequest.branch="${BUILDKITE_BRANCH}" \
    /d:sonar.pullrequest.key="${BUILDKITE_PULL_REQUEST}" \
    /d:sonar.pullrequest.base="${BUILDKITE_PULL_REQUEST_BASE_BRANCH}"
fi
  
dotnet build

dotnet sonarscanner end /d:sonar.token="$SONARQUBE_TOKEN"
