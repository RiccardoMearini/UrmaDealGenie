dotnet publish -c Release -o ./publish
Compress-Archive -Update ./publish/* UrmaDealGenie.zip
