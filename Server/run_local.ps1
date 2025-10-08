Write-Host "Starting Circuit Runners (local)..."

# Ensure Node modules are up to date
npm install

# Set environment vars
$env:PORT = 3000
$env:NODE_ENV = "development"
$env:CORS_ORIGIN = "http://localhost:3000"

# Start server with auto-reload
npm run dev
