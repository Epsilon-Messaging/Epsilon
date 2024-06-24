echo '--- :dotnet::test_tube: dotnet test'
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat="opencover" --logger "console;verbosity=minimal" --filter "FullyQualifiedName!~Integration"