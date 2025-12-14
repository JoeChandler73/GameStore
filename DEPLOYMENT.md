# GameStore Kubernetes Deployment Guide

This guide will help you deploy the GameStore application to your k3s cluster on Raspberry Pi.

## Prerequisites

- k3s cluster running on Raspberry Pi (ARM64)
- `kubectl` configured to connect to your k3s cluster
- Docker with buildx support on your development machine
- A container registry (Docker Hub, GitHub Container Registry, or private registry)

## Project Structure

```
GameStore/
├── GameStoreApi/
│   └── Dockerfile
├── GameStoreFrontEnd/
│   └── Dockerfile
├── k8s/
│   ├── 00-namespace.yaml
│   ├── 01-secrets.yaml
│   ├── 02-postgres.yaml
│   ├── 03-redis.yaml
│   ├── 04-api.yaml
│   └── 05-frontend.yaml
├── build-and-push.sh
└── deploy.sh
```

## Step 1: Set Up Your Project

1. Create the `k8s` directory in your project root:
```bash
mkdir -p k8s
```

2. Place all the Kubernetes YAML files (00-namespace.yaml through 05-frontend.yaml) in the `k8s` directory.

3. Place the Dockerfiles in their respective project directories:
   - `GameStoreApi/Dockerfile`
   - `GameStoreFrontEnd/Dockerfile`

4. Place the scripts (`build-and-push.sh` and `deploy.sh`) in the project root.

5. Make the scripts executable:
```bash
chmod +x build-and-push.sh deploy.sh
```

## Step 2: Configure Your Registry

Update the following files with your container registry information:

### In `k8s/04-api.yaml`:
```yaml
image: your-registry/gamestoreapi:latest
```

### In `k8s/05-frontend.yaml`:
```yaml
image: your-registry/gamestorefrontend:latest
```

Replace `your-registry` with:
- Docker Hub: `docker.io/yourusername`
- GitHub Container Registry: `ghcr.io/yourusername`
- Private registry: `your-registry.com:5000`

## Step 3: Update Database Credentials

**IMPORTANT**: Change the default password in `k8s/01-secrets.yaml`:

```yaml
stringData:
  POSTGRES_PASSWORD: YourSecurePasswordHere123!  # Change this!
```

Also update the password in:
- `k8s/04-api.yaml` in the `ConnectionStrings__DefaultConnection` environment variable

## Step 4: Build and Push Images

1. Log in to your container registry:
```bash
# Docker Hub
docker login

# GitHub Container Registry
echo $GITHUB_TOKEN | docker login ghcr.io -u USERNAME --password-stdin
```

2. Build and push the images:
```bash
./build-and-push.sh your-registry/username latest
```

This will build multi-architecture images (AMD64 and ARM64) and push them to your registry.

## Step 5: Configure kubectl for k3s

If not already configured, set up kubectl to connect to your k3s cluster:

```bash
# On your Raspberry Pi, get the kubeconfig
sudo cat /etc/rancher/k3s/k3s.yaml

# On your development machine, save it to ~/.kube/config
# Replace 127.0.0.1 with your Raspberry Pi's IP address
```

Test the connection:
```bash
kubectl get nodes
```

## Step 6: Deploy to k3s

Run the deployment script:
```bash
./deploy.sh gamestore
```

This script will:
1. Create the namespace
2. Deploy PostgreSQL with persistent storage
3. Deploy Redis with persistent storage
4. Deploy the API
5. Deploy the Frontend
6. Wait for all pods to be ready

## Step 7: Verify Deployment

Check the status of all resources:
```bash
kubectl get all -n gamestore
```

Check pod logs if there are issues:
```bash
kubectl logs -f deployment/gamestoreapi -n gamestore
kubectl logs -f deployment/gamestorefrontend -n gamestore
kubectl logs -f deployment/postgres -n gamestore
kubectl logs -f deployment/redis -n gamestore
```

## Step 8: Access the Application

The frontend is exposed via NodePort on port 30080:
```
http://<raspberry-pi-ip>:30080
```

## Troubleshooting

### Images not pulling
- Verify you're logged into your registry on the k3s node
- For private registries, create an image pull secret:
```bash
kubectl create secret docker-registry regcred \
  --docker-server=your-registry \
  --docker-username=your-username \
  --docker-password=your-password \
  -n gamestore
```
Then add to your deployments:
```yaml
spec:
  imagePullSecrets:
  - name: regcred
```

### Database connection issues
- Check PostgreSQL is running: `kubectl get pods -n gamestore`
- Verify connection string in API deployment
- Check API logs for connection errors

### API not accessible from Frontend
- Verify service names match in frontend environment variables
- Check that services are created: `kubectl get svc -n gamestore`

### Pods not starting on ARM64
- Ensure you built multi-arch images with buildx
- Verify the image manifest includes ARM64: `docker buildx imagetools inspect your-registry/gamestoreapi:latest`

## Updating the Application

To update after code changes:

1. Rebuild and push images:
```bash
./build-and-push.sh your-registry/username v1.1
```

2. Update the image tags in `k8s/04-api.yaml` and `k8s/05-frontend.yaml`

3. Apply the changes:
```bash
kubectl apply -f k8s/04-api.yaml -n gamestore
kubectl apply -f k8s/05-frontend.yaml -n gamestore
```

Or use rolling restart:
```bash
kubectl rollout restart deployment/gamestoreapi -n gamestore
kubectl rollout restart deployment/gamestorefrontend -n gamestore
```

## Cleaning Up

To remove the entire deployment:
```bash
kubectl delete namespace gamestore
```

## Additional Considerations

### Persistent Storage
- k3s uses `local-path` storage by default, storing data on the Raspberry Pi's SD card
- For better performance/durability, consider using an external USB drive
- Data persists across pod restarts but is tied to the node

### Resource Limits
- The YAML files include conservative resource limits suitable for Raspberry Pi
- Adjust based on your specific hardware and requirements

### SSL/TLS
- Currently configured for HTTP only
- For production, configure an Ingress controller with Let's Encrypt certificates

### Scaling
- API and Frontend are configured with 2 replicas
- PostgreSQL and Redis are single-instance (consider replication for production)

## Next Steps

- Set up Ingress for domain-based routing
- Configure automated backups for PostgreSQL
- Implement monitoring with Prometheus/Grafana
- Set up CI/CD pipeline for automated deployments
