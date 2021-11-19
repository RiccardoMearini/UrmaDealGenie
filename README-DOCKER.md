# Build
1. Build docker container from source
```
docker build -t urma-deal-genie -f Dockerfile .
```
# Run docker container
1. Edit the docker.env file to add your APIKEY and SECRET
1. Run container with environment variable file
```
docker run --env-file=docker.env urma-deal-genie
```