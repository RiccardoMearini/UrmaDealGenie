# Build image
1. Build docker container from source
```
docker build -t urma-deal-genie -f Dockerfile .
```

# Push image to Docker registry 
```
docker push ghcr.io/UrmaGurd/urma-deal-genie:1.0
```

# Run container from image
1. Edit the docker.env file to add your APIKEY and SECRET
1. Run container with environment variable file
```
docker run --env-file=docker.env urma-deal-genie
```
