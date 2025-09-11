const express = require('express');
const http = require('http');
const { Server } = require('socket.io');
const path = require('path');
const cors = require('cors');
const compression = require('compression');
const expressStaticGzip = require('express-static-gzip');

const app = express();
const server = http.createServer(app);
const io = new Server(server, {
  cors: {
    origin: process.env.CORS_ORIGIN || '*',
    methods: ['GET', 'POST']
  }
});

// Middleware
app.use(compression());
app.use(cors());
app.use(express.static(path.join(__dirname, 'public')));

// Serve Unity WebGL Build folder with precompressed assets (Brotli/Gzip)
const unityBuildPath = path.join(__dirname, 'public', 'game', 'Build');
app.use('/game/Build', expressStaticGzip(unityBuildPath, {
  enableBrotli: true,
  orderPreference: ['br', 'gz'],
  setHeaders: (res, filePath) => {
    // Long cache for build artifacts
    res.setHeader('Cache-Control', 'public, max-age=31536000, immutable');
  }
}));

// Health check endpoint
app.get('/health', (req, res) => {
  res.json({ status: 'ok', uptime: process.uptime() });
});

// Game state
const rooms = new Map();
const players = new Map();

// Socket.io connection handler
io.on('connection', (socket) => {
  console.log(`New connection: ${socket.id}`);
  
  // Handle new player
  socket.on('joinGame', (playerData) => {
    const { playerName, roomId } = playerData || {};

    // Decide the room ID to use
    let targetRoomId = roomId && String(roomId).trim().length > 0
      ? String(roomId).trim()
      : `room_${Math.random().toString(36).substr(2, 9)}`;

    // Create the room if it doesn't exist yet
    if (!rooms.has(targetRoomId)) {
      rooms.set(targetRoomId, {
        id: targetRoomId,
        players: new Map(),
        gameState: 'waiting',
        createdAt: Date.now()
      });
    }

    const room = rooms.get(targetRoomId);
    const isFirstInRoom = room.players.size === 0;

    const player = {
      id: socket.id,
      name: playerName || `Player_${Math.floor(Math.random() * 1000)}`,
      roomId: targetRoomId,
      isHost: isFirstInRoom
    };

    // Add player to room and global index
    room.players.set(socket.id, player);
    players.set(socket.id, player);

    socket.join(targetRoomId);

    // Notify all players in the room about the update
    io.to(targetRoomId).emit('playerJoined', {
      playerId: socket.id,
      playerName: player.name,
      room: {
        id: room.id,
        playerCount: room.players.size,
        gameState: room.gameState,
        createdAt: room.createdAt
      },
      isHost: player.isHost
    });

    // For convenience, reply directly to the joiner with their assigned room
    socket.emit('joined', { roomId: targetRoomId, isHost: player.isHost });

    console.log(`Player ${player.name} joined room ${targetRoomId}${player.isHost ? ' (host)' : ''}`);
  });

  // Handle game state updates
  socket.on('gameUpdate', (data) => {
    const player = players.get(socket.id);
    if (player) {
      io.to(player.roomId).emit('gameState', data);
    }
  });

  // Handle disconnection
  socket.on('disconnect', () => {
    const player = players.get(socket.id);
    if (player) {
      const room = rooms.get(player.roomId);
      if (room) {
        room.players.delete(socket.id);
        
        // If room is empty, clean it up
        if (room.players.size === 0) {
          rooms.delete(player.roomId);
        } else {
          // Notify other players
          socket.to(player.roomId).emit('playerLeft', {
            playerId: socket.id,
            playerName: player.name
          });
        }
      }
      players.delete(socket.id);
      console.log(`Player ${player.name} disconnected`);
    }
  });

  // Handle errors
  socket.on('error', (error) => {
    console.error('Socket error:', error);
  });
});

// API Routes
app.get('/api/rooms', (req, res) => {
  const publicRooms = Array.from(rooms.values())
    .filter(room => room.players.size < 4) // Example: Limit players per room
    .map(room => ({
      id: room.id,
      playerCount: room.players.size,
      gameState: room.gameState,
      createdAt: room.createdAt
    }));
  res.json(publicRooms);
});

// Serve the Unity WebGL build
app.get('/game', (req, res) => {
  res.sendFile(path.join(__dirname, 'public', 'game', 'index.html'));
});

// Serve the main page
app.get('*', (req, res) => {
  res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

// Start server
const PORT = process.env.PORT || 3000;
server.listen(PORT, () => {
  console.log(`Server running on port ${PORT}`);
  console.log(`Socket.IO CORS origin: ${process.env.CORS_ORIGIN || '*'} (set CORS_ORIGIN to restrict in production)`);
});

// Error handling
process.on('unhandledRejection', (err) => {
  console.error('Unhandled Rejection:', err);
});

process.on('uncaughtException', (err) => {
  console.error('Uncaught Exception:', err);
  process.exit(1);
});
