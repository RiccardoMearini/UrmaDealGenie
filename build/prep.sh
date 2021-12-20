# Prep nuget and install pre-reqs
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
dotnet tool install --global Amazon.Lambda.Tools
dotnet tool update --global Amazon.Lambda.Tools
dotnet tool install -g Amazon.Lambda.TestTool-3.1
dotnet add package AWSSDK.S3
dotnet new -i Amazon.Lambda.Templates::*