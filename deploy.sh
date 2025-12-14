#!/bin/bash

# Deploy GameStore to k3s cluster
# Usage: ./deploy.sh [namespace]

NAMESPACE=${1:-"gamestore"}
K8S_DIR="k8s"

echo "Deploying GameStore to namespace: $NAMESPACE"

# Check if kubectl is available
if ! command -v kubectl &> /dev/null; then
    echo "kubectl not found. Please install kubectl first."
    exit 1
fi

# Check if k8s directory exists
if [ ! -d "$K8S_DIR" ]; then
    echo "k8s directory not found. Please create it and add your YAML files."
    exit 1
fi

# Create namespace if it doesn't exist
echo "Creating namespace..."
kubectl create namespace $NAMESPACE 2>/dev/null || echo "Namespace already exists"

# Apply all manifests in order
echo "Deploying secrets and config..."
kubectl apply -f ${K8S_DIR}/01-secrets.yaml -n $NAMESPACE

echo "Deploying PostgreSQL..."
kubectl apply -f ${K8S_DIR}/02-postgres.yaml -n $NAMESPACE

echo "Deploying Redis..."
kubectl apply -f ${K8S_DIR}/03-redis.yaml -n $NAMESPACE

echo "Waiting for databases to be ready..."
kubectl wait --for=condition=ready pod -l app=postgres -n $NAMESPACE --timeout=120s
kubectl wait --for=condition=ready pod -l app=redis -n $NAMESPACE --timeout=120s

echo "Deploying API..."
kubectl apply -f ${K8S_DIR}/04-api.yaml -n $NAMESPACE

echo "Waiting for API to be ready..."
kubectl wait --for=condition=ready pod -l app=gamestoreapi -n $NAMESPACE --timeout=120s

echo "Deploying Frontend..."
kubectl apply -f ${K8S_DIR}/05-frontend.yaml -n $NAMESPACE

echo "Waiting for Frontend to be ready..."
kubectl wait --for=condition=ready pod -l app=gamestorefrontend -n $NAMESPACE --timeout=120s

echo ""
echo "Deployment complete!"
echo ""
echo "Check status with:"
echo "  kubectl get all -n $NAMESPACE"
echo ""
echo "Access the frontend at:"
echo "  http://<raspberry-pi-ip>:30080"
echo ""
echo "View logs with:"
echo "  kubectl logs -f deployment/gamestorefrontend -n $NAMESPACE"
echo "  kubectl logs -f deployment/gamestoreapi -n $NAMESPACE"
