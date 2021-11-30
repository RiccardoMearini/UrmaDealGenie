# Set 'ver' env variable, and run this command with from solution root:
#   source ./build/build.sh

# Build AWS Package
dotnet restore ./src/UrmaDealGenie
dotnet lambda package UrmaDealGenieAWS-${ver}.zip -pl ./src/UrmaDealGenie

# Build .NET Core Application Package
dotnet restore ./src/UrmaDealGenieApp
dotnet publish ./src/UrmaDealGenieApp -c Release -o ./src/UrmaDealGenieApp/UrmaDealGenieApp
cd  ./src/UrmaDealGenieApp
zip ../../UrmaDealGenieApp-${ver}.zip ./UrmaDealGenieApp/*

# Build docker image and push to registry
docker build -t urmagurd/deal-genie:${ver} -f Dockerfile .
docker push urmagurd/deal-genie:${ver}

cd ../..

# Remove temporary app publish folder
rm -r ./src/UrmaDealGenieApp/UrmaDealGenieApp
