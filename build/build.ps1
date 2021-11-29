# Prep nuget and install pre-reqs
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
dotnet tool install --global Amazon.Lambda.Tools

dotnet restore ./src/UrmaDealGenie
dotnet lambda package UrmaDealGenieAWS-beta-2.0.zip -pl ./src/UrmaDealGenie

dotnet restore ./src/UrmaDealGenieApp
dotnet publish ./src/UrmaDealGenieApp -c Release -o ./src/UrmaDealGenieApp/publish
zip UrmaDealGenieApp-beta-2.0.zip ./src/UrmaDealGenieApp/publish/

# Windows zip app
Compress-Archive -Update ./src/UrmaDealGenieApp/publish/* UrmaDealGenieApp.zip

