# Circuit Runners - Game Server

This is the server component for the Circuit Runners game, handling multiplayer functionality and game state management.

## Features

- 🎮 Real-time multiplayer support using WebSockets
- 🌐 Web-based game hosting
- 🔗 Shareable game links
- 🏠 Local development support
- 🚀 Easy deployment

## Prerequisites

- Node.js (v14 or later)
- npm (comes with Node.js)

## Installation

1. Clone the repository
2. Navigate to the server directory:
   ```bash
   cd Server
   ```
3. Install dependencies:
   ```bash
   npm install
   ```

## Run locally (testing)

1) Install dependencies (first time only)

```bash
npm install
```

2) Start the dev server (auto-restart on changes)

```bash
npm run dev
```

3) Open the lobby

- http://localhost:3000
- Create a game to get a shareable link, or join by entering a Room ID

4) Open the game page directly (useful for testing)

- http://localhost:3000/game?room=ROOM_ID&name=YourName
- Before the Unity build is exported, a placeholder page will load and still join/create the room so you can test networking and links.

Notes

- The server listens on port 3000 by default. To change it, set `PORT` before starting.
- To run without nodemon: `npm start`
- Health check endpoint: http://localhost:3000/health

## Environment Variables

Create a `.env` file in the server root with the following variables:

```
PORT=3000
NODE_ENV=development
```

## Game Integration

### Connecting from Unity

1. Build your Unity game for WebGL
2. Place the built files in the `public/game` directory (this will replace the placeholder)
3. The game will be accessible at `http://localhost:3000/game`

Unity WebGL export steps (checklist)

1) In Unity, switch Platform to WebGL (File → Build Settings → WebGL → Switch Platform)
2) Player Settings (Project Settings → Player → WebGL)
   - Resolution and Presentation: Template = Default (or your custom)
   - Publishing Settings:
     - Compression Format = Brotli (recommended) or Gzip
     - Decompression Fallback = Enabled (optional; helps on hosts without proper encodings)
     - Data Caching = Enabled
     - Threads = Disabled unless your host supports COOP/COEP
3) Build
   - Build folder target: `Server/public/game/`
   - Unity will write `index.html` and a `Build/` folder into `Server/public/game/`
4) Verify output
   - `Server/public/game/index.html` (Unity-generated)
   - `Server/public/game/Build/*.data(.br)`, `*.framework.js(.br)`, `*.wasm(.br)`
5) WebGL JS bridge (for URL + messaging)
   - Custom bridge already added at: `CircuitRunners/Assets/Plugins/WebGL/NetworkBridge.jslib`
   - In WebGL builds, your C# calls to `GetURLParameter` and `SendNetworkMessageWebGL` will bind to window functions defined on the page.

Unity WebGL export tips

- Expected output under `Server/public/game/`:
  - `index.html` (Unity-generated; safe to overwrite the placeholder)
  - `Build/YourBuild.data`, `Build/YourBuild.framework.js`, `Build/YourBuild.wasm` (plus `.br`/`.gz` if compression is enabled)
- Recommended Player Settings for the web
  - Compression: Brotli or Gzip (both are served as static files here)
  - WebAssembly: enabled
  - Threads: keep disabled unless your deployment host supports COOP/COEP headers
- Join link format for testing or sharing
  - `http(s)://<host-or-ip>:<port>/game?room=<ROOM_ID>&name=<YourName>`

### WebSocket Connection

Use the following code in your Unity game to connect to the server:

```csharp
using SocketIOClient;

// Initialize the client
var client = new SocketIO("http://localhost:3000");

// Connect to the server
await client.ConnectAsync();

// Join a game
await client.EmitAsync("joinGame", new { playerName = "Player1", roomId = "room123" });

// Listen for game updates
client.On("gameState", response => {
    var gameState = response.GetValue<GameState>();
    // Update game state
});

// Send game updates
await client.EmitAsync("gameUpdate", new {
    // Your game state here
});
```

## Deploy to the web (production)

Our primary option is the Kent-provided host (institutional platform). As a backup, I included a Docker-based deployment below.

Institutional host (Kent) – general guidance

1) Provision a Node.js-capable web service or container on the platform
2) Set environment variables
   - `NODE_ENV=production`
   - `PORT` (if required by the platform; many inject it automatically)
   - `CORS_ORIGIN=https://your-official-hostname` (tighten CORS)
3) Start command
   - `npm start`
4) Upload your Unity WebGL build to `Server/public/game/` (see Unity steps above)
5) Expose HTTPS and ensure WebSockets are allowed (Socket.IO on `/socket.io`)

Docker backup plan (runs anywhere with Docker)

From the `Server/` directory (Dockerfile provided):

```bash
# 1) Build the image (ensure your Unity build is already in Server/public/game/)
docker build -t circuit-runners-server .

# 2) Run the container locally (port 3000)
docker run --name circuit-runners -p 3000:3000 \
  -e NODE_ENV=production \
  -e CORS_ORIGIN="http://localhost:3000" \
  circuit-runners-server

# Optional: run with your public hostname for correct CORS
docker run --name circuit-runners -p 3000:3000 \
  -e NODE_ENV=production \
  -e CORS_ORIGIN="https://your-domain" \
  circuit-runners-server
```

Once running:

- Lobby: `http://<host>:3000`
- Game: `http://<host>:3000/game?room=ROOM_ID&name=YourName`

CORS & security notes

- In development, CORS is `*`. In production, set `CORS_ORIGIN` to your domain.
- Use HTTPS in production and ensure the platform supports WebSockets.

LAN testing (on your network)

1) Allow inbound connections on the server’s port in your OS firewall
2) Have teammates connect to `http://<your-local-ip>:3000`
3) Join links will work on LAN as well: `http://<your-local-ip>:3000/game?room=...&name=...`

## Troubleshooting

- **Connection Issues**: Ensure the server is running and accessible from the client
- **CORS Errors**: Verify that the client is making requests to the correct server URL
- **Socket.IO Version Mismatch**: Make sure the Socket.IO client version in Unity matches the server version

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
