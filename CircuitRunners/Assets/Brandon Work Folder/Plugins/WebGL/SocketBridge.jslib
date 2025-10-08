mergeInto(LibraryManager.library, {
  socket: null,

  ConnectToServer: function (urlPtr) {
    const url = UTF8ToString(urlPtr);
    console.log("[WebGL] Connecting to Socket.IO server:", url);

    if (typeof io === "undefined") {
      console.error("Socket.IO client library not loaded!");
      return;
    }

    this.socket = io(url);

    this.socket.on("connect", () => {
      console.log("[WebGL] Connected:", this.socket.id);
      unityInstance.SendMessage("UnityClientManager", "OnWebSocketReady");
    });

    this.socket.on("playerJoined", data =>
      unityInstance.SendMessage("UnityClientManager", "OnPlayerJoined", JSON.stringify(data))
    );

    this.socket.on("playerLeft", data =>
      unityInstance.SendMessage("UnityClientManager", "OnPlayerLeft", JSON.stringify(data))
    );

    this.socket.on("roomUpdate", data =>
      unityInstance.SendMessage("UnityClientManager", "OnGameStateUpdate", JSON.stringify(data))
    );

    this.socket.on("error", err =>
      unityInstance.SendMessage("UnityClientManager", "OnNetworkError", JSON.stringify({ message: err }))
    );
  },

  SendNetworkMessageWebGL: function (typePtr, dataPtr) {
    const type = UTF8ToString(typePtr);
    const data = UTF8ToString(dataPtr);
    if (this.socket) this.socket.emit(type, JSON.parse(data));
    else console.warn("[WebGL] Socket not ready");
  },

  GetURLParameter: function (namePtr) {
    const name = UTF8ToString(namePtr);
    const urlParams = new URLSearchParams(window.location.search);
    const value = urlParams.get(name);
    return value ? allocateUTF8(value) : 0;
  }
});
