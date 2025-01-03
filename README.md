# Three Rings Picture Wall

This application takes user account infomation from 3 Rings and displays member's thumbnail images with their details in a picture wall. 
This was developed for Samaritans Luton Branch.

To run the application you will need to pass a 3 Rings API key and your email address to the docker container.

To build and dockerise you will also need a container repo.

## To Build the release version of the app

`dotnet publish -c Release`

Build the docker image, this create Mac image (as I am on mac)

`docker build -t three-rings-picturewall-image-arm-mac -f Dockerfile .`  

Build the docker image targeting intel 64 bit for linux on intel

`docker build -t three-rings-picturewall-image-linux-amd64 --platform linux/amd64 -f Dockerfile . `  

Build the docker image targeting raspberry Pi 64 bit for linux

`docker build -t three-rings-picturewall-image-linux-arm64-v8 --platform linux/arm64/v8 -f Dockerfile .  `

Build the docker image targeting raspberry Pi 32 bit for linux

`docker build -t three-rings-picturewall-image-linux-arm32-v7 --platform linux/arm/v7 -f Dockerfile .  `

Tag the images

`docker tag three-rings-picturewall-image-arm-mac your-docker-repo/three-rings-picturewall-image-arm-mac`

`docker tag three-rings-picturewall-image-linux-amd64 your-docker-repo/three-rings-picturewall-image-linux-amd64`

`docker tag three-rings-picturewall-image-linux-arm32-v7 your-docker-repo/three-rings-picturewall-image-linux-arm32-v7`  

`docker tag three-rings-picturewall-image-linux-arm64-v8 your-docker-repo/three-rings-picturewall-image-linux-arm64-v8`

Then push to the docker hub

`docker push your-docker-repo/three-rings-picturewall-image-linux-amd64`

docker push your-docker-repo/three-rings-picturewall-image-arm-mac`

docker push your-docker-repo/three-rings-picturewall-image-linux-arm32-v7`

docker push your-docker-repo/three-rings-picturewall-image-linux-arm64-v8`

To list images to check it exists

`docker images`

Create the docker container Mac

`docker create --name three-rings-picturewall three-rings-picturewall-image-arm-mac`

When running the container pass in the variables it needs ... 

`docker start three-rings-picturewall-image-arm-mac  -e ThreeRingsUrl='https://www.3r.org.uk/'
-e ThreeRingsApiKey='your-api-key' 
-e ContactEmail='your-email-address'
-p 8089:8080`

`docker run --name picwall docker.io/your-docker-repo/threeringspicturewall:latest
-e ThreeRingsUrl='https://www.3r.org.uk/'
-e ThreeRingsApiKey='your-api-key' 
-e ContactEmail='your-email-address'
-p 8089:8080`

List the running containers

`docker ps`

List the running containers and the containers

`docker ps -a`

