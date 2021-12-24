# Run Urma Deal Genie in a Docker container
These Docker commands need [Docker Desktop](https://docs.docker.com/desktop/) and [docker-compose](https://docs.docker.com/compose/install/) installed with file share access to your local drive. The commands should be run in the same folder as the `docker.env` and `appsettings.json` and `docker-compose.yml` files.
1. Edit the docker.env file to add your [APIKEY and SECRET](/README.md#create-a-3commas-api-key-and-secret)
1. Run container with app settings and environment variable files
    - **Mac/Linux**
    ```
    docker run --rm -v $(pwd)/appsettings.json:/App/appsettings.json -v $(pwd)/dealrules.json:/App/dealrules.json --env-file=docker.env urmagurd/deal-genie:2.3
    ```
    - **Windows** (not tested, so this might not be right) - substituting _yourpath_ in the 2 places below with the path to your `appsettings.json`, `dealrules.json` and `docker.env` files.    
    ```
    docker run --rm -v c:\yourpath\appsettings.json:c:\App\appsettings.json c:\yourpath\dealrules.json:c:\App\dealrules.json --env-file=docker.env urmagurd/deal-genie:2.3
    ```
  
1. OR, simply run container with docker-compose, much easier
    ```
    docker-compose up
    ```

# Build Docker image
These steps are for building a Docker image and optionally pushing to a Docker Hub registry. This is only really needed for UrmaGurd to do.
1. Build Linux build of UrmaDealGenie (or download and unzip)
    ```
    dotnet publish ./src/UrmaDealGenieApp -c Release \
    -o ./src/UrmaDealGenieApp/UrmaDealGenieApp-linux-x64 \
    -p:PublishSingleFile=true --self-contained false \
    -r linux-x64  
    mv ./src/UrmaDealGenieApp/UrmaDealGenieApp-osx-x64/UrmaDealGenieApp \
       ./src/UrmaDealGenieApp/UrmaDealGenieApp-osx-x64/UrmaDealGenieApp-osx-x64
    ```

1. Build Docker container from source
    ```
    cd ./src/UrmaDealGenieApp
    docker build -t urmagurd/deal-genie -f Dockerfile .
    ```
1. Push image to Docker registry 
    ```
    docker push urmagurd/deal-genie:2.3
    ```
   Obviously you can only push to a registry for which you have access to.
1. Running with your own local docker.dev.env file (this is same as above run command but with .dev.env)
    ```
    docker run --rm -v $(pwd)/appsettings.json:/App/appsettings.json -v $(pwd)/dealrules.json:/App/dealrules.json --env-file=docker.dev.env urmagurd/deal-genie:2.3
    ```
