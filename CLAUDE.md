# Three Rings Picture Wall — Claude Code Notes

## Project
ASP.NET Core 8 web app that displays a picture wall of member thumbnails from the Three Rings volunteer management system. Developed for Samaritans Luton Branch.

## Deployment

Docker images are published to Docker Hub under `davideclark/three-rings-picturewall`.

### GitHub Actions CI/CD
Releasing is done via git version tags. Pushing a tag triggers `.github/workflows/docker-publish.yml`, which builds a multi-platform Docker image and pushes it to Docker Hub.

To release a new version:
```bash
git tag v1.x.x
git push origin v1.x.x
```

This produces the following tags on Docker Hub:
- `latest`
- `1.x.x`
- `1.x`

### Platforms built
- `linux/amd64` — Intel 64-bit Linux
- `linux/arm64` — Raspberry Pi 64-bit (also works on Apple Silicon Mac via Docker Desktop)
- `linux/arm/v7` — Raspberry Pi 32-bit

### GitHub Secrets required
- `DOCKERHUB_USERNAME` — Docker Hub username (`davideclark`)
- `DOCKERHUB_TOKEN` — Docker Hub personal access token (Read & Write permissions)

### Running the container
```bash
docker run --name picwall davideclark/three-rings-picturewall:latest \
  -e ThreeRingsUrl='https://www.3r.org.uk/' \
  -e ThreeRingsApiKey='your-api-key' \
  -e ContactEmail='your-email-address' \
  -p 8089:8080
```
