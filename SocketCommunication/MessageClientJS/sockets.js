var InitializeWebSocket = function (url, onauthorized, onmessage, onclose) {

	var connected = false;
	var wsImpl = window.WebSocket || window.MozWebSocket;
	var url = url;

	var WebSocketManager = {};

	tryConnection();
	var reconnectTimer = window.setInterval(function () {
		tryConnection()
	}, 1000);

	function tryConnection() {
		if (!connected) {
			var socketUrl = url;
			if (window.url) socketUrl = window.url;

			// create a new websocket and connect
			WebSocketManager.ws = new wsImpl(socketUrl);

			// when data is coming from the server, this metod is called
			WebSocketManager.ws.onmessage = function (evt) {
				var message = JSON.parse(evt.data);

				if (message.action.toUpperCase() == "AUTHORIZE") {
					if (typeof (onauthorized) === "function") onauthorized();
					console.log("Module authorized");
				} else {
					if (typeof (onmessage) === "function") onmessage(message);
					console.log("Message received", message);
				}

			};


			// when the connection is established, this method is called
			WebSocketManager.ws.onopen = function () {
				connected = true;

				//send auth request
				var mex = new Message();
				mex.action = "authorize";
				mex.topic = "";
				mex.content = {
					name: "JS",
					token: "ok",
					unique: false
				};
				WebSocketManager.ws.send(JSON.stringify(mex));

				console.log("Module connection opened");

			};

			// when the connection is closed, this method is called
			WebSocketManager.ws.onclose = function () {
				connected = false;
				if (typeof (onclose) === "function") onclose();
				console.log("Module connection closed");
			}


			function Message() {
				var action = null;
				var topic = null;
				var content = null;
			}


		}
	}


	WebSocketManager.SubscribeTo = function (topic) {
		var mex = new Message();
		mex.action = "subscribe";
		mex.topic = topic;
		WebSocketManager.ws.send(JSON.stringify(mex));
		console.log("Message sent", mex);
	}

	WebSocketManager.Publish = function (topic, content = "") {
		var mex = new Message();
		mex.action = "publish";
		mex.topic = topic;
		mex.content = content;
		WebSocketManager.ws.send(JSON.stringify(mex));
		console.log("Message sent", mex);
	}



	return WebSocketManager;

}
