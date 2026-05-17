#!/bin/bash

# Oracle Cloud Deployment Script
# Run this on your Oracle Cloud VM

set -e

echo "🚀 Starting Kirana Store Deployment on Oracle Cloud..."

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
DB_USER="kirana"
DB_PASSWORD="${DB_PASSWORD:-KiranaStorePassword123}"
DB_NAME="kirana_store"
API_PORT=6000
UI_PORT=7000
GITHUB_REPO="${GITHUB_REPO:-https://github.com/YOUR-USERNAME/kirana-store.git}"

echo -e "${BLUE}📝 Configuration:${NC}"
echo "   Database User: $DB_USER"
echo "   Database Name: $DB_NAME"
echo "   API Port: $API_PORT"
echo "   UI Port: $UI_PORT"
echo "   GitHub Repo: $GITHUB_REPO"
echo ""

# Step 1: Update system
echo -e "${BLUE}Step 1/7: Updating system packages...${NC}"
sudo apt update && sudo apt upgrade -y

# Step 2: Install Docker
echo -e "${BLUE}Step 2/7: Installing Docker...${NC}"
if ! command -v docker &> /dev/null; then
    curl -fsSL https://get.docker.com -o get-docker.sh
    sudo sh get-docker.sh
    sudo usermod -aG docker ubuntu
    rm get-docker.sh
    echo -e "${GREEN}✓ Docker installed${NC}"
else
    echo -e "${GREEN}✓ Docker already installed${NC}"
fi

# Step 3: Install Docker Compose
echo -e "${BLUE}Step 3/7: Installing Docker Compose...${NC}"
if ! command -v docker-compose &> /dev/null; then
    sudo apt install -y docker-compose
    echo -e "${GREEN}✓ Docker Compose installed${NC}"
else
    echo -e "${GREEN}✓ Docker Compose already installed${NC}"
fi

# Step 4: Clone repository
echo -e "${BLUE}Step 4/7: Cloning repository...${NC}"
if [ ! -d "kirana-store" ]; then
    git clone $GITHUB_REPO kirana-store
    echo -e "${GREEN}✓ Repository cloned${NC}"
else
    echo -e "${YELLOW}⚠ Repository already exists, pulling latest changes...${NC}"
    cd kirana-store
    git pull
    cd ..
fi

# Step 5: Create network
echo -e "${BLUE}Step 5/7: Setting up Docker network...${NC}"
docker network create kirana_network 2>/dev/null || echo "Network already exists"
echo -e "${GREEN}✓ Network ready${NC}"

# Step 6: Build images
echo -e "${BLUE}Step 6/7: Building Docker images...${NC}"
cd kirana-store

# Build API
echo "   Building API image..."
docker build -f Dockerfile -t kirana-api:latest .

# Build UI
echo "   Building UI image..."
docker build -f Dockerfile.UI -t kirana-ui:latest .

cd ..
echo -e "${GREEN}✓ Images built${NC}"

# Step 7: Start services
echo -e "${BLUE}Step 7/7: Starting services...${NC}"

# Start PostgreSQL
echo "   Starting PostgreSQL..."
docker run -d \
  --name kirana_postgres \
  --network kirana_network \
  -e POSTGRES_USER=$DB_USER \
  -e POSTGRES_PASSWORD=$DB_PASSWORD \
  -e POSTGRES_DB=$DB_NAME \
  -v kirana_postgres_data:/var/lib/postgresql/data \
  -p 5432:5432 \
  postgres:15 || true

# Wait for PostgreSQL
echo "   Waiting for PostgreSQL to start..."
sleep 5

# Start API
echo "   Starting API..."
docker run -d \
  --name kirana_api \
  --network kirana_network \
  -e ConnectionStrings__DefaultConnection="Server=kirana_postgres;Port=5432;Database=$DB_NAME;User Id=$DB_USER;Password=$DB_PASSWORD;" \
  -e Jwt__Key="ThisIsMySuperSecretKeyForJwtAuthentication1234567890" \
  -e Jwt__Issuer="WebApi" \
  -e Jwt__Audience="WebApiUsers" \
  -e ASPNETCORE_ENVIRONMENT="Production" \
  -p $API_PORT:8080 \
  kirana-api:latest || true

# Wait for API
echo "   Waiting for API to start..."
sleep 5

# Start UI
echo "   Starting UI..."
docker run -d \
  --name kirana_ui \
  --network kirana_network \
  -e ApiBaseUrl="http://kirana_api:8080/api/" \
  -e ASPNETCORE_ENVIRONMENT="Production" \
  -p $UI_PORT:8080 \
  kirana-ui:latest || true

echo -e "${GREEN}✓ Services started${NC}"

# Step 8: Configure firewall
echo -e "${BLUE}Configuring firewall...${NC}"
sudo firewall-cmd --permanent --add-port=$API_PORT/tcp 2>/dev/null || true
sudo firewall-cmd --permanent --add-port=$UI_PORT/tcp 2>/dev/null || true
sudo firewall-cmd --reload 2>/dev/null || true
echo -e "${GREEN}✓ Firewall configured${NC}"

# Print summary
echo ""
echo -e "${GREEN}✅ Deployment Complete!${NC}"
echo ""
echo -e "${BLUE}Access your application:${NC}"
echo "   API Swagger:  http://YOUR_PUBLIC_IP:$API_PORT/swagger/index.html"
echo "   Frontend UI:  http://YOUR_PUBLIC_IP:$UI_PORT"
echo ""
echo -e "${BLUE}Useful commands:${NC}"
echo "   View logs:           docker logs kirana_api"
echo "   Restart services:    docker restart kirana_api kirana_ui"
echo "   Stop services:       docker stop kirana_api kirana_ui kirana_postgres"
echo "   Update from git:     cd kirana-store && git pull && docker build -f Dockerfile -t kirana-api:latest . && docker restart kirana_api"
echo ""
