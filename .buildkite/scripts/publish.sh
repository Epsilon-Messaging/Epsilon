echo '--- :dotnet: dotnet publish'
dotnet publish Epsilon -c Release -o epsilon_publish
zip -r epsilon_publish.zip epsilon_publish