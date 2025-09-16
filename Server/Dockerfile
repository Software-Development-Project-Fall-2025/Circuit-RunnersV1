# Circuit Runners server - Dockerfile
# Build a small production image to run the Node/Express + Socket.IO server

FROM node:18-alpine AS base

# Non-root for security
RUN addgroup -S appgroup && adduser -S appuser -G appgroup

WORKDIR /app

# Install only the dependencies first (leverages Docker layer cache)
COPY package*.json ./
RUN npm ci --only=production || npm install --omit=dev

# Copy the rest of the server code (including public/ with Unity WebGL build)
COPY . .

ENV NODE_ENV=production
ENV PORT=3000
EXPOSE 3000

USER appuser

CMD ["node", "server.js"]
