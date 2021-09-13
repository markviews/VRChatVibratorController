using MelonLoader;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Vibrator_Controller {
    class Client {

        internal static string currentlyConnectedCode;
        private static ClientWebSocket webSocket;
        internal static int autoReconnectTries = 0;

        //Source https://codereview.stackexchange.com/questions/41591/websockets-client-code-and-making-it-production-ready
        internal static async void setupClient(string msg) {
            if (autoReconnectTries >= 5) {
                currentlyConnectedCode = null;
                MelonLogger.Warning("Failed to connect to server 5 times... Press AddToy and type code to try again.");
                return;
            }
            autoReconnectTries++;

            try {
                webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(new Uri("wss://control.markstuff.net:8080"), CancellationToken.None);
                await send(msg);
                await Receive(webSocket);
            } catch (Exception) {
                MelonLogger.Warning("Disconnected from server");
            }
        }

        internal static async Task send(string msg) {
            if (webSocket == null || webSocket.State != WebSocketState.Open) {
                if (currentlyConnectedCode != null) setupClient("join " + currentlyConnectedCode);
                return;
            }

            await webSocket.SendAsync(new ArraySegment<byte>(Encoding.Unicode.GetBytes(msg)), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private static async Task Receive(ClientWebSocket webSocket) {
            byte[] buffer = new byte[256];
            while (webSocket.State == WebSocketState.Open) {
                Array.Clear(buffer, 0, buffer.Length);
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close) {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                } else {
                    VibratorController.message(Encoding.ASCII.GetString(buffer));
                }
            }
        }

    }
}
