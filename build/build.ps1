# Prep nuget and install pre-reqs
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
dotnet tool install --global Amazon.Lambda.Tools
dotnet lambda package

dotnet restore ./src/UrmaDealGenie
dotnet publish ./src/UrmaDealGenie -c Release -o ./src/UrmaDealGenie/publish
Compress-Archive -Update ./src/UrmaDealGenie/publish/* UrmaDealGenie.zip

dotnet restore ./src/UrmaDealGenieApp
dotnet publish ./src/UrmaDealGenieApp -c Release -o ./src/UrmaDealGenieApp/publish
Compress-Archive -Update ./src/UrmaDealGenieApp/publish/* UrmaDealGenieApp.zip
