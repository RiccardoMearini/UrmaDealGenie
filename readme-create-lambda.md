# Create new .NET AWS Lambda Function
This is here for reference if you want to create your own .NET Lambda function that using .NET AWS tools.

## Install .NET Lambda Tools
[Amazon lambda tools and templates](https://www.youtube.com/watch?v=GZ8_anxgpK8&list=PLXCqSX1D2fd9rVux4a5y6krZQQOwMcWz2&index=3).
```
dotnet tool install --global Amazon.Lambda.Tools
dotnet tool update --global Amazon.Lambda.Tools
dotnet tool install -g Amazon.Lambda.TestTool-3.1
dotnet new -i Amazon.Lambda.Templates::*
```
## Create .NET lambda function
```
dotnet new gitignore
dotnet new lambda.EmptyFunction --name UrmaAutoDealUpdater --profile urmagurd --region eu-west-1
cd ./UrmaAutoDealUpdater/src/UrmaAutoDealUpdater
dotnet restore
```

## Add XCommas 3rd party package
```
dotnet add package XCommas.Net
```
