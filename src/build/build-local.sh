#!/bin/bash
# Script to build the Docker image locally (bash version for Linux/macOS)

TAG="${1:-latest}"
REGISTRY="${REGISTRY:-ghcr.io}"
OWNER="${GITHUB_REPOSITORY_OWNER}"

# Determine the repository owner
if [ -z "$OWNER" ]; then
    GIT_REMOTE=$(git remote get-url origin 2>/dev/null || echo "")
    if [[ $GIT_REMOTE =~ github\.com[:/]([^/]+)/ ]]; then
     OWNER="${BASH_REMATCH[1]}"
    else
        OWNER="dr1rrb"  # Fallback
    fi
fi

IMAGE_NAME="$REGISTRY/$OWNER/wled-manager"
DOCKER_CONTEXT="./src/WledManager"
DOCKERFILE="$DOCKER_CONTEXT/Dockerfile"

echo "?? Building Docker image..."
echo "   Image: $IMAGE_NAME:$TAG"
echo "   Context: $DOCKER_CONTEXT"
echo "   Dockerfile: $DOCKERFILE"
echo ""

docker build \
    -t "$IMAGE_NAME:$TAG" \
    -f "$DOCKERFILE" \
    "$DOCKER_CONTEXT"

if [ $? -eq 0 ]; then
  echo ""
echo "? Build successful!"
    echo ""
    echo "To test the image:"
    echo "  docker run -p 8080:8080 $IMAGE_NAME:$TAG"
else
    echo ""
    echo "? Build failed!"
    exit 1
fi
