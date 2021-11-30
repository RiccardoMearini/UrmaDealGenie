# Prep nuget and install pre-reqs
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
dotnet tool install --global Amazon.Lambda.Tools
