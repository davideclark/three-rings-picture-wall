#Build the release version of the app
dotnet publish -c Release

#Build the docker image, this create Mac image (as I am on mac)
docker build -t three-rings-picturewall-image-arm-mac -f Dockerfile .  

#Build the docker image targeting intel 64 bit for linux on intel
docker build -t three-rings-picturewall-image-linux-amd64 --platform linux/amd64 -f Dockerfile .   

#Build the docker image targeting raspberry Pi 64 bit for linux
docker build -t three-rings-picturewall-image-linux-arm64-v8 --platform linux/arm64/v8 -f Dockerfile .  

#Build the docker image targeting raspberry Pi 32 bit for linux
docker build -t three-rings-picturewall-image-linux-arm32-v7 --platform linux/arm/v7 -f Dockerfile .  

#tag the images
docker tag three-rings-picturewall-image-arm-mac davideclark/three-rings-picturewall-image-arm-mac      
docker tag three-rings-picturewall-image-linux-amd64 davideclark/three-rings-picturewall-image-linux-amd64    
docker tag three-rings-picturewall-image-linux-arm32-v7 davideclark/three-rings-picturewall-image-linux-arm32-v7  

docker tag three-rings-picturewall-image-linux-arm64-v8 davideclark/three-rings-picturewall-image-linux-arm64-v8

# Then push to the docker hub
docker push davideclark/three-rings-picturewall-image-linux-amd64 
docker push davideclark/three-rings-picturewall-image-arm-mac 
docker push davideclark/three-rings-picturewall-image-linux-arm32-v7 
docker push davideclark/three-rings-picturewall-image-linux-arm64-v8