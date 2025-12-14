#!/bin/bash

# Build and push multi-architecture images for GameStore
# Usage: ./build-and-push.sh <registry> <tag>
# Example: ./build-and-push.sh docker.io/joechandler73 latest

REGISTRY=${1:-"docker.io/joechandler73"}
TAG=${2:-"latest"}

echo "Building images for registry: $REGISTRY with tag: $TAG"

# Create buildx builder if it doesn't exist
docker buildx create --name gamestore-builder --use 2>/dev/null || docker buildx use gamestore-builder

# Build and push GameStoreApi
echo "Building GameStoreApi..."
docker buildx build --platform linux/amd64,linux/arm64 \
  -t ${REGISTRY}/gamestoreapi:${TAG} \
  -f GameStoreApi/Dockerfile \
  --push \
  .

if [ $? -ne 0 ]; then
    echo "Failed to build GameStoreApi"
    exit 1
fi

echo "GameStoreApi built successfully!"

# Build and push GameStoreFrontEnd
echo "Building GameStoreFrontEnd..."
docker buildx build --platform linux/amd64,linux/arm64 \
  -t ${REGISTRY}/gamestorefrontend:${TAG} \
  -f GameStoreFrontEnd/Dockerfile \
  --push \
  .

if [ $? -ne 0 ]; then
    echo "Failed to build GameStoreFrontEnd"
    exit 1
fi

echo "GameStoreFrontEnd built successfully!"

echo ""
echo "All images built and pushed successfully!"
echo "API Image: ${REGISTRY}/gamestoreapi:${TAG}"
echo "Frontend Image: ${REGISTRY}/gamestorefrontend:${TAG}"
echo ""
echo "Next steps:"
echo "1. Update the image references in k8s/04-api.yaml and k8s/05-frontend.yaml"
echo "2. Deploy to k3s using: kubectl apply -f k8s/"
