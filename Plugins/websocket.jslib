
mergeInto(LibraryManager.library, {

  WebSocketInit: function(url) {
        this.socket = new WebSocket(UTF8ToString(url));
  },
  WebSocketSend: function(message) {
       	this.socket.send(UTF8ToString(message));
  },
  WebSocketReadyState: function() {
	return this.socket.readyState;
  },
  WebSocketAddMessageListener: function(obj) {
	this.socket.addEventListener("message", (event) => 
        {
		var bufferSize = lengthBytesUTF8(event.data) + 1;
            	var buffer = _malloc(bufferSize);
            	stringToUTF8(event.data, buffer, bufferSize);

		Module['dynCall_vi'](obj,[buffer]);
	});
  },
  WebSocketClose: function() {
	this.socket.close();
  }

});
