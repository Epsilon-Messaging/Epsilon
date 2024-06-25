function push(){
  echo "Pushing peachesmlg/epsilon:$1"
  docker tag epsilon "peachesmlg/epsilon:$1"
  docker push "peachesmlg/epsilon:$1"
}

echo '--- :dotnet: dotnet deploy'
docker build -t epsilon .

push "1.0.$BUILDKITE_BUILD_NUMBER"
push "latest"