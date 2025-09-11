mergeInto(LibraryManager.library, {
  GetURLParameter: function(namePtr) {
    try {
      var name = UTF8ToString(namePtr);
      var params = new URLSearchParams(window.location.search);
      var val = params.get(name) || '';
      var lengthBytes = lengthBytesUTF8(val) + 1;
      var stringOnWasmHeap = _malloc(lengthBytes);
      stringToUTF8(val, stringOnWasmHeap, lengthBytes);
      return stringOnWasmHeap;
    } catch (e) {
      var fallback = '';
      var lengthBytes = lengthBytesUTF8(fallback) + 1;
      var stringOnWasmHeap = _malloc(lengthBytes);
      stringToUTF8(fallback, stringOnWasmHeap, lengthBytes);
      return stringOnWasmHeap;
    }
  },

  SendNetworkMessageWebGL: function(typePtr, dataPtr) {
    try {
      var type = UTF8ToString(typePtr);
      var data = UTF8ToString(dataPtr);
      if (typeof window !== 'undefined' && typeof window.SendNetworkMessage === 'function') {
        var parsed;
        try { parsed = data ? JSON.parse(data) : undefined; } catch (_) { parsed = data; }
        window.SendNetworkMessage(type, parsed);
      } else {
        console.warn('SendNetworkMessage is not available on window');
      }
    } catch (e) {
      console.error('SendNetworkMessageWebGL error:', e);
    }
  }
});
