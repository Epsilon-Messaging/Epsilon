containers=$(docker-compose -f .buildkite/docker-compose-test.yml config --services)

for container in $containers; do
  if [ "$container" = "dotnet" ]; then
    continue
  fi
  echo "--- ${container} Service Logs"
  docker-compose -f .buildkite/docker-compose-test.yml logs --no-log-prefix "${container}"
done

exit 1
