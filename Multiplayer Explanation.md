# How does the server handle Multiplayer? 
- Leaderboards, Ranks, Checkpoints, etc.

## Answer:
The server handles multiplayer communication via Socket.IO, which provides real-time, bidirectional messaging between clients (players) and the server.
Each Unity WebGL client sends player state updates — such as position, rotation, speed, and checkpoint progress — to the server at regular intervals (for example, every frame or fixed update).

The Socket.IO server merges these updates into a shared game state, which is stored in Redis, a fast in-memory data structure store. The updated state is then broadcast to all connected clients in the same room. This ensures that every player’s game instance stays synchronized in real time.

When a player crosses a checkpoint or changes speed, the server recalculates ranks and leaderboards, then pushes the updated race data to all clients.
This implementation keeps every client’s view of the race consistent while allowing for scalable multiplayer sessions over the web.

## Possible Questions
- Is this fully implemented? 
- What is Redis? 
- What is Socket? 
- Will Redis be able to have enough room in it's cache?  

## Answers:
- No, the server currently has the framework (skeleton), to be used for testing, local hosting, web hosting, and implementation to host the unity game.  However, it still needs the logic to handle, the actual multiplayer features like leaderboards, ranks, position changes, player disconneciton, checkpoints reached, etc.  
- These will implemented in a gameserver.js file/ possible server.js expansion that handles all of this.

- Redis is an in-memory key value data store, often used for caching and session storgage, and synchronization across distributed systems. 

In this project, it's used to help the Socket.IO server store and share game state between rooms and servers, allowing the system to scale horizontally if we host it on the web.

- Socket.IO is a library that enables real-time, event-based communication between a client and a server over WebSockets.

It’s what lets Unity clients instantly send updates (like checkpoint reached or position changed) and immediately receive updates about other players. 

- Yes, Redis stores data in memory and is extremely fast. Game session data (positions, ranks, timers, etc.) is small and temporary, so even hundreds of rooms can be supported.

For large deployments, a Redis adapter can be used to distribute the load across multiple Redis instances and ensure smooth scaling.

- Overview to understand communication: 
Unity Client → Socket.IO → Node.js Server → Redis → 
Feedback to Clients