# Prep nuget and install pre-reqs
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
dotnet tool install --global Amazon.Lambda.Tools

dotnet restore ../UrmaDealGenie
dotnet publish ../UrmaDealGenie -c Release -o ../UrmaDealGenie/publish
Compress-Archive -Update ../UrmaDealGenie/publish/* UrmaDealGenie.zip

dotnet restore ../UrmaDealGenieApp
dotnet publish ../UrmaDealGenieApp -c Release -o ../UrmaDealGenieApp/publish
Compress-Archive -Update ../UrmaDealGenieApp/publish/* UrmaDealGenieApp.zip
