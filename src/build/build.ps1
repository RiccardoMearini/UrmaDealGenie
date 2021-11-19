dotnet publish ../UrmaDealGenie -c Release -o ../UrmaDealGenie/publish
Compress-Archive -Update ../UrmaDealGenie/publish/* UrmaDealGenie.zip

dotnet publish ../UrmaDealGenieApp -c Release -o ../UrmaDealGenieApp/publish