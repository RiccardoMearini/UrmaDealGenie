# Restore all project dependencies
dotnet restore

# Build AWS Package
dotnet lambda package UrmaDealGenieAWS-localbuild.zip -pl ./src/UrmaDealGenie
# see ./deploy/deploy-urmadealgenie.sh for how to deploy with AWS CLI

# Build .NET Core Application Packages (Win & Mac & Linux)
dotnet publish ./src/UrmaDealGenieApp -c Release \
  -o ./src/UrmaDealGenieApp/UrmaDealGenieApp-win10-x64 \
  -p:PublishSingleFile=true --self-contained false \
  -r win10-x64

dotnet publish ./src/UrmaDealGenieApp -c Release \
  -o ./src/UrmaDealGenieApp/UrmaDealGenieApp-osx-x64 \
  -p:PublishSingleFile=true --self-contained false \
  -r osx-x64

mv ./src/UrmaDealGenieApp/UrmaDealGenieApp-osx-x64/UrmaDealGenieApp \
   ./src/UrmaDealGenieApp/UrmaDealGenieApp-osx-x64/UrmaDealGenieApp-osx-x64

dotnet publish ./src/UrmaDealGenieApp -c Release \
  -o ./src/UrmaDealGenieApp/UrmaDealGenieApp-linux-x64 \
  -p:PublishSingleFile=true --self-contained false \
  -r linux-x64  

mv ./src/UrmaDealGenieApp/UrmaDealGenieApp-linux-x64/UrmaDealGenieApp \
   ./src/UrmaDealGenieApp/UrmaDealGenieApp-linux-x64/UrmaDealGenieApp-linux-x64


# Build docker image and push to registry
cd ./src/UrmaDealGenieApp
docker build -t urmagurd/deal-genie:${ver} -f Dockerfile .
docker push urmagurd/deal-genie:${ver}

cd ../..

# Remove temporary app publish folder
rm -r ./src/UrmaDealGenieApp/UrmaDealGenieApp
