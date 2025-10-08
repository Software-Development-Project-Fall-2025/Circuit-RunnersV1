Write-Host "Building and starting Docker container..."
docker build -t circuit-runners-server .
docker run --name circuit-runners -p 3000:3000 `
  -e NODE_ENV=production `
  -e CORS_ORIGIN="http://YOUR_PUBLIC_IP_OR_DOMAIN" `
  circuit-runners-server
