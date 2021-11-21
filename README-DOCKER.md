# Run Urma Deal Genie in a Docker container
These Docker commands need [Docker Desktop](https://docs.docker.com/desktop/) and [docker-compose](https://docs.docker.com/compose/install/) installed with file share access to your local drive. The commands should be run in the same folder as the `docker.env` and `appsettings.json` and `docker-compose.yml` files.
1. Edit the docker.env file to add your APIKEY and SECRET
1. Run container with app settings and environment variable files
    ```
    docker run --rm -v $(pwd)/appsettings.json:/App/appsettings.json -v $(pwd)/dealrules.json:/App/dealrules.json --env-file=docker.env urmagurd/deal-genie:1.0
    ```
1. Run container with docker-compose
    ```
    docker-compose up
    ```

# Build
These steps are for building a Docker image and optionally pushing to a Docker Hub registry. This is only really needed for UrmaGurd to do.
1. Build Docker container from source
    ```
    docker build -t urmagurd/deal-genie -f Dockerfile .
    ```
1. Push image to Docker registry 
    ```
    docker push urmagurd/deal-genie:1.0
    ```
   Obviously you can only push to a registry for which you have access to.
1. Running with your own local docker.dev.env file (this is same as above run command but with .dev.env)
    ```
    docker run --rm -v $(pwd)/appsettings.json:/App/appsettings.json -v $(pwd)/dealrules.json:/App/dealrules.json --env-file=docker.dev.env urmagurd/deal-genie:1.0
    ```
