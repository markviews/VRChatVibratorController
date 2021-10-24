/* Webpage Client */
var ws = new WebSocket("ws://control.markstuff.net:8080")
	ws.onopen = function() {
		document.getElementById("serverStatus").innerText = "Server Status: Connected to server"
	}

	ws.onmessage = function (evt) {
			var msg = evt.data
			console.log(msg)
	var args = msg.split(" ")

	switch(args[0]) {
		case "speed":
			//controllerClient setting toy speed
			var id = args[1]
			var speed = args[2]
			var toy = myToys.get(id)
			var edgeVibrator = ""
			if (args.length == 4) {
				edgeVibrator = args[3]
			}
			lovense.sendCommand(`Vibrate${edgeVibrator}`,{v:speed, t:id})
			document.getElementById(id + "speed" + edgeVibrator).value = speed
		break
		case "air":
			var id = args[1]
			var speed = args[2]
			var toy = myToys.get(id)
			lovense.sendCommand("AirAuto",{v:speed, t:id})
			document.getElementById(id + "air").value = speed
		break
		case "rotate":
			var id = args[1]
			var toy = myToys.get(id)
			lovense.sendCommand("RotateChange",{t:id})
		break
		case "id":
			//server assigining an ID
			document.getElementById("codeDisplay").style.display = "block"
			document.getElementById("id").innerText = args[1]
			document.getElementById("serverStatus").innerText = "Server Status: Received code from server"
			hasCode = true
		break
		case "joined":
			//controllerClient joined, session is active
			//send controllerClient toy name and ids
			var str = ""
			for (const [id, toy] of myToys.entries()) {
				if (toy.share)
					str += toy.name + ":" + id + " "
			}
			str = str.slice(0, -1)//remove space at end
			ws.send("add " + str)
			document.getElementById("serverStatus").innerText = "Server Status: Control Client joined"
		break
		case "left":
			//controllerClient left, moved back to waiting room
			document.getElementById("serverStatus").innerText = "Server Status: Control Client left"
		break
	}
	}

	ws.onclose = function() {
			document.getElementById("serverStatus").innerText = "Server Status: Disconnected from server"
			sendWebhook()
	}

var hasCode = false
let myToys = new Map()

function getCode() {
	if (!hasCode)
		ws.send('new')
}

function toggle(button) {
	var id = button.id
	var toy = myToys.get(id)

	if (toy.share) {
		toy.share = false
		document.getElementById(id).style.backgroundColor = "#a80a2a"
		ws.send("remove " + toy.name + ":" + id)
	} else {
		toy.share = true
		document.getElementById(id).style.backgroundColor = "#33a151"
		ws.send("add " + toy.name + ":" + id)
	}

	fixToolTip(id)
}

var storedIP = localStorage.getItem("ip").replaceAll("-",".")
if (storedIP != undefined) {
	document.getElementById("ip").value = storedIP
}

function searchForToys() {
	var ip = document.getElementById("ip").value
	if (ip != "") {
		localStorage.setItem("ip", ip)
		lovense.setConnectCallbackData({domain: ip, httpsPort: "34567"})
	}

	toys = lovense.getToys()
	console.log(toys)
	for (var toy of Object.keys(toys)) {
		var toyJson = toys[toy]
		addToy(toyJson)
	}

	if (myToys.size == 0) {
		document.getElementById("error").style.display = "block"
	} else {
		document.getElementById("error").style.display = "none"
		getCode()
	}
}

function fixToolTip(id) {
	var toy = myToys.get(id)
	document.getElementById(id).childNodes[1].innerHTML = "Sharing:" + ((toy.share) == true ? 'on' : 'off')  + "<br>Status: " + ((toy.status) == 1 ? 'Active' : 'Inactive') + "<br>ID: " + id + "<br>Host: " + toy.url
	if (toy.status == 0) {
		document.getElementById(id).disabled = true
		document.getElementById(id).style.backgroundColor = "#a80a2a"
	} else {
		document.getElementById(id).disabled = false
		//document.getElementById(id).style.backgroundColor = "#33a151"
	}
}

function addToy(toyJson) {
	var id = toyJson["id"]
	var name = toyJson["name"]
	var status = toyJson["status"]

	//console.log(toyJson)
	name = name.charAt(0).toUpperCase() + name.slice(1)

	var toy = myToys.get(id)
	if (toy != undefined) {
		if (toy.status != status) {
			toy.status = status
			fixToolTip(id)

			if (toy.status == 0) {
				if (toy.share) toggle(document.getElementById(id))
			} else {
				if (!toy.share) toggle(document.getElementById(id))
			}

		}
	return
	}

	myToys.set(id, {share: true, name: name, status: status})

	var button = document.createElement("button")
	button.id = id
	button.innerHTML = name
	button.className = "tooltip"
	button.onclick = function() {toggle(button)}
	button.style.backgroundColor = "#33a151"

	var tooltip = document.createElement("span")
	tooltip.className = "tooltiptext"
	button.appendChild(tooltip)

	var row = document.createElement("tr")
	var col = document.createElement("th")
	document.getElementById("table").appendChild(row)
	row.appendChild(col)
	col.appendChild(button)

	var col2 = document.createElement("th")
	var slider = document.createElement("input")
	slider.type = "range"
	slider.min = 0
	slider.max = 20
	slider.value = 0

	if (name == "Edge") {
		slider.oninput = function() {setSpeed(id, "1")}
		slider.id = id + "speed1"
	} else {
		slider.oninput = function() {setSpeed(id, "")}
		slider.id = id + "speed"
	}

	col2.appendChild(slider)
	row.appendChild(col2)

	if (name == "Edge") {
		var col3 = document.createElement("th")
		var slider = document.createElement("input")
		slider.type = "range"
		slider.min = 0
		slider.max = 10
		slider.value = 0
		slider.id = id + "speed2"
		slider.oninput = function() {setSpeed(id, "2")}
		col3.appendChild(slider)
		row.appendChild(col3)
	}

	if (name == "Nora") {
		var col3 = document.createElement("th")
		var button2 = document.createElement("button")
		button2.innerHTML = "Rotate"
		button2.onclick = function() {rotateNora(id)}
		button2.style.backgroundColor = "#33a151"
		col3.appendChild(button2)
		row.appendChild(col3)
	}

	if (name == "Max") {
		var col3 = document.createElement("th")
		var slider = document.createElement("input")
		slider.type = "range"
		slider.min = 0
		slider.max = 3
		slider.value = 0
		slider.id = id + "air"
		slider.oninput = function() {airMax(id)}
		col3.appendChild(slider)
		row.appendChild(col3)
	}

	fixToolTip(id)
}

function setSpeed(id, vibrator) {
	var speed = document.getElementById(id + "speed" + vibrator).value
	var toy = myToys.get(id)
	lovense.sendCommand(`Vibrate${vibrator}`,{v:speed, t:id})
}

function rotateNora(id) {
	var toy = myToys.get(id)
	lovense.sendCommand(`RotateChange`,{t:id})
}

function airMax(id) {
	var speed = document.getElementById(id + "air").value
	var toy = myToys.get(id)
	lovense.sendCommand(`AirAuto`,{v:speed, t:id})
}

var id = 1000
function addTest(name) {
  if (name == undefined) name = "Hush"
  addToy({"id":id++,"name":name,"status":1})
}

//pls don't spam this webhook :) it just notifies me when my server crashed
function sendWebhook() {
  var timeout = localStorage.getItem("alertTimeout")
  if (timeout && new Date().getTime() < timeout) return

  localStorage.setItem("alertTimeout", new Date().getTime() + 600000) //timeout: 10 mins

  var request = new XMLHttpRequest()
  request.open("POST", "https://discord.com/api/webhooks/882389927122305044/fC0P-i3YyASroB8OOOeFZCPCIMkL8SOnpraZlmkGJ8pYatWxTsxr0IbDzS4zhGZA5uWB")
  request.setRequestHeader('Content-type', 'application/json')
  request.send(JSON.stringify({content: "Someone failed to connect to the server.."}))
}
