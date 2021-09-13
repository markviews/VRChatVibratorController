/* Node.js Server */
const fs = require('fs')
const https = require('https')
const WebSocket = require('ws')
const server = https.createServer({
  cert: fs.readFileSync('/etc/letsencrypt/live/control.markstuff.net/fullchain.pem'),
  key: fs.readFileSync('/etc/letsencrypt/live/control.markstuff.net/privkey.pem')
})
const wss = new WebSocket.Server({ server })
let rateLimit = new Map()
let openCodes = new Map()//waiting room
let sessions = new Map()//active sessions

log("Server ready")

wss.on('connection', function connection(ws) {

	ws.on('message', function incoming(msg) {
		msg = msg.replace(new RegExp(String.fromCharCode(0), "g"),"")
		var args = msg.toString().split(" ")

		switch(args[0]) {
			case "speed":
			case "air":
			case "rotate":
				//controlClient trying to set toyClient speed
				if (sessions.has(ws)) {
					var id = args[1]
					var toyClient = sessions.get(ws).get(id)
          if (toyClient != undefined)
					     toyClient.send(msg)
				}
			break
			case "new"://toyClient requesting a new code
				var id = newID()
				openCodes.set(id, ws)
				ws.send("id "  + id)
				log("Generated new id for a client: " + id)
			break
			case "join":
				//controlClient trying to join a toyClient

				//rate limit
				var ip = ws._socket.remoteAddress
				if (rateLimit.has(ip)) {
					const rate = rateLimit.get(ip)
					if (rate.length >= 50) {
						var earliest = rate[0]
						if (earliest < Date.now() - 300000) {
							rate.shift()
						} else {
							ws.send("rateLimited")
							return
						}
					}
					rateLimit.set(ip, rate.concat([Date.now()]))
				} else {
					rateLimit.set(ip, [Date.now()])
				}

				var id = args[1]
				if (openCodes.has(id)) {
					var toyClient = openCodes.get(id)
					openCodes.delete(id)
					sessions.set(ws, new Map().set(id, toyClient))
					toyClient.send("joined")
          log("Session joined: " + id)
				} else {
				ws.send("notFound")
				}
			break
			case "toys":
			case "add":
			case "remove":
				//toyClient sending toy list to controlClient
				for (const [controlClient, session] of sessions.entries()) {
					for (const [id, toyClient] of session.entries()) {
						if (toyClient === ws) {
							controlClient.send(msg)

							for (var i = 1; i < args.length; i++) {
							var data = args[i].toString().split(":")
							var toyID = data[1]

							if (args[0] == "remove") {
								if (sessions.get(controlClient).has(toyID))
									sessions.get(controlClient).delete(toyID)
							} else
								sessions.get(controlClient).set(toyID, toyClient)

							}

							return
						}
					}
				}
			break
		}

	})

	ws.on('close', function close() {
		//check if toyClient left waiting room
		for (let id of openCodes.keys()) {
			if (ws == openCodes.get(id)) {
				openCodes.delete(id)
				log("Waiting room closed: " + id)
			}
		}

		//check if this was a controlClient in a session
		if (sessions.has(ws)) {
			var roomID = ""
			//move all toyClients connected to this controlClient to the waiting room
			for (const [id, toyClient] of sessions.get(ws).entries()) {
				if (id.length == 4) {
				openCodes.set(id, toyClient)
				toyClient.send("left")
				roomID = id
				}
			}
			sessions.delete(ws)
			if (roomID != "")
			log("controlCliet left, moving toyClient to waiting room " + roomID)
			return
		}

		//check if this was a toyClient in a session
		for (const [controlClient, session] of sessions.entries()) {
			var myid = ""
			for (const [id, toyClient] of session.entries()) {
				if (ws === toyClient) {

          if (id != undefined) {
            if (id.length == 4) {
  						myid = id
  					}
            session.delete(id)
          }

				}
			}
			if (session.size == 0 && myid != "") {
				controlClient.send("left")
				sessions.delete(controlClient)
				log("toyClient left, closing session " + myid)
				return
			}
		}

	})


})
server.listen(8080)

function log(msg) {
	console.log("[Waiting: " + openCodes.size + " Active: " + sessions.size + "] " + msg)
}

//every 10 mins check to delete old rate limits
(function loop() {
	for (let ip of rateLimit.keys()) {
		//if newest rateLimit is expired, clear limit
		var rate = rateLimit.get(ip)
		if (rate[rate.length - 1] < Date.now() - 300000) {
			rateLimit.delete(ip)
			//log("clearned a rate limit")
		}
	}
setTimeout(loop, 600000)})()

//TODO verify code isn't duplicate
function newID() {
   var result = ''
   var characters = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz123456789'//removed Il0oO
   for (var i=0; i<4; i++) {
      result += characters.charAt(Math.floor(Math.random() * characters.length))
   }
   return result
}
