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

        //Source https://codereview.stackexchange.com/questions/41591/websockets-client-code-and-making-it-production-ready
        internal static async void setupClient(string msg) {
            try {
                webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(new Uri("wss://control.markstuff.net:8080"), CancellationToken.None);
                await send(msg);
                MelonLogger.Msg("Connected to server");
                await Receive(webSocket);
            } catch (Exception) {
                MelonLogger.Warning("Disconnected from server");
                webSocket = null;
            }
        }

        internal static async Task send(string msg) {
            if (webSocket == null) {
                if (currentlyConnectedCode != null) setupClient("join " + currentlyConnectedCode);
                return;
            }

            if (webSocket.State == WebSocketState.Open)
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
