// =============================================================
// Circuit Runners - Multiplayer Game Server
// =============================================================

const express = require('express');
const http = require('http');
const { Server } = require('socket.io');
const path = require('path');
const cors = require('cors');
const compression = require('compression');
const expressStaticGzip = require('express-static-gzip');

let createAdapter, Redis;
try {
  ({ createAdapter } = require('@socket.io/redis-adapter'));
  Redis = require('ioredis');
} catch (_) {}

const app = express();
const server = http.createServer(app);
app.set('trust proxy', 1);

const io = new Server(server, {
  cors: {
    origin: process.env.CORS_ORIGIN || '*',
    methods: ['GET', 'POST'],
  },
});

// Optional Redis (for scaling)
if (createAdapter && Redis) {
  try {
    const pub = new Redis(process.env.REDIS_URL || 'redis://localhost:6379');
    const sub = new Redis(process.env.REDIS_URL || 'redis://localhost:6379');
    io.adapter(createAdapter(pub, sub));
    console.log('✅ Redis adapter enabled');
  } catch (err) {
    console.warn('Redis unavailable:', err.message);
  }
}

// Middleware
app.use(compression());
app.use(cors());
app.use(express.static(path.join(__dirname, 'public')));

// Serve Unity WebGL build (precompressed)
const unityBuildPath = path.join(__dirname, 'public', 'game', 'Build');
app.use(
  '/game/Build',
  expressStaticGzip(unityBuildPath, {
    enableBrotli: true,
    orderPreference: ['br', 'gz'],
    setHeaders: (res) => res.setHeader('Cache-Control', 'public, max-age=31536000, immutable'),
  })
);

app.get('/health', (_, res) => res.json({ status: 'ok', uptime: process.uptime() }));

// =============================================================
// Game Data Structures
// =============================================================
const rooms = new Map(); // roomId → room info
const players = new Map(); // socket.id → player info

function generateRoomCode() {
  return Math.random().toString(36).substring(2, 8).toUpperCase();
}

function broadcastRoomUpdate(roomId) {
  const room = rooms.get(roomId);
  if (!room) return;
  const playerList = Array.from(room.players.values()).map((p) => ({
    id: p.id,
    name: p.name,
    isHost: p.isHost,
    checkpoint: p.checkpoint || 0,
  }));
  io.to(roomId).emit('roomUpdate', playerList);
}

// =============================================================
// Socket.IO Logic
// =============================================================
io.on('connection', (socket) => {
  console.log(`🔌 New connection: ${socket.id}`);

  // Join room
  socket.on('joinGame', (data) => {
    const { playerName, roomId } = data || {};
    const cleanName = playerName?.trim() || `Player_${Math.floor(Math.random() * 1000)}`;
    const targetRoomId = roomId?.trim() || generateRoomCode();

    if (!rooms.has(targetRoomId)) {
      rooms.set(targetRoomId, {
        id: targetRoomId,
        players: new Map(),
        checkpoints: new Map(),
        gameState: 'waiting',
        createdAt: Date.now(),
      });
    }

    const room = rooms.get(targetRoomId);
    const isHost = room.players.size === 0;

    const player = {
      id: socket.id,
      name: cleanName,
      roomId: targetRoomId,
      isHost,
      checkpoint: 0,
    };

    room.players.set(socket.id, player);
    players.set(socket.id, player);
    socket.join(targetRoomId);

    console.log(`✅ ${cleanName} joined ${targetRoomId}${isHost ? ' (host)' : ''}`);

    socket.emit('joined', { roomId: targetRoomId, isHost });
    broadcastRoomUpdate(targetRoomId);
  });

  // Host starts the race
socket.on("startRace", () => {
  const player = players.get(socket.id);
  if (!player) return;

  const room = rooms.get(player.roomId);
  if (!room) return;

  // Only host can start
  if (!player.isHost) {
    socket.emit("errorMessage", { message: "Only the host can start the race." });
    return;
  }

  // Avoid duplicate starts
  if (room.gameState !== "waiting") return;

  room.gameState = "countdown";
  console.log(`🏁 Race countdown started in room ${room.id}`);

  // Send countdown to all clients
  let countdown = 3;
  const countdownInterval = setInterval(() => {
    io.to(room.id).emit("countdown", { time: countdown });
    countdown--;

    if (countdown < 0) {
      clearInterval(countdownInterval);
      room.gameState = "playing";
      io.to(room.id).emit("startRace", { roomId: room.id });
      console.log(`🚗 Race started in room ${room.id}`);
    }
  }, 1000);
});


  // Position update
  socket.on('positionUpdate', (pos) => {
    const player = players.get(socket.id);
    if (!player) return;
    io.to(player.roomId).emit('playerPositions', {
      id: player.id,
      x: pos.x,
      y: pos.y,
      z: pos.z,
    });
  });

  // Checkpoint reached
  socket.on('checkpointReached', ({ checkpointIndex }) => {
    const player = players.get(socket.id);
    if (!player) return;

    player.checkpoint = checkpointIndex;
    const room = rooms.get(player.roomId);
    room.checkpoints.set(player.id, checkpointIndex);

    // Recalculate ranks
    const ranked = Array.from(room.checkpoints.entries())
      .sort((a, b) => b[1] - a[1])
      .map(([id, cp], i) => ({ id, rank: i + 1 }));

    io.to(player.roomId).emit('rankUpdate', ranked);
  });

  // Disconnection cleanup
  socket.on('disconnect', () => {
    const player = players.get(socket.id);
    if (!player) return;

    const room = rooms.get(player.roomId);
    if (room) {
      room.players.delete(socket.id);
      room.checkpoints.delete(socket.id);
      io.to(player.roomId).emit('playerLeft', { id: socket.id, name: player.name });

      if (room.players.size === 0) {
        rooms.delete(player.roomId);
        console.log(`🗑 Room ${player.roomId} deleted`);
      } else {
        broadcastRoomUpdate(player.roomId);
      }
    }
    players.delete(socket.id);
    console.log(`❌ ${player.name} disconnected`);
  });

  socket.on('error', (err) => console.error('Socket error:', err));
});

// =============================================================
// Routes
// =============================================================
app.get('/api/rooms', (_, res) => {
  const list = Array.from(rooms.values()).map((r) => ({
    id: r.id,
    playerCount: r.players.size,
    gameState: r.gameState,
    createdAt: r.createdAt,
  }));
  res.json(list);
});

// Default routes
app.get('/game', (_, res) => res.sendFile(path.join(__dirname, 'public', 'game', 'index.html')));
app.get('*', (_, res) => res.sendFile(path.join(__dirname, 'public', 'index.html')));

// Start server
const PORT = process.env.PORT || 3000;
server.listen(PORT, () => console.log(`🚀 Circuit Runners server running on port ${PORT}`));
