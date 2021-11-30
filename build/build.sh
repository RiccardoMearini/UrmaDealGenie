# Run this with: source ./build/build.sh

export ver=beta-2.1

# Build AWS Package
dotnet restore ./src/UrmaDealGenie
dotnet lambda package UrmaDealGenieAWS-${ver}.zip -pl ./src/UrmaDealGenie

# Build .NET Core Application Package
dotnet restore ./src/UrmaDealGenieApp
dotnet publish ./src/UrmaDealGenieApp -c Release -o ./UrmaDealGenieApp
zip UrmaDealGenieApp-${ver}.zip ./UrmaDealGenieApp/*
rm -r ./UrmaDealGenieApp